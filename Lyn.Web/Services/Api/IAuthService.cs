using Lyn.Backend.Models.Enums;
using Lyn.Shared.Models;
using Lyn.Shared.Result;
using Microsoft.AspNetCore.Components.Forms;

namespace Lyn.Web.Services.Api;

public interface IAuthService
{
    /// <summary>
    /// API-kall med LoginRequest som setter token og endrer brukerens autentiseringsstate
    /// og returnerer en token"/>
    /// </summary>
    /// <param name="request">Email og Password</param>
    /// <param name="cancellationToken"></param>
    /// <returns>SignupResponse - userid, name, email og token</returns>
    Task<Result> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<Result<string>> UploadFileAsync(
        IBrowserFile file,
        string version,
        DownloadPlatform platform,
        CancellationToken cancellationToken);
}