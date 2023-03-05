using System.Reflection;

namespace AdvancedRepositories.Core.Extensions;

internal static class TypeExtensions
{
    internal static IEnumerable<PropertyInfo> GetPropsWithCustomType<T>(this Type type)
        => type.GetProperties().Where(x => x.GetCustomAttribute(typeof(T)) is T);
}