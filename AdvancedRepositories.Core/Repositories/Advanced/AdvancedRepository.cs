using AdvancedRepositories.Core.Configuration;
using System.Data.SqlClient;
using System.Reflection;

namespace AdvancedRepositories.Core.Repositories.Advanced;

public abstract class AdvancedRepository : BaseRepository
{
    protected AdvancedRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig)
    {
    }

    protected AdvancedCommand CreateAdvancedCommand(string query = "")
    {
        SqlCommand cmd = new SqlCommand(query, _con, _transaction);
        return new AdvancedCommand(cmd, this);
    }

    public class AdvancedCommand
    {
        AdvancedRepository _advancedRepository;
        SqlCommand _cmd;

        public AdvancedCommand(SqlCommand cmd, AdvancedRepository advancedRepository)
        {
            _cmd = cmd;
            _advancedRepository = advancedRepository;
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

        public AdvancedCommandReady WithParameters(Action<Dictionary<string, object>> sqlParameters = null)
        {
            FillParameters(_cmd, sqlParameters);
            return new AdvancedCommandReady(_cmd, _advancedRepository);
        }

        public AdvancedCommandReady NoParameters() => new AdvancedCommandReady(_cmd, _advancedRepository);
    }

    public class AdvancedCommandReady
    {
        AdvancedRepository _advancedRepository;
        SqlCommand _cmd;

        public AdvancedCommandReady(SqlCommand cmd, AdvancedRepository advancedRepository)
        {
            _cmd = cmd;
            _advancedRepository = advancedRepository;
        }

        public AdvancedCommandReady GetAdvancedCommand() => this;

        public DbResult Execute()
        {
            try
            {
                _cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
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
            }
            catch (Exception ex)
            {
                return DbResult.Exception<object>("Could not execute the command.", ex);
            }

            return DbResult.Ok(obj);
        }

        public DbResult<List<T>> GetList<T>(Action<Dictionary<string, string>> propertyDbNamePair) where T : class, new()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            propertyDbNamePair(map);

            List<T> list = new List<T>();

            try
            {
                SqlDataReader rdr = _cmd.ExecuteReader();

                while (rdr.Read())
                {
                    T obj = new T();

                    foreach (PropertyInfo p in typeof(T).GetProperties())
                    {
                        string propertyName = p.Name;
                        string dbPropName = "";
                        if (!map.TryGetValue(propertyName, out dbPropName))
                            continue;

                        Type type = p.PropertyType;
                        object value = rdr[dbPropName];

                        p.SetValue(obj, Convert.ChangeType(value, p.PropertyType), null);
                    }

                    list.Add(obj);
                }
            }
            catch (Exception ex)
            {
                return DbResult.Exception<List<T>>("There was an exception while trying to retrieve the data.", ex);
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
                SqlDataReader rdr = _cmd.ExecuteReader();

                while (rdr.Read())
                {
                    obj = new T();

                    foreach (PropertyInfo p in typeof(T).GetProperties())
                    {
                        string propertyName = p.Name;
                        string dbPropName = "";
                        if (!map.TryGetValue(propertyName, out dbPropName))
                            continue;

                        Type type = p.PropertyType;
                        object value = rdr[dbPropName];

                        p.SetValue(obj, Convert.ChangeType(value, p.PropertyType), null);
                    }

                    return DbResult.Ok(obj);
                }
            }
            catch (Exception ex)
            {
                return DbResult.Exception<T>("There was an exception while trying to retrieve the data.", ex);
            }

            return DbResult.Ok(obj ?? default);
        }

    }
}
