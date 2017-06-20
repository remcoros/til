using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class ReadmeGenerator
{
    private const string StartToc = "<!-- TOC -->";
    private const string EndToc = "<!-- /TOC -->";
    private const string StartCatToc = "<!-- CATEGORIES -->";
    private const string EndCatToc = "<!-- /CATEGORIES -->";

    public static void Generate(string directory)
    {
        var readmeFile = Path.Combine(directory, "README.md");
        if (!File.Exists(readmeFile))
        {
            throw new FileNotFoundException(readmeFile);
        }

        var content = File.ReadAllText(readmeFile);

        // could be more efficient, but who cares here..
        content = ReplaceToken(content, TocGenerator.GenerateToc(directory), StartToc, EndToc);
        content = ReplaceToken(content, TocGenerator.GenerateCategoryList(directory), StartCatToc, EndCatToc);

        File.WriteAllText(readmeFile, content);
    }

    private static string ReplaceToken(string content, string replacementContent, string startToken, string endToken)
    {
        // TOC
        var startOffset = content.IndexOf(startToken, StringComparison.OrdinalIgnoreCase);
        var endOffset = content.IndexOf(endToken, StringComparison.OrdinalIgnoreCase);
        if (startOffset < 0
            || endOffset < 0
            || endOffset < startOffset)
        {
            throw new Exception(string.Format("Error while parsing {0} ... {1} tokens.", startToken, endToken));
        }

        var sb = new StringBuilder();
        sb.Append(content.Substring(0, startOffset));
        sb.AppendLine(startToken);
        sb.AppendLine(replacementContent);
        sb.Append(endToken);
        sb.Append(content.Substring(endOffset + endToken.Length));
        return sb.ToString();
    }
}

public static class TocGenerator
{
    private static readonly Regex FileDateRegex = new Regex(@"^(\d{4})\-?(\d\d)\-?(\d\d).*");

    public static string GenerateToc(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(directory);
        }

        // only root folders for now, a tree would be nice for subcategories.
        var dirInfo = new DirectoryInfo(directory);
        var categories = from d in dirInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                         where !d.Name.StartsWith(".")
                         orderby d.Name.ToLowerInvariant()
                         select new
                         {
                             Name = Capitalize(d.Name),
                             Posts = from post in
                                     from f in d.EnumerateFiles("*.md", SearchOption.TopDirectoryOnly)
                                     select new
                                     {
                                         File = f,
                                         RelativeLink = string.Format("{0}/{1}", d.Name, Uri.EscapeDataString(f.Name)),
                                         Title = GetTitle(f),
                                         Date = GetPostDate(f)
                                     }
                                     orderby post.Date.HasValue descending, post.Date descending, post.Title ascending
                                     select post
                         };

        var sb = new StringBuilder();
        foreach (var category in categories)
        {
            sb.AppendLine(string.Format("### <a name=\"{0}\"></a>{0}", category.Name));
            sb.AppendLine();

            foreach (var post in category.Posts)
            {
                string postDate = null;
                if (post.Date != null)
                {
                    postDate = string.Format(" *{0:yyyy-MM-dd}*", post.Date);
                }

                sb.AppendLine(string.Format("- [{0}]({1}){2}", post.Title, post.RelativeLink, postDate));
            }

            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string GenerateCategoryList(string directory)
    {
        // note: duplicate logic above
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException(directory);
        }

        // only root folders for now, a tree would be nice for subcategories.
        var dirInfo = new DirectoryInfo(directory);
        var categories = from d in dirInfo.EnumerateDirectories("*", SearchOption.TopDirectoryOnly)
                         where !d.Name.StartsWith(".")
                         orderby d.Name.ToLowerInvariant()
                         select new
                         {
                             Name = Capitalize(d.Name),
                         };

        var sb = new StringBuilder();
        foreach (var category in categories)
        {
            sb.AppendLine(string.Format("- [{0}](#{0})", category.Name));
        }

        return sb.ToString();
    }

    private static DateTime? GetPostDate(FileInfo file)
    {
        var match = FileDateRegex.Match(file.Name);
        if (match.Success)
        {
            return new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value));
        }

        return null;
    }

    private static string Capitalize(string str)
    {
        if (str == null)
            return null;

        if (str.Length > 1)
            return char.ToUpper(str[0]) + str.Substring(1);

        return str.ToUpper();
    }

    private static string GetTitle(FileInfo fileInfo)
    {
        using (var rdr = fileInfo.OpenText())
        {
            string line;
            while ((line = rdr.ReadLine()) != null)
            {
                line = line.TrimStart();
                if (line.StartsWith("#") && !line.StartsWith("##"))
                {
                    return line.Substring(1).TrimStart();
                }
            }
        }

        return "No title";
    }
}