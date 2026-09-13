#:property RestorePackagesWithLockFile=false

using System.Text.RegularExpressions;
using System.Xml.Linq;

var root = Directory.GetCurrentDirectory();
var propsPath = Path.Combine(root, "Directory.Packages.props");
var registerPath = Path.Combine(root, "docs", "dependencies.md");

var packages = XDocument.Load(propsPath)
    .Descendants("PackageVersion")
    .Select(element => (string?)element.Attribute("Include"))
    .Where(name => name is not null)
    .Select(name => name!)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var registered = File.ReadLines(registerPath)
    .Select(line => Regex.Match(line, @"^\|\s*`([^`]+)`\s*\|"))
    .Where(match => match.Success)
    .Select(match => match.Groups[1].Value)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

var missing = packages.Except(registered).Order().ToList();
var stale = registered.Except(packages).Order().ToList();

foreach (var name in missing)
{
    Console.Error.WriteLine($"No row in docs/dependencies.md for package {name}");
}

foreach (var name in stale)
{
    Console.Error.WriteLine($"docs/dependencies.md lists {name}, which is not in Directory.Packages.props");
}

return missing.Count + stale.Count == 0 ? 0 : 1;
