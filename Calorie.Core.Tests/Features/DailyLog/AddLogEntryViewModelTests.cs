using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.DailyLog;
using Calorie.Core.Features.DailyLog.Services;
using Calorie.Core.Features.DailyLog.ViewModels;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.Services;
using Calorie.Core.Features.Library.ViewModels;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Moq;

namespace Calorie.Core.Tests.Features.DailyLog;

/// <summary>
/// Tester logge-flytens ViewModel med mockede stores — ingen database.
/// FakeTimeProvider styrer klokka (måltidsforslag og "i dag"-logikk).
/// </summary>
public class AddLogEntryViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigation = new();
    private readonly Mock<IToastService> _mockToast = new();
    private readonly Mock<ILibraryStore> _mockLibrary = new();
    private readonly Mock<IDayLogStore> _mockDayLog = new();

    // Lørdag 26. juli 2026 kl. 12:00 — FakeTimeProvider bruker UTC som lokal sone
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 7, 26, 12, 0, 0, TimeSpan.Zero));

    public AddLogEntryViewModelTests()
    {
        _mockLibrary.Setup(l => l.GetMealsAsync()).ReturnsAsync(new List<LibraryMeal>());
        _mockLibrary.Setup(l => l.GetIngredientsAsync()).ReturnsAsync(new List<LibraryIngredient>());
    }

    private AddLogEntryViewModel CreateSut() =>
        new(_mockNavigation.Object, _mockToast.Object, _mockLibrary.Object, _mockDayLog.Object, _time);

    // ===== Måltidsforslag fra klokka =====

    [Theory]
    [InlineData(8, MealType.Breakfast)]
    [InlineData(12, MealType.Lunch)]
    [InlineData(15, MealType.Snack)]
    [InlineData(19, MealType.Dinner)]
    [InlineData(22, MealType.Supper)]
    public void Constructor_WhenCreatedAtGivenHour_ShouldSuggestMealTypeFromClock(int hour, MealType expected)
    {
        // Arrange — egen klokke per case (SetUtcNow kan ikke stilles bakover)
        var time = new FakeTimeProvider(new DateTimeOffset(2026, 7, 26, hour, 0, 0, TimeSpan.Zero));

        // Act
        var sut = new AddLogEntryViewModel(
            _mockNavigation.Object, _mockToast.Object, _mockLibrary.Object, _mockDayLog.Object, time);

        // Assert
        sut.SelectedMealType.Should().Be(expected);
    }

    // ===== ConfirmAdd-validering =====

    [Fact]
    public void ConfirmAdd_WhenNoActiveItem_ShouldBeDisabled()
    {
        // Arrange
        var sut = CreateSut();

        // Assert
        sut.ConfirmAddCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void ConfirmAdd_WhenItemSelected_ShouldEnableWithSuggestedAmount()
    {
        // Arrange
        var sut = CreateSut();

        // Act — valg av vare forhåndsfyller mengden (kroken på ActiveItem)
        sut.ActiveItem = new LibraryItem { Name = "Havregryn", DefaultAmountGrams = 40 };

        // Assert
        sut.AmountText.Should().Be("40");
        sut.ConfirmAddCommand.CanExecute(null).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("abc")]
    public void ConfirmAdd_WhenAmountInvalid_ShouldBeDisabled(string amount)
    {
        // Arrange
        var sut = CreateSut();
        sut.ActiveItem = new LibraryItem { Name = "Havregryn" };

        // Act
        sut.AmountText = amount;

        // Assert
        sut.ConfirmAddCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task ConfirmAdd_WhenConfirmed_ShouldLogItemOnLogDateAndNavigateBack()
    {
        // Arrange
        var sut = CreateSut();
        var item = new LibraryItem { Name = "Havregryn" };
        var logDate = new DateOnly(2026, 7, 20);

        sut.SetLogDate(logDate);
        sut.SetMealType(MealType.Dinner);
        sut.ActiveItem = item;
        sut.AmountText = "150";

        // Act
        await sut.ConfirmAddCommand.ExecuteAsync(null);

        // Assert — logges på den VISTE dagen, ikke i dag
        _mockDayLog.Verify(d => d.AddItemAsync(MealType.Dinner, item, 150m, logDate), Times.Once);
        _mockToast.Verify(t => t.ShowSuccessAsync(It.IsAny<string>()), Times.Once);
        _mockNavigation.Verify(n => n.GoBackAsync(), Times.Once);
    }

    [Fact]
    public async Task ConfirmAdd_WhenCommaDecimal_ShouldParseAsDecimal()
    {
        // Arrange — norsk tastatur gir komma som desimaltegn
        var sut = CreateSut();
        var item = new LibraryItem { Name = "Olivenolje" };
        sut.ActiveItem = item;
        sut.AmountText = "1,5";

        // Act
        await sut.ConfirmAddCommand.ExecuteAsync(null);

        // Assert
        _mockDayLog.Verify(
            d => d.AddItemAsync(It.IsAny<MealType>(), item, 1.5m, It.IsAny<DateOnly>()),
            Times.Once);
    }

    // ===== Forhåndsvalg av nyopprettet vare =====

    [Fact]
    public async Task LoadItems_WhenItemCreatedInEditor_ShouldSwitchTabAndPreselectIt()
    {
        // Arrange
        var created = new LibraryIngredient { Name = "Cottage cheese" };
        var other = new LibraryIngredient { Name = "Havregryn" };
        _mockLibrary.Setup(l => l.GetIngredientsAsync())
            .ReturnsAsync(new List<LibraryIngredient> { other, created });

        var sut = CreateSut();
        sut.SearchText = "cott";

        // Act — editoren melder fra om opprettet vare, deretter gjenlastes listen
        sut.NotifyItemCreated(created.Id, LibraryItemKind.Ingredient);
        await sut.LoadItemsCommand.ExecuteAsync(null);

        // Assert — fanen fulgte varetypen, søket ble nullstilt og varen er valgt
        sut.ActiveKind.Should().Be(LibraryItemKind.Ingredient);
        sut.SearchText.Should().BeEmpty();
        sut.ActiveItem.Should().NotBeNull();
        sut.ActiveItem!.Source.Should().BeSameAs(created);
    }

    [Fact]
    public async Task LoadItems_WhenNothingCreated_ShouldNotSelectAnything()
    {
        // Arrange
        _mockLibrary.Setup(l => l.GetIngredientsAsync())
            .ReturnsAsync(new List<LibraryIngredient> { new() { Name = "Havregryn" } });

        var sut = CreateSut();
        sut.SelectTabCommand.Execute(LibraryItemKind.Ingredient);

        // Act
        await sut.LoadItemsCommand.ExecuteAsync(null);

        // Assert
        sut.ActiveItem.Should().BeNull();
    }

    // ===== Søkefilter =====

    [Fact]
    public async Task SearchFilter_WhenTextSet_ShouldMatchNameAndBrandCaseInsensitive()
    {
        // Arrange
        _mockLibrary.Setup(l => l.GetMealsAsync()).ReturnsAsync(new List<LibraryMeal>
        {
            new() { Name = "Taco" },
            new() { Name = "Havregrøt", Brand = "Fjordland" },
            new() { Name = "Omelett" }
        });

        var sut = CreateSut();
        await sut.LoadItemsCommand.ExecuteAsync(null);

        // Act + Assert — navn, store/små bokstaver ignoreres
        sut.SearchText = "TACO";
        sut.FilteredItems.Should().ContainSingle(i => i.Name == "Taco");

        // Merkenavn treffer også
        sut.SearchText = "fjord";
        sut.FilteredItems.Should().ContainSingle(i => i.Name == "Havregrøt");

        // Tomt søk viser alt, alfabetisk
        sut.SearchText = string.Empty;
        sut.FilteredItems.Select(i => i.Name).Should().Equal("Havregrøt", "Omelett", "Taco");
    }
}
