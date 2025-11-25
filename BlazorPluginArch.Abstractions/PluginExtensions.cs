using Microsoft.Extensions.DependencyInjection;

namespace BlazorPluginArch.Abstractions;

/// <summary>
/// Extension methods for plugin registration.
/// </summary>
public static class PluginExtensions
{
    /// <summary>
    /// Registers plugins and creates a plugin registry from the provided plugin infos.
    /// </summary>
    public static IServiceCollection AddPlugins(
        this IServiceCollection services,
        params IEnumerable<PluginInfo>[] pluginSources)
    {
        var registry = new AggregatePluginRegistry(pluginSources.SelectMany(p => p).ToList());

        // Register the registry
        services.AddSingleton<IPluginRegistry>(registry);

        // Initialize all plugins and register their services
        foreach (var plugin in registry.Plugins)
        {
            plugin.Instance.Initialize();
            plugin.ServiceRegistrar?.RegisterServices(services);
        }

        return services;
    }
}

/// <summary>
/// A plugin registry that aggregates plugins from multiple sources.
/// </summary>
internal sealed class AggregatePluginRegistry : IPluginRegistry
{
    private readonly List<PluginInfo> _plugins;
    private readonly List<System.Reflection.Assembly> _assemblies;
    private readonly List<PluginComponentInfo> _navigationComponents;

    public AggregatePluginRegistry(List<PluginInfo> plugins)
    {
        _plugins = plugins;
        _assemblies = plugins.Select(p => p.Assembly).Distinct().ToList();
        _navigationComponents = plugins
            .SelectMany(p => p.Components)
            .Where(c => c.ShowInNavigation)
            .OrderBy(c => c.Order)
            .ToList();
    }

    public IReadOnlyList<PluginInfo> Plugins => _plugins;
    public IReadOnlyList<System.Reflection.Assembly> PluginAssemblies => _assemblies;
    public IReadOnlyList<PluginComponentInfo> NavigationComponents => _navigationComponents;

    public PluginInfo? GetPlugin(string id) => _plugins.FirstOrDefault(p => p.Id == id);
}
