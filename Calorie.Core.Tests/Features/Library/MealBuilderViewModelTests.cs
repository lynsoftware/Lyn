using Calorie.Core.Common;
using Calorie.Core.Common.Constants;
using Calorie.Core.Common.Services;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.Services;
using Calorie.Core.Features.Library.ViewModels;
using FluentAssertions;
using Moq;

namespace Calorie.Core.Tests.Features.Library;

/// <summary>
/// Tester måltidsbyggerens ViewModel med mockede stores — ingen database.
/// Dekker gram/stykk-utregning, utflating av måltider og Tilpass vs. lagre.
/// </summary>
public class MealBuilderViewModelTests
{
    private readonly Mock<INavigationService> _mockNavigation = new();
    private readonly Mock<IToastService> _mockToast = new();
    private readonly Mock<ILibraryStore> _mockLibrary = new();

    private MealBuilderViewModel CreateSut() =>
        new(_mockNavigation.Object, _mockToast.Object, _mockLibrary.Object);

    private static LibraryIngredient MakeIngredient(
        string name = "Egg",
        decimal caloriesPer100g = 150,
        string? unitName = null,
        decimal? unitWeightGrams = null) => new()
    {
        Name = name,
        CaloriesPer100g = caloriesPer100g,
        UnitName = unitName,
        UnitWeightGrams = unitWeightGrams
    };

    // ===== Gram/stykk-utregning =====

    [Fact]
    public void AddComponent_WhenGramMode_ShouldUseGramsDirectly()
    {
        // Arrange
        var sut = CreateSut();
        sut.SelectedIngredient = MakeIngredient();
        sut.AmountText = "150";

        // Act
        sut.AddComponentCommand.Execute(null);

        // Assert
        sut.ComponentRows.Should().ContainSingle()
            .Which.Component.AmountGrams.Should().Be(150m);
    }

    [Fact]
    public void AddComponent_WhenUnitMode_ShouldConvertUnitsToGrams()
    {
        // Arrange — "2 stk" à 60 g
        var sut = CreateSut();
        sut.SelectedIngredient = MakeIngredient(unitName: "stk", unitWeightGrams: 60);
        sut.ToggleUnitCommand.Execute(null);
        sut.AmountText = "2";

        // Act
        sut.AddComponentCommand.Execute(null);

        // Assert — lagres alltid i gram; raden viser begge
        var row = sut.ComponentRows.Should().ContainSingle().Subject;
        row.Component.AmountGrams.Should().Be(120m);
        row.AmountText.Should().Be("2 stk · 120 g");
    }

    [Fact]
    public void AddComponent_WhenUnitModeWithoutAmount_ShouldDefaultToOneUnit()
    {
        // Arrange
        var sut = CreateSut();
        sut.SelectedIngredient = MakeIngredient(unitName: "stk", unitWeightGrams: 60);
        sut.ToggleUnitCommand.Execute(null);
        sut.AmountText = string.Empty;

        // Act
        sut.AddComponentCommand.Execute(null);

        // Assert
        sut.ComponentRows.Should().ContainSingle()
            .Which.Component.AmountGrams.Should().Be(60m);
    }

    [Fact]
    public void ToggleUnit_WhenIngredientHasNoUnit_ShouldStayInGramMode()
    {
        // Arrange
        var sut = CreateSut();
        sut.SelectedIngredient = MakeIngredient();

        // Act
        sut.ToggleUnitCommand.Execute(null);

        // Assert
        sut.UseUnits.Should().BeFalse();
    }

    // ===== Utflating av måltid (kilde-chips: Måltider) =====

    [Fact]
    public void AddComponent_WhenMealSource_ShouldFlattenScaledToChosenAmount()
    {
        // Arrange — kilde på 400 g totalt
        var sut = CreateSut();
        var source = new LibraryMeal
        {
            Name = "Kylling og ris",
            Components =
            [
                new MealComponent { Ingredient = MakeIngredient("Kylling"), AmountGrams = 100 },
                new MealComponent { Ingredient = MakeIngredient("Ris"), AmountGrams = 300 }
            ]
        };

        sut.SelectMealSourceCommand.Execute(null);
        sut.SelectedMeal = source;

        // Valg av måltid forhåndsfyller mengden med kildens totalvekt
        sut.AmountText.Should().Be("400");

        // Act — halv porsjon: faktor 0,5 på hver komponent
        sut.AmountText = "200";
        sut.AddComponentCommand.Execute(null);

        // Assert — flatet ut til ingrediens-rader, ingen måltid-referanse
        sut.ComponentRows.Should().HaveCount(2);
        sut.ComponentRows.Select(r => r.Name).Should().Equal("Kylling", "Ris");
        sut.ComponentRows[0].Component.AmountGrams.Should().Be(50m);
        sut.ComponentRows[1].Component.AmountGrams.Should().Be(150m);
    }

    // ===== Lagre vs. Tilpass =====

