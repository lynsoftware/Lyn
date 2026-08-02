using Lyn.Backend.Apps.Calorie.Sync.DTOs.Requests;
using Lyn.Backend.Apps.Calorie.Sync.DTOs.Responses;
using Lyn.Shared.Result;

namespace Lyn.Backend.Apps.Calorie.Sync.Services;

public interface ISyncService
{
    /// <summary>
    /// Anvender en push-batch fra klienten: upserts i avhengighetsrekkefølge
    /// (ingredienser → måltider → logg → mål), deretter tombstones i omvendt
    /// rekkefølge — alt i én transaksjon (LWW per rad via UpdatedAtUtc).
    /// Samme kontrakt for outbox-drypp og engangs-full-push.
    /// </summary>
    /// <param name="userId">Eieren — fra JWT-claimen, aldri fra requesten</param>
    /// <param name="request">Endrede rader + tombstones</param>
    /// <param name="ct"></param>
    /// <returns>Result — OK eller failure (hele batchen avvist)</returns>
    Task<Result> PushAsync(string userId, SyncPushRequest request, CancellationToken ct = default);

    /// <summary>
    /// Full state for brukeren — hentes ved innlogging/ny enhet og erstatter
    /// lokal database i sin helhet (dekker også sletting på tvers av enheter).
    /// </summary>
    /// <param name="userId">Eieren — fra JWT-claimen</param>
    /// <param name="ct"></param>
    /// <returns>Result med alle brukerens rader</returns>
    Task<Result<SyncStateResponse>> GetStateAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Sletter ALT brukeren har i skyen (sletteretten — brukeren skrur av
    /// synk og vil ha dataene fjernet). Lokal database på enheten røres ikke.
    /// </summary>
    /// <param name="userId">Eieren — fra JWT-claimen</param>
    /// <param name="ct"></param>
    /// <returns>Result — OK også når det ikke fantes noe å slette (idempotent)</returns>
    Task<Result> DeleteAllDataAsync(string userId, CancellationToken ct = default);
}
