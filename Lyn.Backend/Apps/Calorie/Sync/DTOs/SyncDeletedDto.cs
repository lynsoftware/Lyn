namespace Lyn.Backend.Apps.Calorie.Sync.DTOs;

/// <summary>
/// Tombstones i en push: id-ene klienten har slettet siden sist. Kun
/// rot-entiteter — komponenter og budsjetter reiser alltid med forelderen
/// sin og erstattes i sin helhet, så de trenger aldri egne tombstones.
/// </summary>
public class SyncDeletedDto
{
    public List<Guid> IngredientIds { get; set; } = [];

    public List<Guid> MealIds { get; set; } = [];

    public List<Guid> LogEntryIds { get; set; } = [];

    public List<Guid> GoalIds { get; set; } = [];
}
