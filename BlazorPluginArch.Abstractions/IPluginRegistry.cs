using System.Reflection;

namespace BlazorPluginArch.Abstractions;

/// <summary>
/// Registry for all discovered plugins.
/// The source generator creates an implementation of this interface.
/// </summary>
public interface IPluginRegistry
{
    /// <summary>
    /// Gets all registered plugins.
    /// </summary>
    IReadOnlyList<PluginInfo> Plugins { get; }

    /// <summary>
    /// Gets all assemblies containing plugin components (for router discovery).
    /// </summary>
    IReadOnlyList<Assembly> PluginAssemblies { get; }

    /// <summary>
    /// Gets all plugin components for navigation menu building.
    /// </summary>
    IReadOnlyList<PluginComponentInfo> NavigationComponents { get; }

    /// <summary>
    /// Gets a plugin by its ID.
    /// </summary>
    PluginInfo? GetPlugin(string id);
}
