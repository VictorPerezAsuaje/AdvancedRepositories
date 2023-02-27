using AdvancedRepositories.Core.Configuration;
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

    SqlCommand FillParameters(SqlCommand cmd, Action<Dictionary<string, object>> sqlParameters)
    {
        Dictionary<string, object> parameters = new();
        if (sqlParameters != null) sqlParameters(parameters);

        foreach (var parameter in parameters)
        {
            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
        }

        return cmd;
    }

    protected BaseRepositoryCommand CreateCommand(string query = "", Action<Dictionary<string, object>> sqlParameters = null)
    {
        SqlCommand cmd = new SqlCommand(query, _con, _transaction);

        FillParameters(cmd, sqlParameters);

        return new BaseRepositoryCommand(cmd, this);
    }

    public class BaseRepositoryCommand
    {
        BaseRepository _baseRepository;
        SqlCommand _cmd;
        public BaseRepositoryCommand(SqlCommand cmd, BaseRepository baseRepository)
        {
            _cmd = cmd;
            _baseRepository = baseRepository;
        }


        public DbResult Execute()
        {
            try
            {
                _cmd.ExecuteNonQuery();
                _baseRepository.SaveChanges();
            }
            catch(Exception ex)
            {
                return DbResult.Exception("Could not execute the command.", ex);
            }

            return DbResult.Ok();
        }

        public DbResult<object> ExecuteScalar()
        {
            object obj = null;

            try
            {
                obj = _cmd.ExecuteScalar();
                _baseRepository.SaveChanges();
            }
            catch (Exception ex)
            {
                return DbResult.Exception<object>("Could not execute the command.", ex);
            }

            return DbResult.Ok(obj);
        }
    }

    protected BaseRepositoryReader CreateReader(string query = "", Action<Dictionary<string, object>> sqlParameters = null)
    {
        SqlCommand cmd = new SqlCommand(query, _con, _transaction);

        FillParameters(cmd, sqlParameters);

        return new BaseRepositoryReader(cmd.ExecuteReader());
    }

    public class BaseRepositoryReader
    {
        SqlDataReader _rdr;
        public BaseRepositoryReader(SqlDataReader rdr)
        {
            _rdr = rdr;
        }

        public DbResult<List<T>> GetList<T>(Action<Dictionary<string, string>> propertyDbNamePair) where T : class, new()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            propertyDbNamePair(map);

            List<T> list = new List<T>();

            try
            {
                while (_rdr.Read())
                {
                    T obj = new T();

                    foreach (PropertyInfo p in typeof(T).GetProperties())
                    {
                        string propertyName = p.Name;
                        string dbPropName = "";
                        if (!map.TryGetValue(propertyName, out dbPropName))
                            continue;

                        Type type = p.PropertyType;
                        object value = _rdr[dbPropName];

                        p.SetValue(obj, Convert.ChangeType(value, p.PropertyType), null);
                    }

                    list.Add(obj);
                }
            }
            catch (Exception ex)
            {
                return DbResult.Exception("There was an exception while trying to retrieve the data.", ex);
            }

            return DbResult.Ok(list);
        }

        public DbResult<T> GetOneOrDefault<T>(Action<Dictionary<string, string>> propertyDbNamePair) where T : class, new()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            propertyDbNamePair(map);

            T obj = null;

            try
            {
                while (_rdr.Read())
                {
                    obj = new T();

                    foreach (PropertyInfo p in typeof(T).GetProperties())
                    {
                        string propertyName = p.Name;
                        string dbPropName = "";
                        if (!map.TryGetValue(propertyName, out dbPropName))
                            continue;

                        Type type = p.PropertyType;
                        object value = _rdr[dbPropName];

                        p.SetValue(obj, Convert.ChangeType(value, p.PropertyType), null);
                    }

                    return DbResult.Ok(obj);
                }
            }
            catch (Exception ex)
            {
                return DbResult.Exception("There was an exception while trying to retrieve the data.", ex);
            }

            return DbResult.Ok(obj) ?? default(T);
        }
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
