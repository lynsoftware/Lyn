namespace Calorie.Core.Features.DailyLog;

/// <summary>
/// Én rad i dagsloggen — visningsmodell for en loggført matvare/måltid.
/// </summary>
public class LoggedItem
{
    public string Name { get; set; } = string.Empty;
    public decimal AmountGrams { get; set; }
    public int Calories { get; set; }
}
