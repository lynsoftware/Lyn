using Lyn.Backend.Apps.Calorie.Sync.DTOs.Requests;
using Lyn.Backend.Apps.Calorie.Sync.Services;
using Lyn.Backend.Common.Controllers;
using Lyn.Backend.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lyn.Backend.Apps.Calorie.Sync.Controllers;

/// <summary>
/// Sync-endepunktene for Calorie (lokal-først: klienten eier dataene,
/// skyen er replika). Alt krever innlogget bruker — eieren hentes fra
/// JWT (sub), aldri fra requesten. Push og sletting kommer i neste steg.
/// </summary>
[Authorize]
[Route("api/calorie/[controller]")]
public class SyncController(ISyncService syncService) : BaseController
{
    /// <summary>
    /// Full state for innlogget bruker — hentes ved innlogging/ny enhet
    /// og erstatter lokal database i sin helhet.
    /// </summary>
    [HttpGet("state")]
    public async Task<IActionResult> GetState(CancellationToken ct)
    {
        var result = await syncService.GetStateAsync(User.GetUserId(), ct);

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok(result.Value);
    }
    
    /// <summary>
    /// Sender alt i utboksen fra klienten slik at serveren kan synke dataen fra klienten ved å opprette nye
    /// entiteter, oppdatere eksisterende eller slette entiteter
    /// </summary>
    [HttpPost("push")]
    public async Task<IActionResult> Push([FromBody] SyncPushRequest syncPushRequest, CancellationToken ct)
    {
        var result = await syncService.PushAsync(User.GetUserId(), syncPushRequest, ct);

        if (result.IsFailure)
            return HandleFailure(result);

        return Ok();
    }
    
    /// <summary>
    /// Sletter ALT synkronisert data tilhørende brukeren, fra serveren
    /// </summary>
    [HttpDelete("data")]
    public async Task<IActionResult> DeleteAllData(CancellationToken ct)
    {
        var result = await syncService.DeleteAllDataAsync(User.GetUserId(), ct);

        if (result.IsFailure)
            return HandleFailure(result);

        return NoContent();
    }
}
