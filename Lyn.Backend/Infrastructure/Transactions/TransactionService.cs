using System.Runtime.CompilerServices;
using Lyn.Backend.Infrastructure.Persistence;
using Lyn.Shared.Result;

namespace Lyn.Backend.Infrastructure.Transactions;

public class TransactionService(AppDbContext context, ILogger<TransactionService> logger) : ITransactionService
{   
    /// <inheritdoc/>
    public async Task<Result<T>> ExecuteAsync<T>(Func<CancellationToken, Task<Result<T>>> operation, 
        CancellationToken ct = default, [CallerMemberName] string callerName = "")
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await operation(ct);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction failed in {Caller}. Rolling back", callerName);
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Result> ExecuteAsync(Func<CancellationToken, Task<Result>> operation, 
        CancellationToken ct = default, [CallerMemberName] string callerName = "")
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            var result = await operation(ct);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }
            await transaction.CommitAsync(ct);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Transaction failed in {Caller}. Rolling back", callerName);
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
