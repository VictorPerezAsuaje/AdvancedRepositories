using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Extensions;
using FluentRepositories.Attributes;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace AdvancedRepositories.Core.Repositories;

public abstract class BaseRepository : IDisposable
{
    protected IDbConnection _con;
    protected IDbTransaction _transaction;
    protected BaseDatabaseConfiguration _dbConfig;

    public BaseRepository(BaseDatabaseConfiguration dbConfig)
    {
        _dbConfig = dbConfig;

        _con = _dbConfig.DatabaseType switch
        {
            DatabaseType.SqlServer => new SqlConnection(_dbConfig.ConnectionString()),
            DatabaseType.Sqlite => new SqliteConnection(_dbConfig.ConnectionString()),
            _ => throw new ArgumentNullException("A DatabaseType was not specified for the DatabaseConfiguration.")
        };

        _con.Open();
    }

    protected void OpenTransaction() => _con.BeginTransaction();

    protected string FieldsFromAttributesOf<T>()
    {
        string queryFields = "";

        foreach (PropertyInfo prop in typeof(T).GetPropsWithCustomType<DatabaseColumn>())
        {
            DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();
            queryFields += $"{(string.IsNullOrWhiteSpace(queryFields) ? "" : ",")} {propAttr.Name}";
        }

        return queryFields;
    }

    protected string FieldsFromMappedFields(params string[] fields)
    {
        if (fields == null)
            throw new ArgumentNullException("You have to specify the fields for the query.");

        if (fields.Length == 0)
            throw new ArgumentNullException("You have to specify the fields for the query.");

        string queryFields = "";

        foreach (string field in fields)
        {
            queryFields += $"{(string.IsNullOrWhiteSpace(queryFields) ? "" : ",")} {field}";
        }

        return queryFields;
    }

    protected IDbCommand CreateCommand()
        => _dbConfig.DatabaseType switch
        {
            DatabaseType.SqlServer => new SqlCommand("", (SqlConnection)_con, (SqlTransaction)_transaction),
            DatabaseType.Sqlite => new SqliteCommand("", (SqliteConnection?)_con, (SqliteTransaction?)_transaction),
        };

    protected IDbCommand CreateCommand(string query)
    {
        if(string.IsNullOrWhiteSpace(query)) 
            throw new ArgumentNullException("The query string can not be null nor empty");

        return _dbConfig.DatabaseType switch
        {
            DatabaseType.SqlServer => new SqlCommand(query, (SqlConnection)_con, (SqlTransaction?)_transaction),
            DatabaseType.Sqlite => new SqliteCommand(query, (SqliteConnection?)_con, (SqliteTransaction?)_transaction),
        };
    }

    public void Dispose()
    {
        if (_transaction != null) _transaction.Dispose();

        if (_con == null) 
            return;

        _con.Close();
        _con.Dispose();
    }

    protected void SaveChanges()
    {
        _transaction.Commit();
    }
}
