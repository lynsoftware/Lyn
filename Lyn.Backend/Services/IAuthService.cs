

using Lyn.Backend.Models;
using Lyn.Shared.Models;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services;

public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginRequest request);
}