using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Repositories.Advanced;

public abstract class AdvancedCommand
{
    protected SqlCommand _cmd;

    public AdvancedCommand(SqlCommand cmd)
    {
        _cmd = cmd;
    }
}
