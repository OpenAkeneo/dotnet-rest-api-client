// DocGen — generates the LLM-facing API documentation (llms.txt + docs/api/*.md) from the
// COMPILED library surface (reflection + the compiler-produced XML doc file), so the docs
// cannot drift from what actually ships. Hand-written context lives in docs/api/_fragments/
// and is merged in. Run via: dotnet run --project tools/DocGen -- <repo-root>
// Regenerating and committing the output is a release-gate step (guidelines/TESTING.md §5).

using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenAkeneo.RestApiClient;

var repoRoot = args.Length > 0 ? Path.GetFullPath(args[0]) : FindRepoRoot();
var libDir = Path.Combine(repoRoot, "OpenAkeneo.RestApiClient");
var fragmentsDir = Path.Combine(repoRoot, "docs", "api", "_fragments");
var outDocsDir = Path.Combine(repoRoot, "docs", "api");
var outLlms = Path.Combine(repoRoot, "llms.txt");

const string RawBaseUrl = "https://raw.githubusercontent.com/OpenAkeneo/dotnet-rest-api-client/main";

var assembly = typeof(AkeneoContext).Assembly;
var xmlDocPath = Path.ChangeExtension(assembly.Location, ".xml");
var xmlDocs = XmlDocs.Load(xmlDocPath);
var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
              ?? assembly.GetName().Version?.ToString(3) ?? "0.0.0";

// ---------------------------------------------------------------------------
// 1. Domain map: which AkeneoContext method belongs to which partial file.
//    (Reflection cannot recover the source file; a light source scan can.)
// ---------------------------------------------------------------------------
var methodRegex = new Regex(@"^\s*public\s+(?:async\s+)?[^(=\n]*?\s(\w+)\s*\(", RegexOptions.Multiline);
var domainOf = new Dictionary<string, (string Domain, int Order)>();
foreach (var file in Directory.GetFiles(libDir, "AkeneoContext*.cs"))
{
    var name = Path.GetFileNameWithoutExtension(file);
    var domain = name == "AkeneoContext" ? "client" : name["AkeneoContext.".Length..];
    var source = File.ReadAllText(file);
    foreach (Match m in methodRegex.Matches(source))
        domainOf.TryAdd(m.Groups[1].Value, (domain, m.Index));
}

// ---------------------------------------------------------------------------
// 2. Collect the public surface.
// ---------------------------------------------------------------------------
var contextMethods = typeof(AkeneoContext)
    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
    .Where(m => !m.IsSpecialName && m.Name != "Dispose")
    .ToList();

var serviceMethods = typeof(IAkeneoRestApiService)
    .GetMethods(BindingFlags.Public | BindingFlags.Instance)
    .Where(m => !m.IsSpecialName)
    .ToList();

var byDomain = contextMethods
    .GroupBy(m => domainOf.TryGetValue(m.Name, out var d) ? d.Domain : "client")
    .OrderBy(g => g.Key, StringComparer.Ordinal)
    .ToList();

// ---------------------------------------------------------------------------
// 3. Emit per-domain pages.
// ---------------------------------------------------------------------------
Directory.CreateDirectory(outDocsDir);
var domainFiles = new List<(string Domain, string FileName, List<string> MethodNames)>();

foreach (var group in byDomain)
{
    var domain = group.Key;
    var fileName = ToKebab(domain) + ".md";
    var ordered = group
        .OrderBy(m => domainOf.TryGetValue(m.Name, out var d) ? d.Order : int.MaxValue)
        .ThenBy(m => m.GetParameters().Length)
        .ToList();

    var sb = new StringBuilder();
    sb.AppendLine($"# {Title(domain)} — OpenAkeneo.RestApiClient");
    sb.AppendLine();
    sb.AppendLine($"Methods on `AkeneoContext` for the {Title(domain)} domain. All methods are async and");
    sb.AppendLine("accept an optional trailing `CancellationToken ct`. All throw `AkeneoApiException` on");
    sb.AppendLine("non-success responses. Generated from the compiled v" + version + " surface — do not edit by hand.");
    AppendFragment(sb, fragmentsDir, ToKebab(domain));
    sb.AppendLine();

    foreach (var method in ordered)
    {
        RenderMethod(sb, method, xmlDocs);
    }

    WriteNormalized(Path.Combine(outDocsDir, fileName), sb.ToString());
    domainFiles.Add((domain, fileName, ordered.Select(m => m.Name).Distinct().ToList()));
}

