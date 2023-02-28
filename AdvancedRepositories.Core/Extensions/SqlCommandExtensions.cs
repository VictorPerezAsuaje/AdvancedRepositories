using AdvancedRepositories.Core.Repositories.Fluent;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Extensions;

public static class SqlCommandExtensions
{
    public static SqlCommand Filter(this SqlCommand cmd, Action<QueryFilterBuilder> filterConfig)
    {
        QueryFilterBuilder filterBuilder = new QueryFilterBuilder(cmd, filterConfig);
        return filterBuilder.GetCommandWithFilter();
    }
}