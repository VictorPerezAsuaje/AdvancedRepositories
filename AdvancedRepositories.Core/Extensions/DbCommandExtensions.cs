using System.Data;

namespace AdvancedRepositories.Core.Extensions;

internal static class DbCommandExtensions
{
    internal static void AddWithValue(this IDbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
