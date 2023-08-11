using System.Text.RegularExpressions;

namespace Moq.To.NSubstitute;

public static class FindAndReplace
{
    public static string RegexReplace(this string content, string pattern, string replacement)
    {
        var replacedContent = Regex.Replace(content, pattern, replacement, RegexOptions.Singleline);

        return replacedContent;
    }
}