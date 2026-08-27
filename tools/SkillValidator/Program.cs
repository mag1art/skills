using System.Text.RegularExpressions;

var root = Directory.GetCurrentDirectory();
var files = Directory
    .EnumerateFiles(root, "SKILL.md", SearchOption.AllDirectories)
    .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}.git{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .ToArray();

var errors = new List<string>();
var names = new HashSet<string>(StringComparer.Ordinal);

if (files.Length == 0)
{
    errors.Add("No SKILL.md files found.");
}

foreach (var path in files)
{
    var relative = Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
    var content = File.ReadAllText(path).Replace("\r\n", "\n");
    if (!content.StartsWith("---\n", StringComparison.Ordinal))
    {
        errors.Add($"{relative}: YAML frontmatter must start at line 1.");
        continue;
    }

    var closing = content.IndexOf("\n---\n", 4, StringComparison.Ordinal);
    if (closing < 0)
    {
        errors.Add($"{relative}: YAML frontmatter is not closed.");
        continue;
    }

    var frontmatter = content[4..closing];

    foreach (var key in new[] { "name", "version", "description", "author", "license" })
    {
        if (!Regex.IsMatch(frontmatter, $@"(?m)^{Regex.Escape(key)}\s*:\s*\S"))
        {
            errors.Add($"{relative}: missing {key}.");
        }
    }

    var nameMatch = Regex.Match(frontmatter, @"(?m)^name\s*:\s*(\S.*)$");
    if (nameMatch.Success)
    {
        var name = nameMatch.Groups[1].Value.Trim().Trim('"');
        if (!names.Add(name))
        {
            errors.Add($"duplicate skill name: {name}");
        }
    }

    ValidateList(frontmatter, "tags", relative, errors);
    ValidateList(frontmatter, "triggers", relative, errors);

    var relatedMatch = Regex.Match(frontmatter, @"(?m)^related_skills\s*:\s*\[([^\]]*)\]");
    if (relatedMatch.Success)
    {
        foreach (var related in relatedMatch.Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var relatedName = related.Trim('"', '\'');
            var relatedPath = Path.Combine(root, relatedName, "SKILL.md");
            if (!File.Exists(relatedPath))
            {
                errors.Add($"{relative}: related skill not found: {relatedName}.");
            }
        }
    }
}

static void ValidateList(string frontmatter, string key, string relative, List<string> errors)
{
    var inline = Regex.Match(frontmatter, $@"(?m)^{Regex.Escape(key)}\s*:\s*(.*)$");
    var isInlineList = inline.Success &&
                       inline.Groups[1].Value.Trim() is var value &&
                       value.Length > 2 &&
                       value.StartsWith("[", StringComparison.Ordinal) &&
                       value.EndsWith("]", StringComparison.Ordinal) &&
                       value[1..^1].Trim().Length > 0;

    var hasBlockItems = Regex.IsMatch(
        frontmatter,
        $@"(?ms)^{Regex.Escape(key)}\s*:\s*\n(?:\s*-\s+\S.*(?:\n|$))+");

    if (!isInlineList && !hasBlockItems)
    {
        errors.Add($"{relative}: {key} must be a non-empty YAML list.");
    }
}

if (errors.Count == 0)
{
    Console.WriteLine($"Validated {files.Length} skills successfully.");
    return;
}

foreach (var error in errors)
{
    Console.Error.WriteLine($"ERROR: {error}");
}

Environment.ExitCode = 1;
