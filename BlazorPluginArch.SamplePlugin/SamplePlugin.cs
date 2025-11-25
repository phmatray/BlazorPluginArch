using BlazorPluginArch.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorPluginArch.SamplePlugin;

/// <summary>
/// A sample plugin demonstrating the plugin architecture.
/// </summary>
public class SamplePlugin : IPlugin, IPluginServiceRegistrar
{
    public string Id => "sample-plugin";
    public string Name => "Sample Plugin";
    public string Version => "1.0.0";
    public string Description => "A sample plugin demonstrating the plugin architecture.";

    public void Initialize()
    {
        // Plugin initialization logic goes here
        Console.WriteLine($"[{Name}] Plugin initialized!");
    }

    public void RegisterServices(IServiceCollection services)
    {
        // Register plugin-specific services
        services.AddScoped<ISampleService, SampleService>();
    }
}

/// <summary>
/// Sample service interface provided by this plugin.
/// </summary>
public interface ISampleService
{
    string GetGreeting(string name);
    int GetCounter();
    void IncrementCounter();
}

/// <summary>
/// Sample service implementation.
/// </summary>
internal class SampleService : ISampleService
{
    private int _counter;

    public string GetGreeting(string name)
        => $"Hello, {name}! This greeting comes from the Sample Plugin.";

    public int GetCounter() => _counter;

    public void IncrementCounter() => _counter++;
}
