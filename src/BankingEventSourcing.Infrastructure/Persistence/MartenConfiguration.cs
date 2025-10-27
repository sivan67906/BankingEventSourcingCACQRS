using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingEventSourcing.Infrastructure.Persistence;

public static class MartenConfiguration
{
    public static IServiceCollection AddMartenEventStore(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("EventStore");

        services.AddMarten(options =>
        {
            options.Connection(connectionString ?? "Host=localhost;Database=BankingES;Username=postgres;Password=postgres");
            options.UseDefaultSerialization(serializerType: SerializerType.SystemTextJson);
            options.Events.StreamIdentity = StreamIdentity.AsString;
        }).UseLightweightSessions();

        return services;
    }
}
