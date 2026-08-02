namespace Lyn.Backend.Apps.Calorie.Sync.DTOs.Responses;

/// <summary>
/// Full state for brukeren — hentes ved innlogging/ny enhet og erstatter
/// lokal database i sin helhet (løser også sletting på tvers av enheter:
/// det som er slettet er rett og slett ikke med). Samme radformer som
/// pushen; ingen Deleted-liste — state er et øyeblikksbilde av det som finnes.
/// </summary>
public class SyncStateResponse
{
    public List<SyncIngredientDto> Ingredients { get; set; } = [];

    public List<SyncMealDto> Meals { get; set; } = [];

    public List<SyncLogEntryDto> LogEntries { get; set; } = [];

    public List<SyncUserGoalDto> Goals { get; set; } = [];
}
