using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Extensions;
using FluentRepositories.Attributes;
using System.Data.SqlClient;
using System.Reflection;

namespace AdvancedRepositories.Core.Repositories.Fluent;

// It's internal because it should only have FluentRepository as its implementation for the singleton, otherwise it could be implemented on another class and it may cause an exception due to not being able to find the appropriate service
internal interface IFluentRepository : ISelectQuery {
    DbResult<List<T>> AutoList<T>(Action<QueryFilterBuilder>? filterConfig = null) where T : class, new();
}

public sealed class FluentRepository : BaseRepository, IFluentRepository
{
    public FluentRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig)
    {
    }

    public DbResult<List<T>> AutoList<T>(Action<QueryFilterBuilder>? filterConfig = null) where T : class, new()
    {
        List<T> list = new List<T>();

        try
        {
            string queryFields = FieldsFromAttributesOf<T>();

            if (string.IsNullOrWhiteSpace(queryFields))
                throw new ArgumentNullException($"The class {typeof(T).Name} does not have any [DatabaseColumn] attribute defined.");

            DefaultTable? tableAttr = Attribute.GetCustomAttribute(typeof(T), typeof(DefaultTable)) as DefaultTable;

            if (tableAttr == null) 
                throw new ArgumentNullException($"The class {typeof(T).Name} does not have a [DefaultTable] attribute defined.");

            SqlCommand cmd = CreateCommand($"SELECT {queryFields} FROM {tableAttr.Name} ");

            if(filterConfig != null)
            {
                var filter = new QueryFilterBuilder(cmd);
                filterConfig(filter);
                cmd = filter.GetCommandWithFilter();
            }        

        
            SqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                T item = new T();

                foreach (PropertyInfo prop in typeof(T).GetPropsWithCustomType<DatabaseColumn>())
                {
                    DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();
                    prop.SetValue(item, rdr[propAttr.Name], null);
                }

                list.Add(item);
            }
        }
        catch (Exception ex)
        {
            return DbResult.Exception<List<T>>("There was an exception while getting the items.", ex);
        }

        return DbResult.Ok(list);
    }

    public FluentQueryBuilder<T> Select<T>(params string[] fields) where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromMappedFields(fields)} "));

    public FluentQueryBuilder<T> SelectDistinct<T>(params string[] fields) where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT DISTINCT {FieldsFromMappedFields(fields)} "));

    public FluentQueryBuilder<T> Select<T>() where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT {FieldsFromAttributesOf<T>()} "));

    public FluentQueryBuilder<T> SelectDistinct<T>() where T : class, new()
        => new FluentQueryBuilder<T>(CreateCommand($"SELECT DISTINCT {FieldsFromAttributesOf<T>()} "));
}
