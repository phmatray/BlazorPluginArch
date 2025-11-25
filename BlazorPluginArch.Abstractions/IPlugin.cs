namespace BlazorPluginArch.Abstractions;

/// <summary>
/// Base interface for all plugins. Implement this interface to create a plugin.
/// The source generator will discover implementations and generate registration code.
/// </summary>
public interface IPlugin
{
    /// <summary>
    /// Unique identifier for the plugin.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Display name of the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Version of the plugin.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Optional description of the plugin.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Called when the plugin is being initialized.
    /// Use this to perform any setup required by the plugin.
    /// </summary>
    void Initialize();
}
