using Microsoft.Extensions.DependencyInjection;

namespace BlazorPluginArch.Abstractions;

/// <summary>
/// Interface for registering plugin services with the DI container.
/// Implement this in your plugin to register custom services.
/// </summary>
public interface IPluginServiceRegistrar
{
    /// <summary>
    /// Register services with the service collection.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void RegisterServices(IServiceCollection services);
}
