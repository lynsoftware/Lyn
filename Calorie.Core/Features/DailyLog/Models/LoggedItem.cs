using Calorie.Core.Common.Enums;

namespace Calorie.Core.Features.DailyLog.Models;

/// <summary>
/// Én rad i dagsloggen — visningsmodell for en loggført matvare/måltid.
/// Id og MealType peker tilbake til LogEntryRecord for redigering/sletting.
/// </summary>
public class LoggedItem
{
    public Guid Id { get; set; }
    public MealType MealType { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal AmountGrams { get; set; }
    public int Calories { get; set; }
}