    [Fact]
    public async Task Save_WhenNewMeal_ShouldSaveToLibraryAndReturnCreatedId()
    {
        // Arrange
        var sut = CreateSut();
        sut.Name = "Omelett";
        sut.SelectedIngredient = MakeIngredient();
        sut.AmountText = "120";
        sut.AddComponentCommand.Execute(null);

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockLibrary.Verify(l => l.SaveMealAsync(It.Is<LibraryMeal>(m =>
            m.Name == "Omelett" && m.Components.Count == 1)), Times.Once);
        _mockNavigation.Verify(n => n.GoBackAsync(NavKeys.CreatedItemId, It.IsAny<object>()), Times.Once);
        _mockToast.Verify(t => t.ShowSuccessAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Save_WhenNameEmpty_ShouldDoNothing()
    {
        // Arrange
        var sut = CreateSut();
        sut.SelectedIngredient = MakeIngredient();
        sut.AmountText = "100";
        sut.AddComponentCommand.Execute(null);

        // Act — navnet mangler
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        _mockLibrary.Verify(l => l.SaveMealAsync(It.IsAny<LibraryMeal>()), Times.Never);
        _mockNavigation.Verify(n => n.GoBackAsync(), Times.Never);
        _mockNavigation.Verify(n => n.GoBackAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
    }

    [Fact]
    public async Task Save_WhenCustomizeMode_ShouldReturnCopyWithoutTouchingTemplate()
    {
        // Arrange
        var sut = CreateSut();
        var template = new LibraryMeal
        {
            Name = "Taco",
            Components = [new MealComponent { Ingredient = MakeIngredient("Kjøttdeig"), AmountGrams = 200 }]
        };
        sut.LoadForCustomize(template);

        object? returnedResult = null;
        _mockNavigation.Setup(n => n.GoBackAsync(NavKeys.MealCustomized, It.IsAny<object>()))
            .Callback<string, object>((_, value) => returnedResult = value)
            .Returns(Task.CompletedTask);

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert — malen røres ikke; resultatet er en engangs-kopi
        _mockLibrary.Verify(l => l.SaveMealAsync(It.IsAny<LibraryMeal>()), Times.Never);

        var transient = returnedResult.Should().BeOfType<LibraryMeal>().Subject;
        transient.Should().NotBeSameAs(template);
        transient.Components.Should().ContainSingle()
            .Which.AmountGrams.Should().Be(200m);
    }

    [Fact]
    public async Task UpdateTemplate_WhenCustomizeMode_ShouldSaveTemplateAndReturnIt()
    {
        // Arrange — brukerens eksplisitte "Oppdater også malen"-valg
        var sut = CreateSut();
        var template = new LibraryMeal
        {
            Name = "Taco",
            Components = [new MealComponent { Ingredient = MakeIngredient("Kjøttdeig"), AmountGrams = 200 }]
        };
        sut.LoadForCustomize(template);

        // Act
        await sut.UpdateTemplateCommand.ExecuteAsync(null);

        // Assert
        _mockLibrary.Verify(l => l.SaveMealAsync(template), Times.Once);
        _mockNavigation.Verify(n => n.GoBackAsync(NavKeys.MealCustomized, template), Times.Once);
    }

    // ===== Manuell modus (ferdigrett) =====

    [Fact]
    public async Task Save_WhenManualPerPackage_ShouldConvertToPer100gAndWrapInMeal()
    {
        // Arrange — etiketten oppgir hele pakken: 900 kcal / 450 g
        var sut = CreateSut();
        sut.SelectManualModeCommand.Execute(null);
        sut.SelectPerPackageCommand.Execute(null);
        sut.Name = "Lasagne";
        sut.PackageWeightText = "450";
        sut.ManualCaloriesText = "900";

        sut.ManualTotalPreview.Should().Contain("900 kcal").And.Contain("450 g");

        LibraryIngredient? savedIngredient = null;
        _mockLibrary.Setup(l => l.SaveIngredientAsync(It.IsAny<LibraryIngredient>()))
            .Callback<LibraryIngredient>(i => savedIngredient = i)
            .Returns(Task.CompletedTask);

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert — per pakke regnes om til per 100 g (900 x 100/450 ≈ 200;
        // desimal-divisjonen terminerer ikke, derfor tilnærming), og
        // ingrediensen pakkes inn i et ett-komponents måltid (ReadyMeal-mønsteret)
        savedIngredient.Should().NotBeNull();
        savedIngredient!.CaloriesPer100g.Should().BeApproximately(200m, 0.001m);
        savedIngredient.UnitWeightGrams.Should().Be(450m);
        _mockLibrary.Verify(l => l.SaveMealAsync(It.Is<LibraryMeal>(m =>
            m.Components.Count == 1 && m.Components[0].AmountGrams == 450m)), Times.Once);
        _mockNavigation.Verify(n => n.GoBackAsync(NavKeys.CreatedItemId, It.IsAny<object>()), Times.Once);
    }
}
