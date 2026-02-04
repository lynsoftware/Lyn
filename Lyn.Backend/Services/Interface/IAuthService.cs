using Lyn.Shared.Models.Request;
using Lyn.Shared.Result;

namespace Lyn.Backend.Services.Interface;

public interface IAuthService
{
    Task<Result<string>> LoginAsync(LoginRequest request);
}