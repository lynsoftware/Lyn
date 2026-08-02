using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.DailyLog.Models;
using Calorie.Core.Features.DailyLog.Services;
using Calorie.Core.Features.MainPage.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Calorie.Core.Tests.Features.MainPage;

/// <summary>
/// Tester hovedsidens ViewModel med mocket dagslogg-store — ingen database.
/// FakeTimeProvider styrer "i dag" (dato-navigasjonens fremtidssperre).
/// </summary>
public class MainPageViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigation = new();
    private readonly Mock<IToastService> _mockToast = new();
    private readonly Mock<IDayLogStore> _mockDayLog = new();

    // Lørdag 26. juli 2026 — FakeTimeProvider bruker UTC som lokal sone
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 7, 26, 12, 0, 0, TimeSpan.Zero));

    private static readonly DateOnly Today = new(2026, 7, 26);

    public MainPageViewModelTests()
    {
        // Standard: hver dato gir en tom dag — enkelttester overstyrer ved behov
        _mockDayLog.Setup(d => d.GetDayAsync(It.IsAny<DateOnly>()))
            .ReturnsAsync((DateOnly date) => new DayLog { Date = date });
    }

    private MainPageViewModel CreateSut() =>
        new(_mockNavigation.Object, _mockToast.Object, _mockDayLog.Object, _time);

    private static LoggedItem MakeItem(
        decimal amountGrams = 200, int calories = 400, MealType mealType = MealType.Dinner) => new()
    {
        Id = Guid.NewGuid(),
        MealType = mealType,
        Name = "Kyllingsuppe",
        AmountGrams = amountGrams,
        Calories = calories
    };

    // ===== Dato-navigasjon =====

    [Fact]
    public void Constructor_WhenCreated_ShouldStartOnTodayWithNextDayBlocked()
    {
        // Arrange + Act
        var sut = CreateSut();

        // Assert — fremtiden er sperret fra dag én
        sut.CurrentDate.Should().Be(Today);
        sut.NextDayCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task PreviousDay_WhenExecuted_ShouldGoBackOneDayAndLoadIt()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.PreviousDayCommand.ExecuteAsync(null);

        // Assert — dagen lastes og fremover-pilen låses opp
        sut.CurrentDate.Should().Be(Today.AddDays(-1));
        sut.Day!.Date.Should().Be(Today.AddDays(-1));
        sut.NextDayCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task NextDay_WhenBackOnToday_ShouldBlockFurtherForwardNavigation()
    {
        // Arrange
        var sut = CreateSut();
        await sut.PreviousDayCommand.ExecuteAsync(null);

        // Act — frem igjen til i dag
        await sut.NextDayCommand.ExecuteAsync(null);

        // Assert
        sut.CurrentDate.Should().Be(Today);
        sut.NextDayCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task GoToDate_WhenDateChosen_ShouldLoadThatDayAndCloseEditPanel()
    {
        // Arrange — åpent redigeringspanel skal ikke overleve datobytte
        var sut = CreateSut();
        sut.EditEntryCommand.Execute(MakeItem());
        var target = Today.AddDays(-5);

        // Act
        await sut.GoToDateCommand.ExecuteAsync(target);

        // Assert
        sut.CurrentDate.Should().Be(target);
        sut.EditingItem.Should().BeNull();
        _mockDayLog.Verify(d => d.GetDayAsync(target), Times.Once);
    }

    // ===== Seksjonsfiltrering =====

    [Fact]
    public async Task LoadDay_ShouldShowOnlySectionsWithContentOrBudget_InDayOrder()
    {
        // Arrange — Dinner har innhold, Breakfast har kun budsjett,
        // Supper er tom uten budsjett (skal skjules)
        _mockDayLog.Setup(d => d.GetDayAsync(Today)).ReturnsAsync(new DayLog
        {
            Date = Today,
            Sections =
            [
                new MealSection { MealType = MealType.Dinner, Items = [MakeItem()] },
                new MealSection { MealType = MealType.Breakfast, MaxCalories = 500 },
                new MealSection { MealType = MealType.Supper }
            ]
        });

        var sut = CreateSut();

        // Act
        await sut.LoadDayCommand.ExecuteAsync(null);

        // Assert — naturlig dagsrekkefølge, ikke lagringsrekkefølge
        sut.VisibleSections.Select(s => s.MealType)
            .Should().Equal(MealType.Breakfast, MealType.Dinner);
        sut.IsDayEmpty.Should().BeFalse();
    }

    // ===== Tomtilstand =====

    [Fact]
    public void IsDayEmpty_BeforeFirstLoad_ShouldBeFalse()
    {
        // Arrange + Act — meldingen skal ikke blinke før dagen er lastet
        var sut = CreateSut();

        // Assert
        sut.IsDayEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task IsDayEmpty_WhenLoadedDayHasNoSections_ShouldBeTrue()
    {
        // Arrange — standardmocken gir en tom dag
        var sut = CreateSut();

        // Act
        await sut.LoadDayCommand.ExecuteAsync(null);

        // Assert
        sut.IsDayEmpty.Should().BeTrue();
    }

    // ===== Redigeringspanelet =====

    [Fact]
    public void EditEntry_WhenRowTapped_ShouldOpenPanelWithItemValues()
    {
        // Arrange
        var sut = CreateSut();
        var item = MakeItem(amountGrams: 120, mealType: MealType.Lunch);

        // Act
        sut.EditEntryCommand.Execute(item);

        // Assert
        sut.IsEditPanelVisible.Should().BeTrue();
        sut.EditItemName.Should().Be("Kyllingsuppe");
        sut.EditAmountText.Should().Be("120");
        sut.EditMealTypeOptions.Single(o => o.IsSelected).Value.Should().Be(MealType.Lunch);
    }

    [Fact]
    public void EditKcalPreview_WhenAmountChanges_ShouldScaleLinearly()
    {
        // Arrange — snapshotet er lineært i gram: 200 g = 400 kcal
        var sut = CreateSut();
        sut.EditEntryCommand.Execute(MakeItem(amountGrams: 200, calories: 400));

        // Act + Assert — halv mengde gir halve kaloriene
        sut.EditAmountText = "100";
        sut.EditKcalPreview.Should().Be("200 kcal");

        // Ugyldig mengde gir tom preview
        sut.EditAmountText = "abc";
        sut.EditKcalPreview.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveEntry_WhenValid_ShouldUpdateStoreCloseAndReload()
    {
        // Arrange — endre både mengde og måltidstype
        var sut = CreateSut();
        var item = MakeItem(amountGrams: 200, mealType: MealType.Dinner);
        sut.EditEntryCommand.Execute(item);

        sut.EditAmountText = "150";
        sut.SelectEditMealTypeCommand.Execute(
            sut.EditMealTypeOptions.Single(o => o.Value == MealType.Lunch));

        // Act
        await sut.SaveEntryCommand.ExecuteAsync(null);

        // Assert
        _mockDayLog.Verify(d => d.UpdateEntryAsync(item.Id, 150m, MealType.Lunch), Times.Once);
        sut.EditingItem.Should().BeNull();
        _mockToast.Verify(t => t.ShowSuccessAsync(It.IsAny<string>()), Times.Once);
        _mockDayLog.Verify(d => d.GetDayAsync(Today), Times.Once);   // gjenlasting
    }

    [Fact]
    public async Task SaveEntry_WhenAmountInvalid_ShouldDoNothing()
    {
        // Arrange
        var sut = CreateSut();
        sut.EditEntryCommand.Execute(MakeItem());
        sut.EditAmountText = "0";

        // Act
        await sut.SaveEntryCommand.ExecuteAsync(null);

        // Assert — panelet står åpent så brukeren kan rette
        _mockDayLog.Verify(
            d => d.UpdateEntryAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<MealType>()),
            Times.Never);
        sut.EditingItem.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteEntry_WhenExecuted_ShouldDeleteCloseAndReload()
    {
        // Arrange
        var sut = CreateSut();
        var item = MakeItem();
        sut.EditEntryCommand.Execute(item);

        // Act
        await sut.DeleteEntryCommand.ExecuteAsync(null);

        // Assert
        _mockDayLog.Verify(d => d.DeleteEntryAsync(item.Id), Times.Once);
        sut.EditingItem.Should().BeNull();
        _mockDayLog.Verify(d => d.GetDayAsync(Today), Times.Once);
    }

    // ===== Logge-flyten =====

    [Fact]
    public async Task OpenLogFlow_WhenOnPastDay_ShouldPassViewedDate()
    {
        // Arrange — "what you see is what you log"
        var sut = CreateSut();
        await sut.PreviousDayCommand.ExecuteAsync(null);

        // Act
        await sut.OpenLogFlowCommand.ExecuteAsync(MealType.Dinner);

        // Assert
        _mockNavigation.Verify(
            n => n.GoToAddLogEntryAsync(MealType.Dinner, Today.AddDays(-1)),
            Times.Once);
    }
}
