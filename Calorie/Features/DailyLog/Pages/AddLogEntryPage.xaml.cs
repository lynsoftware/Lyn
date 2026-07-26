using Calorie.Core.Common;
using Calorie.Core.Common.Constants;
using Calorie.Core.Common.Enums;
using Calorie.Core.Features.DailyLog.ViewModels;
using Calorie.Core.Features.Library;
using Calorie.Core.Features.Library.Models;
using Calorie.Core.Features.Library.ViewModels;

namespace Calorie.Features.DailyLog.Pages;

public partial class AddLogEntryPage : ContentPage, IQueryAttributable
{
    private readonly AddLogEntryViewModel _vm;
    
    public AddLogEntryPage(AddLogEntryViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.LoadItemsCommand.Execute(null);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavKeys.MealType, out var mealType) && mealType is MealType type)
            _vm.SetMealType(type);

        if (query.TryGetValue(NavKeys.LogDate, out var logDate) && logDate is DateOnly date)
            _vm.SetLogDate(date);
        
        if (query.TryGetValue(NavKeys.CreatedItemId, out var created) && created is Guid id)
        {
            var kind = query.TryGetValue(NavKeys.CreatedItemKind, out var k) && k is LibraryItemKind itemKind
                ? itemKind
                : (LibraryItemKind?)null;
            
            _vm.NotifyItemCreated(id, kind);
        }

        if (query.TryGetValue(NavKeys.MealCustomized, out var meal) && meal is LibraryMeal customized)
            _vm.ApplyCustomizeMeal(customized);
        
    }
}
