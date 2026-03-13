using Microsoft.JSInterop;

namespace BlazorPluginArch.Abstractions;

/// <summary>
/// Helper for loading JavaScript modules from plugin assemblies.
/// Automatically resolves the correct <c>_content/{AssemblyName}/{path}</c> URL
/// for static assets served from Razor Class Libraries.
/// </summary>
/// <example>
/// <code>
/// @implements IAsyncDisposable
/// @inject IJSRuntime JS
///
/// @code {
///     private PluginJsModule? _module;
///
///     protected override async Task OnAfterRenderAsync(bool firstRender)
///     {
///         if (firstRender)
///         {
///             _module = await PluginJsModule.ImportAsync(JS, typeof(MyComponent), "js/my-script.js");
///         }
///     }
///
///     public async ValueTask DisposeAsync() => await (_module?.DisposeAsync() ?? ValueTask.CompletedTask);
/// }
/// </code>
/// </example>
public sealed class PluginJsModule : IAsyncDisposable
{
    private readonly IJSObjectReference _module;
    private bool _disposed;

    private PluginJsModule(IJSObjectReference module)
    {
        _module = module;
    }

    /// <summary>
    /// Imports a JavaScript module from a plugin's static assets.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime instance.</param>
    /// <param name="pluginType">Any type from the plugin assembly (used to resolve the assembly name).</param>
    /// <param name="modulePath">
    /// Path to the JS module relative to the plugin's <c>wwwroot/</c> folder.
    /// Example: <c>"js/my-script.js"</c>
    /// </param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="PluginJsModule"/> wrapping the imported JS module.</returns>
    public static async Task<PluginJsModule> ImportAsync(
        IJSRuntime jsRuntime,
        Type pluginType,
        string modulePath,
        CancellationToken cancellationToken = default)
    {
        var assemblyName = pluginType.Assembly.GetName().Name
            ?? throw new InvalidOperationException(
                $"Cannot determine assembly name for type '{pluginType.FullName}'.");

        var contentPath = $"./_content/{assemblyName}/{modulePath}";
        var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, [contentPath]);
        return new PluginJsModule(module);
    }

    /// <summary>
    /// Invokes a JavaScript function from the imported module.
    /// </summary>
    public async ValueTask<TValue> InvokeAsync<TValue>(
        string identifier,
        CancellationToken cancellationToken,
        params object?[] args)
        => await _module.InvokeAsync<TValue>(identifier, cancellationToken, args);

    /// <summary>
    /// Invokes a JavaScript function from the imported module.
    /// </summary>
    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, params object?[] args)
        => await _module.InvokeAsync<TValue>(identifier, args);

    /// <summary>
    /// Invokes a void JavaScript function from the imported module.
    /// </summary>
    public async ValueTask InvokeVoidAsync(
        string identifier,
        CancellationToken cancellationToken,
        params object?[] args)
        => await _module.InvokeVoidAsync(identifier, cancellationToken, args);

    /// <summary>
    /// Invokes a void JavaScript function from the imported module.
    /// </summary>
    public async ValueTask InvokeVoidAsync(string identifier, params object?[] args)
        => await _module.InvokeVoidAsync(identifier, args);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected during disposal — safe to ignore.
        }
    }
}
