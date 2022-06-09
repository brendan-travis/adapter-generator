// See https://aka.ms/new-console-template for more information

using System.Text;

var methods = typeof(Console).GetMethods().Where(m => m.IsPublic);

var interfaceStringBuilder = new StringBuilder();
var adapterStringBuilder = new StringBuilder();

interfaceStringBuilder.Append($"/// <inheritdoc cref=\"{nameof(Console)}\"/>\n");
interfaceStringBuilder.Append($"public interface I{nameof(Console)}Adapter\n");
interfaceStringBuilder.Append('{');

adapterStringBuilder.Append($"public class {nameof(Console)}Adapter : I{nameof(Console)}Adapter\n");
adapterStringBuilder.Append('{');

foreach (var method in methods)
{
    if (method.Name.StartsWith("get_") ||
        method.Name.StartsWith("set_") ||
        method.Name.StartsWith("add_") ||
        method.Name.StartsWith("remove_"))
    {
        continue;
    }

    var i = method.ToString()!.IndexOf(" ", StringComparison.Ordinal) + 1;
    var inheritDoc = method.ToString()![i..];

    var returnType = method.ReturnType.ToString();
    if (returnType.Contains("`2["))
    {
        returnType = returnType.Replace("`2[", "<");
        returnType = returnType.Replace("]", ">");
    }

    var parameterStringBuilder = new StringBuilder();
    var callingStringBuilder = new StringBuilder();

    for (var index = 0; index < method.GetParameters().Length; index++)
    {
        var parameter = method.GetParameters()[index];

        parameterStringBuilder.Append($"{parameter.ParameterType} {parameter.Name}");
        callingStringBuilder.Append($"{parameter.Name}");

        if (index != method.GetParameters().Length - 1)
        {
            parameterStringBuilder.Append(", ");
            callingStringBuilder.Append(", ");
        }
    }

    interfaceStringBuilder.Append(
        $"\n    /// <inheritdoc cref=\"{method.DeclaringType!.ToString()}.{inheritDoc}\"/>\n");
    interfaceStringBuilder.Append($"    public {returnType} {method.Name}({parameterStringBuilder.ToString()});\n");

    adapterStringBuilder.Append($"\n    public {returnType} {method.Name}({parameterStringBuilder.ToString()})");
    adapterStringBuilder.Append($" => {method.DeclaringType.ToString()}.{method.Name}({callingStringBuilder.ToString()});\n");
}

interfaceStringBuilder.Append("}\n");
adapterStringBuilder.Append("}\n");

var directory = Directory.CreateDirectory("output");
var interfaceOutput = Path.Combine(directory.FullName, $"I{nameof(Console)}Adapter.cs");
var adapterOutput = Path.Combine(directory.FullName, $"{nameof(Console)}Adapter.cs");

await File.WriteAllTextAsync(interfaceOutput, interfaceStringBuilder.ToString());
await File.WriteAllTextAsync(adapterOutput, adapterStringBuilder.ToString());

Console.WriteLine($"File written to: {interfaceOutput}");
Console.WriteLine($"File written to: {adapterOutput}");