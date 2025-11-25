namespace BlazorPluginArch.Abstractions;

/// <summary>
/// Optional attribute to customize how a Razor component is registered as a plugin component.
/// The source generator reads this from .razor files to set DisplayName, Order, etc.
/// If not specified, defaults are derived from the file name and @page directive.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PluginComponentAttribute : Attribute
{
    /// <summary>
    /// Display name for navigation menus. Defaults to the component file name split by PascalCase.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Order in navigation menu. Lower values appear first. Default is 100.
    /// </summary>
    public int Order { get; set; } = 100;

    /// <summary>
    /// Whether this component should appear in navigation menus. Default is true.
    /// </summary>
    public bool ShowInNavigation { get; set; } = true;
}