// Transport page (IAkeneoRestApiService).
{
    var sb = new StringBuilder();
    sb.AppendLine("# Transport (IAkeneoRestApiService) — OpenAkeneo.RestApiClient");
    sb.AppendLine();
    sb.AppendLine("The low-level HTTP service underneath `AkeneoContext` (reachable via `context.Service`).");
    sb.AppendLine("Use it as an escape hatch for raw calls; token management, retries and 401 refresh apply");
    sb.AppendLine("automatically. Generated from the compiled v" + version + " surface — do not edit by hand.");
    AppendFragment(sb, fragmentsDir, "transport");
    sb.AppendLine();
    foreach (var method in serviceMethods.OrderBy(m => m.Name, StringComparer.Ordinal).ThenBy(m => m.GetParameters().Length))
        RenderMethod(sb, method, xmlDocs);
    WriteNormalized(Path.Combine(outDocsDir, "transport.md"), sb.ToString());
    domainFiles.Add(("Transport", "transport.md", serviceMethods.Select(m => m.Name).Distinct().ToList()));
}

// ---------------------------------------------------------------------------
// 4. Emit llms.txt: hand-written header + generated index.
// ---------------------------------------------------------------------------
{
    var sb = new StringBuilder();
    var header = Path.Combine(fragmentsDir, "llms-header.md");
    sb.AppendLine(File.ReadAllText(header).Replace("{VERSION}", version).TrimEnd());
    sb.AppendLine();
    sb.AppendLine("## API reference by domain");
    sb.AppendLine();
    sb.AppendLine("Each page lists every method with its exact signature, purpose, parameters and gotchas.");
    sb.AppendLine("Grep the method name inside the page. Raw URLs resolve without authentication.");
    sb.AppendLine();
    foreach (var (domain, fileName, methods) in domainFiles.OrderBy(d => d.FileName, StringComparer.Ordinal))
    {
        sb.AppendLine($"### [{Title(domain)}]({RawBaseUrl}/docs/api/{fileName})");
        sb.AppendLine();
        sb.AppendLine(string.Join(", ", methods.Select(m => $"`{m}`")));
        sb.AppendLine();
    }
    sb.AppendLine("---");
    sb.AppendLine($"Generated by tools/DocGen from the compiled v{version} assembly. Regenerate with:");
    sb.AppendLine("`dotnet run --project tools/DocGen` (release-gate step, see guidelines/TESTING.md §5).");
    WriteNormalized(outLlms, sb.ToString());
}

Console.WriteLine($"DocGen v{version}: {domainFiles.Count} domain pages, {contextMethods.Count} context methods, {serviceMethods.Count} service methods.");
Console.WriteLine($"→ {outLlms}");
Console.WriteLine($"→ {outDocsDir}");
return;

// ---------------------------------------------------------------------------
// Rendering
// ---------------------------------------------------------------------------

/// <summary>LF line endings regardless of OS, so output is byte-identical on every CI runner.</summary>
static void WriteNormalized(string path, string content)
    => File.WriteAllText(path, content.Replace("\r\n", "\n"));
static void RenderMethod(StringBuilder sb, MethodInfo method, XmlDocs xmlDocs)
{
    var doc = xmlDocs.For(method);

    sb.AppendLine($"## `{method.Name}`");
    sb.AppendLine();
    sb.AppendLine("```csharp");
    sb.AppendLine(Signature(method));
    sb.AppendLine("```");
    sb.AppendLine();
    if (!string.IsNullOrWhiteSpace(doc.Summary))
        sb.AppendLine(doc.Summary);

    var parameters = method.GetParameters()
        .Where(p => p.Name != "ct")
        .Select(p => (p.Name, Text: doc.Param(p.Name!)))
        .Where(p => !string.IsNullOrWhiteSpace(p.Text))
        .ToList();
    if (parameters.Count > 0)
    {
        sb.AppendLine();
        foreach (var (name, text) in parameters)
            sb.AppendLine($"- `{name}` — {text}");
    }
    if (!string.IsNullOrWhiteSpace(doc.Returns))
    {
        sb.AppendLine();
        sb.AppendLine($"Returns: {doc.Returns}");
    }
    sb.AppendLine();
}

static void AppendFragment(StringBuilder sb, string fragmentsDir, string key)
{
    var path = Path.Combine(fragmentsDir, key + ".md");
    if (!File.Exists(path)) return;
    sb.AppendLine();
    sb.AppendLine(File.ReadAllText(path).TrimEnd());
}

static string Signature(MethodInfo method)
{
    var nullability = new NullabilityInfoContext();
    var parameters = method.GetParameters().Select(p =>
    {
        var type = TypeName(p.ParameterType, nullability.Create(p));
        var text = $"{type} {p.Name}";
        if (p.HasDefaultValue)
            text += " = " + DefaultValue(p);
        return text;
    });
    return $"{TypeName(method.ReturnType, null)} {method.Name}({string.Join(", ", parameters)})";
}

static string DefaultValue(ParameterInfo p) => p.DefaultValue switch
{
    null when p.ParameterType == typeof(CancellationToken) => "default",
    null => "null",
    bool b => b ? "true" : "false",
    string s => $"\"{s}\"",
    var v when v.GetType() == typeof(CancellationToken) => "default",
    var v => v.ToString() ?? "default"
};

