using System.Reflection;

namespace AdvancedRepositories.Core.Extensions;

internal static class PropertyInfoArrayExtensions
{
    internal static IEnumerable<PropertyInfo> OfCustomType<T>(this PropertyInfo[] props)
        => props.Where(x => x.GetCustomAttribute(typeof(T)) is T);
}
