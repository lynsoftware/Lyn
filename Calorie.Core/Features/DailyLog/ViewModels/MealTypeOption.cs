using Calorie.Core.Common;
using Calorie.Core.Common.Enums;
using CommunityToolkit.Mvvm.ComponentModel;
namespace Calorie.Core.Features.DailyLog.ViewModels;

/// <summary>
/// Én måltidstype-chip i logge-flyten. IsSelected er observerbar slik at
/// chipens DataTrigger kan bytte farge når valget endres.
/// </summary>
public partial class MealTypeOption(MealType value, string label) : ObservableObject
{
    public MealType Value { get; } = value;

    public string Label { get; } = label;

    [ObservableProperty] 
    private bool _isSelected;
}