static string TypeName(Type type, NullabilityInfo? nullability)
{
    var suffix = nullability?.ReadState == NullabilityState.Nullable && Nullable.GetUnderlyingType(type) == null ? "?" : "";
    if (type.IsArray)
        return TypeName(type.GetElementType()!, null) + "[]" + suffix;
    var underlying = Nullable.GetUnderlyingType(type);
    if (underlying != null)
        return TypeName(underlying, null) + "?";
    if (type.IsGenericType)
    {
        var name = type.Name[..type.Name.IndexOf('`')];
        var argInfos = nullability?.GenericTypeArguments;
        var argNames = type.GetGenericArguments()
            .Select((t, i) => TypeName(t, argInfos != null && i < argInfos.Length ? argInfos[i] : null));
        return $"{name}<{string.Join(", ", argNames)}>" + suffix;
    }
    return Alias(type) + suffix;
}

static string Alias(Type type) => type == typeof(string) ? "string"
    : type == typeof(int) ? "int"
    : type == typeof(bool) ? "bool"
    : type == typeof(long) ? "long"
    : type == typeof(byte) ? "byte"
    : type == typeof(object) ? "object"
    : type == typeof(void) ? "void"
    : type == typeof(decimal) ? "decimal"
    : type == typeof(double) ? "double"
    : type.Name;

static string ToKebab(string value) =>
    Regex.Replace(value, "(?<=[a-z0-9])(?=[A-Z])", "-").ToLowerInvariant();

static string Title(string domain) =>
    Regex.Replace(domain, "(?<=[a-z0-9])(?=[A-Z])", " ");

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null && !File.Exists(Path.Combine(dir.FullName, "OpenAkeneo.RestApiClient.sln")))
        dir = dir.Parent;
    return dir?.FullName ?? throw new InvalidOperationException("Repo root not found; pass it as the first argument.");
}

// ---------------------------------------------------------------------------
// XML documentation file access
// ---------------------------------------------------------------------------
sealed class XmlDocs
{
    private readonly Dictionary<string, XElement> _members;

    private XmlDocs(Dictionary<string, XElement> members) => _members = members;

    public static XmlDocs Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"XML doc file not found: {path}. Build the library with GenerateDocumentationFile first.");
        var members = XDocument.Load(path).Descendants("member")
            .Where(m => m.Attribute("name") != null)
            .GroupBy(m => m.Attribute("name")!.Value)
            .ToDictionary(g => g.Key, g => g.First());
        return new XmlDocs(members);
    }

    public (string Summary, string Returns, Func<string, string> Param) For(MethodInfo method)
    {
        _members.TryGetValue(DocId(method), out var element);
        var summary = Clean(element?.Element("summary"));
        var returns = Clean(element?.Element("returns"));
        return (summary, returns, name => Clean(
            element?.Elements("param").FirstOrDefault(p => p.Attribute("name")?.Value == name)));
    }

    /// <summary>Builds the compiler's documentation ID (M:Namespace.Type.Method(Params)).</summary>
    private static string DocId(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var paramList = parameters.Length == 0
            ? ""
            : "(" + string.Join(",", parameters.Select(p => DocTypeName(p.ParameterType))) + ")";
        return $"M:{method.DeclaringType!.FullName}.{method.Name}{paramList}";
    }

    private static string DocTypeName(Type type)
    {
        if (type.IsArray)
            return DocTypeName(type.GetElementType()!) + "[]";
        if (type.IsGenericType)
        {
            var name = type.GetGenericTypeDefinition().FullName!;
            name = name[..name.IndexOf('`')];
            var args = string.Join(",", type.GetGenericArguments().Select(DocTypeName));
            return $"{name}{{{args}}}";
        }
        return type.FullName!;
    }

    /// <summary>Flattens XML doc content to plain markdown-ish text.</summary>
    private static string Clean(XElement? element)
    {
        if (element == null) return "";
        var sb = new StringBuilder();
        Flatten(element, sb);
        return Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
    }

    private static void Flatten(XElement element, StringBuilder sb)
    {
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText text:
                    sb.Append(text.Value);
                    break;
                case XElement el when el.Name == "see" || el.Name == "seealso":
                    var cref = el.Attribute("cref")?.Value ?? el.Attribute("href")?.Value ?? "";
                    sb.Append('`').Append(cref.Split('.', '(').LastOrDefault(s => s.Length > 0) ?? cref).Append('`');
                    break;
                case XElement el when el.Name == "paramref" || el.Name == "typeparamref":
                    sb.Append('`').Append(el.Attribute("name")?.Value).Append('`');
                    break;
                case XElement el when el.Name == "c" || el.Name == "code":
                    sb.Append('`').Append(el.Value).Append('`');
                    break;
                case XElement el:
                    Flatten(el, sb);
                    break;
            }
        }
    }
}
