using System.Text.RegularExpressions;

namespace AdvancedRepositories.Core.Extensions;

internal static class StringExtensions
{
    internal static string ClearMultipleSpaces(this string stringToFormat)
        => Regex.Replace(stringToFormat, @"\s+", " ");
}