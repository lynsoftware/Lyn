using Calorie.Core.Common.Enums;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.Goals.Models;
using Calorie.Core.Features.Goals.Services;
using Calorie.Core.Features.Goals.ViewModels;
using Calorie.Core.Resources.Strings;
using FluentAssertions;
using Moq;

namespace Calorie.Core.Tests.Features.Goals;

/// <summary>
/// Tester mål-oppsettets ViewModel med mocket goal-store — ingen database.
/// Dekker lasting, kos-prosent-preview/klamping og parsing ved lagring.
/// </summary>
public class GoalSettingsViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigation = new();
    private readonly Mock<IToastService> _mockToast = new();
    private readonly Mock<IGoalStore> _mockGoals = new();

    public GoalSettingsViewModelTests()
    {
        _mockGoals.Setup(g => g.GetCurrentAsync()).ReturnsAsync(new GoalSettings());
    }

    private GoalSettingsViewModel CreateSut() =>
        new(_mockNavigation.Object, _mockToast.Object, _mockGoals.Object);

    // ===== Lasting =====

    [Fact]
    public async Task Load_WhenFirstTime_ShouldPopulateFieldsFromStore()
    {
        // Arrange
        _mockGoals.Setup(g => g.GetCurrentAsync()).ReturnsAsync(new GoalSettings
        {
            DailyCalories = 2000,
            Mode = GoalMode.Target,
            ProteinGoalGrams = 120,
            TreatPercent = 20,
            MealBudgets = { [MealType.Breakfast] = 500 }
        });

        var sut = CreateSut();

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert — satte verdier fylles inn, usatte forblir tomme felt
        sut.DailyCaloriesText.Should().Be("2000");
        sut.IsTargetMode.Should().BeTrue();
        sut.ProteinText.Should().Be("120");
        sut.CarbsText.Should().BeEmpty();
        sut.TreatPercentText.Should().Be("20");

        sut.BudgetOptions.Single(o => o.Value == MealType.Breakfast).AmountText.Should().Be("500");
        sut.BudgetOptions.Where(o => o.Value != MealType.Breakfast)
            .Should().OnlyContain(o => o.AmountText == string.Empty);
    }

    [Fact]
    public async Task Load_WhenCalledAgain_ShouldNotOverwriteUserInput()
    {
        // Arrange — retur fra andre sider trigger LoadCommand på nytt
        var sut = CreateSut();
        await sut.LoadCommand.ExecuteAsync(null);
        sut.DailyCaloriesText = "1800";

        // Act
        await sut.LoadCommand.ExecuteAsync(null);

        // Assert
        sut.DailyCaloriesText.Should().Be("1800");
        _mockGoals.Verify(g => g.GetCurrentAsync(), Times.Once);
    }

    [Fact]
    public void BudgetOptions_ShouldExcludeTreat()
    {
        // Arrange + Act — kos-grensen settes som prosent, ikke kcal-budsjett
        var sut = CreateSut();

        // Assert
        sut.BudgetOptions.Should().NotContain(o => o.Value == MealType.Treat);
    }

    // ===== Kos-prosent =====

    [Fact]
    public void TreatKcalPreview_WhenPercentAndGoalSet_ShouldShowCalculatedKcal()
    {
        // Arrange
        var sut = CreateSut();

        // Act — 20 % av 2000 kcal
        sut.DailyCaloriesText = "2000";
        sut.TreatPercentText = "20";

        // Assert
        sut.TreatKcalPreview.Should().Be(string.Format(AppResources.TreatShareCalculated, 400));
    }

    [Theory]
    [InlineData("2000", "")]      // prosent mangler
    [InlineData("2000", "150")]   // over 100 % gir ikke mening
    [InlineData("", "20")]        // dagsmål mangler
    public void TreatKcalPreview_WhenInputIncomplete_ShouldBeEmpty(string daily, string percent)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        sut.DailyCaloriesText = daily;
        sut.TreatPercentText = percent;

        // Assert
        sut.TreatKcalPreview.Should().BeEmpty();
    }

    // ===== Lagring =====

    [Fact]
    public async Task Save_WhenValid_ShouldPersistParsedGoalWithClampedTreatPercent()
    {
        // Arrange
        GoalSettings? saved = null;
        _mockGoals.Setup(g => g.SaveAsync(It.IsAny<GoalSettings>()))
            .Callback<GoalSettings>(g => saved = g)
            .Returns(Task.CompletedTask);

        var sut = CreateSut();
        sut.DailyCaloriesText = "2000";
        sut.SelectTargetModeCommand.Execute(null);
        sut.ProteinText = "120";
        sut.TreatPercentText = "150";   // skal klampes til 100
        sut.BudgetOptions.Single(o => o.Value == MealType.Breakfast).AmountText = "500";
        sut.BudgetOptions.Single(o => o.Value == MealType.Dinner).AmountText = "abc";   // ignoreres

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        saved.Should().NotBeNull();
        saved!.DailyCalories.Should().Be(2000);
        saved.Mode.Should().Be(GoalMode.Target);
        saved.ProteinGoalGrams.Should().Be(120);
        saved.CarbsGoalGrams.Should().BeNull();
        saved.TreatPercent.Should().Be(100);
        saved.MealBudgets.Should().Equal(
            new Dictionary<MealType, int> { [MealType.Breakfast] = 500 });

        _mockToast.Verify(t => t.ShowSuccessAsync(It.IsAny<string>()), Times.Once);
        _mockNavigation.Verify(n => n.GoBackAsync(), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("abc")]
    public async Task Save_WhenDailyCaloriesInvalid_ShouldDoNothing(string daily)
    {
        // Arrange
        var sut = CreateSut();
        sut.DailyCaloriesText = daily;

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockGoals.Verify(g => g.SaveAsync(It.IsAny<GoalSettings>()), Times.Never);
        _mockNavigation.Verify(n => n.GoBackAsync(), Times.Never);
    }

    // ===== Modus-chips =====

    [Fact]
    public void SelectMode_WhenToggled_ShouldFlipModeFlags()
    {
        // Arrange — Limit er standard
        var sut = CreateSut();
        sut.IsLimitMode.Should().BeTrue();

        // Act + Assert
        sut.SelectTargetModeCommand.Execute(null);
        sut.IsTargetMode.Should().BeTrue();
        sut.IsLimitMode.Should().BeFalse();

        sut.SelectLimitModeCommand.Execute(null);
        sut.IsLimitMode.Should().BeTrue();
        sut.IsTargetMode.Should().BeFalse();
    }
}
