using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Calorie.Core.Data;

/// <summary>
/// Design-time factory for dotnet ef (migrasjonsgenerering). Brukes aldri
/// ved kjøring — appen initialiserer via LocalDb med ekte enhetssti.
/// </summary>
internal sealed class LocalDbContextFactory : IDesignTimeDbContextFactory<LocalDbContext>
{
    public LocalDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LocalDbContext>()
            .UseSqlite("Data Source=design-time.db3")
            .Options;

        return new LocalDbContext(options);
    }
}
