using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MOps.Infrastructure.Persistence;

/// <summary>
/// Usada apenas em design-time (dotnet ef). A connection string é fictícia e não é usada para conectar.
/// </summary>
public class MOpsDbContextFactory : IDesignTimeDbContextFactory<MOpsDbContext>
{
    public MOpsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MOpsDbContext>()
            .UseNpgsql("Host=localhost;Database=mops_design;Username=design;Password=design",
                npgsql => npgsql.MigrationsAssembly(typeof(MOpsDbContext).Assembly.FullName))
            .Options;

        return new MOpsDbContext(options);
    }
}
