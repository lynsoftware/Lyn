namespace Lyn.Backend.Apps.Calorie.Sync.DTOs.Requests;

/// <summary>
/// Én push fra klienten: endrede rader (upserts, LWW via UpdatedAtUtc) og
/// tombstones — alt i én batch som anvendes i én transaksjon. Samme kontrakt
/// for outbox-drypp og engangs-full-push (når brukeren skrur på synk).
/// Tomme samlinger som default: en klient med bare én type endringer
/// trenger ikke sende de andre feltene.
/// </summary>
public class SyncPushRequest
{
    public List<SyncIngredientDto> Ingredients { get; set; } = [];

    public List<SyncMealDto> Meals { get; set; } = [];

    public List<SyncLogEntryDto> LogEntries { get; set; } = [];

    public List<SyncUserGoalDto> Goals { get; set; } = [];

    public SyncDeletedDto Deleted { get; set; } = new();
}
