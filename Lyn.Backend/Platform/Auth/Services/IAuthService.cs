using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;

namespace Lyn.Backend.Platform.Auth.Services;

public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginRequest request);
}