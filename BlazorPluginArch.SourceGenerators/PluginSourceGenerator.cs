using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BlazorPluginArch.SourceGenerators;

[Generator]
public class PluginSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes that implement IPlugin
        var pluginProvider = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsClassDeclaration(node),
                transform: static (ctx, _) => GetPluginInfo(ctx))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Find all .razor files and extract component info
        var razorFilesProvider = context.AdditionalTextsProvider
            .Where(static file => file.Path.EndsWith(".razor"))
            .Select(static (file, ct) => ParseRazorFile(file, ct))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!);

        // Combine plugins with razor components
        var combined = pluginProvider.Collect()
            .Combine(razorFilesProvider.Collect());

        // Generate the registration code
        context.RegisterSourceOutput(combined, static (spc, source) =>
        {
            var (plugins, components) = source;
            if (plugins.Length > 0)
            {
                GeneratePluginRegistration(spc, plugins, components);
            }
        });
    }

    private static bool IsClassDeclaration(SyntaxNode node)
        => node is ClassDeclarationSyntax;

    private static PluginData? GetPluginInfo(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;

        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol symbol)
            return null;

        // Check if the class implements IPlugin
        foreach (var iface in symbol.AllInterfaces)
        {
            if (iface.Name == "IPlugin" &&
                iface.ContainingNamespace?.ToDisplayString() == "BlazorPluginArch.Abstractions")
            {
                // Also check if it implements IPluginServiceRegistrar
                var hasServiceRegistrar = symbol.AllInterfaces.Any(i =>
                    i.Name == "IPluginServiceRegistrar" &&
                    i.ContainingNamespace?.ToDisplayString() == "BlazorPluginArch.Abstractions");

                return new PluginData(
                    symbol.ToDisplayString(),
                    symbol.ContainingAssembly.Name,
                    hasServiceRegistrar);
            }
        }

        return null;
    }

    private static RazorComponentData? ParseRazorFile(AdditionalText file, System.Threading.CancellationToken ct)
    {
        var text = file.GetText(ct)?.ToString();
        if (string.IsNullOrEmpty(text))
            return null;

        // Extract @page directive
        var pageMatch = Regex.Match(text, @"@page\s+""([^""]+)""");
        if (!pageMatch.Success)
            return null; // Not a routable component

        var route = pageMatch.Groups[1].Value;

        // Extract component name from file path
        // e.g., "Components/Pages/SamplePage.razor" -> "SamplePage"
        var fileName = System.IO.Path.GetFileNameWithoutExtension(file.Path);

        // Try to determine the namespace from the file path
        // This is a heuristic - we look for common patterns
        var fullPath = file.Path.Replace("\\", "/");
        var componentTypeName = GetComponentTypeName(fullPath, fileName);

        // Check for PluginComponent attribute
        var attrMatch = Regex.Match(text, @"@attribute\s+\[PluginComponent\s*\(([^\]]*)\)\]");

        string? displayName = null;
        int order = 100;
        bool showInNavigation = true;

        if (attrMatch.Success)
        {
            var attrContent = attrMatch.Groups[1].Value;

            // Parse DisplayName
            var displayNameMatch = Regex.Match(attrContent, @"DisplayName\s*=\s*""([^""]+)""");
            if (displayNameMatch.Success)
                displayName = displayNameMatch.Groups[1].Value;

            // Parse Order
            var orderMatch = Regex.Match(attrContent, @"Order\s*=\s*(\d+)");
            if (orderMatch.Success)
                order = int.Parse(orderMatch.Groups[1].Value);

            // Parse ShowInNavigation
            var showMatch = Regex.Match(attrContent, @"ShowInNavigation\s*=\s*(true|false)", RegexOptions.IgnoreCase);
            if (showMatch.Success)
                showInNavigation = showMatch.Groups[1].Value.ToLower() == "true";
        }

        // Default display name from file name if not specified
        if (string.IsNullOrEmpty(displayName))
            displayName = SplitPascalCase(fileName);

        return new RazorComponentData(
            componentTypeName,
            route,
            displayName ?? fileName,
            order,
            showInNavigation);
    }

    private static string GetComponentTypeName(string fullPath, string fileName)
    {
        // Try to extract namespace from path
        // Common patterns: "ProjectName/Components/Pages/MyPage.razor"
        // We need to figure out the assembly name and namespace

        var parts = fullPath.Split('/');
        var componentParts = new List<string>();

        bool foundComponents = false;
        string? assemblyName = null;

        for (int i = 0; i < parts.Length - 1; i++)
        {
            var part = parts[i];

            // Look for the project folder (contains .csproj typically)
            if (part.EndsWith(".SamplePlugin") || part.Contains("Plugin"))
            {
                assemblyName = part;
                foundComponents = false;
                componentParts.Clear();
            }

            if (foundComponents || part == "Components" || part == "Pages" || part == "Shared")
            {
                foundComponents = true;
                componentParts.Add(part);
            }
        }

        // If we found an assembly name, use it
        if (assemblyName != null && componentParts.Count > 0)
        {
            return $"{assemblyName}.{string.Join(".", componentParts)}.{fileName}";
        }

        // Fallback: just use the parts we found
        if (componentParts.Count > 0)
        {
            return $"{string.Join(".", componentParts)}.{fileName}";
        }

        return fileName;
    }

    private static string SplitPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var result = new StringBuilder();
        result.Append(input[0]);

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]))
                result.Append(' ');
            result.Append(input[i]);
        }

        return result.ToString();
    }

    private static void GeneratePluginRegistration(
        SourceProductionContext context,
        ImmutableArray<PluginData> plugins,
        ImmutableArray<RazorComponentData> components)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System.Reflection;");
        sb.AppendLine("using BlazorPluginArch.Abstractions;");
        sb.AppendLine();

        var assemblyName = plugins.First().AssemblyName;
        sb.AppendLine($"namespace {assemblyName}.Generated;");
        sb.AppendLine();

        // Generate a static registration class for this plugin assembly
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// Auto-generated plugin registration for this assembly.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static class PluginRegistration");
        sb.AppendLine("{");
        sb.AppendLine("    /// <summary>");
        sb.AppendLine("    /// Gets the plugin info for plugins in this assembly.");
        sb.AppendLine("    /// </summary>");
        sb.AppendLine("    public static IEnumerable<PluginInfo> GetPlugins()");
        sb.AppendLine("    {");

        foreach (var plugin in plugins)
        {
            var varName = plugin.FullTypeName.Replace(".", "_");
            sb.AppendLine($"        var plugin_{varName} = new {plugin.FullTypeName}();");
            sb.AppendLine($"        var assembly_{varName} = typeof({plugin.FullTypeName}).Assembly;");
            sb.AppendLine();

            // Generate component list
            sb.AppendLine($"        var components_{varName} = new List<PluginComponentInfo>");
            sb.AppendLine("        {");

            foreach (var comp in components)
            {
                sb.AppendLine("            new PluginComponentInfo");
                sb.AppendLine("            {");
                sb.AppendLine($"                ComponentType = typeof({comp.TypeName}),");
                sb.AppendLine($"                Route = \"{comp.Route}\",");
                sb.AppendLine($"                DisplayName = \"{comp.DisplayName}\",");
                sb.AppendLine($"                Order = {comp.Order},");
                sb.AppendLine($"                ShowInNavigation = {comp.ShowInNavigation.ToString().ToLower()}");
                sb.AppendLine("            },");
            }

            sb.AppendLine("        };");
            sb.AppendLine();

            sb.AppendLine("        yield return new PluginInfo");
            sb.AppendLine("        {");
            sb.AppendLine($"            Id = plugin_{varName}.Id,");
            sb.AppendLine($"            Name = plugin_{varName}.Name,");
            sb.AppendLine($"            Version = plugin_{varName}.Version,");
            sb.AppendLine($"            Description = plugin_{varName}.Description,");
            sb.AppendLine($"            Assembly = assembly_{varName},");
            sb.AppendLine($"            Instance = plugin_{varName},");

            if (plugin.HasServiceRegistrar)
            {
                sb.AppendLine($"            ServiceRegistrar = plugin_{varName} as IPluginServiceRegistrar,");
            }
            else
            {
                sb.AppendLine("            ServiceRegistrar = null,");
            }

            sb.AppendLine($"            Components = components_{varName}");
            sb.AppendLine("        };");
            sb.AppendLine();
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("PluginRegistration.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }
}

internal sealed class PluginData
{
    public PluginData(string fullTypeName, string assemblyName, bool hasServiceRegistrar)
    {
        FullTypeName = fullTypeName;
        AssemblyName = assemblyName;
        HasServiceRegistrar = hasServiceRegistrar;
    }

    public string FullTypeName { get; }
    public string AssemblyName { get; }
    public bool HasServiceRegistrar { get; }
}

internal sealed class RazorComponentData
{
    public RazorComponentData(
        string typeName,
        string route,
        string displayName,
        int order,
        bool showInNavigation)
    {
        TypeName = typeName;
        Route = route;
        DisplayName = displayName;
        Order = order;
        ShowInNavigation = showInNavigation;
    }

    public string TypeName { get; }
    public string Route { get; }
    public string DisplayName { get; }
    public int Order { get; }
    public bool ShowInNavigation { get; }
}
