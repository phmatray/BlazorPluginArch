using System.Reflection;

namespace BlazorPluginArch.Abstractions;

/// <summary>
/// Contains metadata about a registered plugin.
/// Generated at compile-time by the source generator.
/// </summary>
public sealed class PluginInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public string? Description { get; init; }
    public required Assembly Assembly { get; init; }
    public required IPlugin Instance { get; init; }
    public IPluginServiceRegistrar? ServiceRegistrar { get; init; }
    public IReadOnlyList<PluginComponentInfo> Components { get; init; } = [];
}

/// <summary>
/// Contains metadata about a plugin component.
/// </summary>
public sealed class PluginComponentInfo
{
    public required Type ComponentType { get; init; }
    public string? Route { get; init; }
    public string? DisplayName { get; init; }
    public string? Icon { get; init; }
    public int Order { get; init; }
    public bool ShowInNavigation { get; init; }
}
