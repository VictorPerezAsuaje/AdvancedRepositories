using AdvancedRepositories.Core.Configuration;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Repositories;

public abstract class BaseRepository : IDisposable
{
    protected SqlConnection _con;
    protected SqlTransaction _transaction;
    protected BaseDatabaseConfiguration _dbConfig;

    public BaseRepository(BaseDatabaseConfiguration dbConfig)
    {
        _dbConfig = dbConfig;
        _con = new SqlConnection(_dbConfig.ConnectionString());
        _con.Open();
        _transaction = _con.BeginTransaction();
    }

    protected SqlCommand CreateCommand(string query = "")
    {
        SqlCommand cmd = new SqlCommand(query, _con, _transaction);
        return cmd;
    }

    public void Dispose()
    {
        if (_transaction != null) _transaction.Dispose();

        if (_con == null) return;

        _con.Close();
        _con.Dispose();
    }

    protected void SaveChanges()
    {
        _transaction.Commit();
    }
}
