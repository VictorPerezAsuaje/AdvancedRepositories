using System.Data;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Repositories.Advanced;

public abstract class AdvancedCommand
{
    protected IDbCommand _cmd;

    public AdvancedCommand(IDbCommand cmd)
    {
        _cmd = cmd;
    }
}
