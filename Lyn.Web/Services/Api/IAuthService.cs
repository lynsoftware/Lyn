using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;


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
}