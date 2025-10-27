using BankingEventSourcing.Application.Interfaces;
using BankingEventSourcing.Infrastructure.EventStore;
using BankingEventSourcing.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingEventSourcing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool useInMemory = false)
    {
        if (useInMemory)
        {
            services.AddSingleton<IEventStore, InMemoryEventStore>();
        }
        else
        {
            services.AddMartenEventStore(configuration);
            services.AddScoped<IEventStore, MartenEventStore>();
        }

        return services;
    }
}
