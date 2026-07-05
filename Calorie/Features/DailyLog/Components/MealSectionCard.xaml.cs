using Calorie.Core.Features.DailyLog;
using Calorie.Core.Common;
using Calorie.Core.Resources.Strings;
using Calorie.Infrastructure.Services;

namespace Calorie.Features.DailyLog.Components;

public partial class MealSectionCard : ContentView
{
    public event EventHandler? AddClicked;

    public MealSectionCard()
    {
        InitializeComponent();
        AddButton.Text = $"+ {AppResources.AddFood}";
    }

    /// <summary>
    /// Fyller kortet med en måltidsseksjon: header, loggrader og rest mot budsjett.
    /// </summary>
    public void SetSection(MealSection section)
    {
        TitleLabel.Text = section.MealType.ToDisplayName();

        // Uten budsjett vises kun spist; med budsjett vises spist/maks + igjen
        BudgetLabel.Text = section.MaxCalories.HasValue
            ? $"{section.EatenCalories} / {section.MaxCalories} kcal"
            : $"{section.EatenCalories} kcal";

        if (section.RemainingCalories is { } remaining)
        {
            RemainingLabel.Text = $"{remaining} {AppResources.Remaining}";
            RemainingLabel.TextColor = remaining < 0
                ? ThemeService.GetColor("OverLimit")
                : ThemeService.GetColor("TextMuted");
        }
        else
        {
            RemainingLabel.IsVisible = false;
        }

        Separator.IsVisible = section.Items.Count > 0;

        ItemsContainer.Children.Clear();
        foreach (var item in section.Items)
            ItemsContainer.Children.Add(CreateItemRow(item));
    }

    /// <summary>
    /// Én loggrad: navn + gram til venstre, kcal til høyre.
    /// </summary>
    private static Grid CreateItemRow(LoggedItem item)
    {
        var row = new Grid
        {
            ColumnDefinitions =
            [
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            ]
        };

        var nameStack = new VerticalStackLayout { Spacing = 0 };
        nameStack.Children.Add(new Label
        {
            Text = item.Name,
            FontSize = 15,
            TextColor = ThemeService.GetColor("TextPrimary")
        });
        nameStack.Children.Add(new Label
        {
            Text = $"{item.AmountGrams:0.#} g",
            FontSize = 12,
            TextColor = ThemeService.GetColor("TextMuted")
        });
        row.Add(nameStack, 0);

        row.Add(new Label
        {
            Text = $"{item.Calories} kcal",
            FontSize = 14,
            TextColor = ThemeService.GetColor("TextSecondary"),
            VerticalOptions = LayoutOptions.Center
        }, 1);

        return row;
    }

    private void OnAddClicked(object sender, EventArgs e) => AddClicked?.Invoke(this, e);
}
