using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Extensions;
using FluentRepositories.Attributes;
using System.Data.SqlClient;
using System.Reflection;

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

    protected string FieldsFromAttributesOf<T>()
    {
        string queryFields = "";

        foreach (PropertyInfo prop in typeof(T).GetProperties().OfCustomType<DatabaseColumn>())
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

    protected SqlCommand CreateCommand() 
        => new SqlCommand("", _con, _transaction);

    protected SqlCommand CreateCommand(string query)
    {
        if(string.IsNullOrWhiteSpace(query)) 
            throw new ArgumentNullException("The query string can not be null nor empty");

        return new SqlCommand(query, _con, _transaction); ;
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
