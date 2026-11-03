using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MOps.Core.Repositories;
using MOps.Core.Services;
using MOps.Infrastructure.Persistence;
using MOps.Infrastructure.Persistence.Repositories;

namespace MOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

        services.AddDbContext<MOpsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(MOpsDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ISprintRepository, SprintRepository>();
        services.AddScoped<IWorkItemRepository, WorkItemRepository>();
        services.AddScoped<ICommitRecordRepository, CommitRecordRepository>();

        return services;
    }

    /// <summary>Serviços de aplicação do Core, compartilhados pela API REST e pelo MCP (SSE).</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISprintService, SprintService>();

        return services;
    }
}
