using Microsoft.Extensions.DependencyInjection;
using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories.Fluent;

namespace AdvancedRepositories.Core;

public static class RepositoriesConfiguration
{
    public static IServiceCollection AddRepositoriesConfiguration(this IServiceCollection services, Action<BaseDatabaseConfiguration> databaseConfig)
    {
        BaseDatabaseConfiguration dbConfig = new DatabaseConfiguration();
        databaseConfig(dbConfig);

        services.AddSingleton(dbConfig);

        services.AddScoped<IFluentRepository, FluentRepository>();

        return services;
    }
}