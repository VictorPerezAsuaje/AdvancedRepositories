using AdvancedRepositories.Core.Configuration;
using FluentRepositories.Attributes;
using FluentRepositories;
using System.Reflection;

namespace AdvancedRepositories.Core.Repositories.Fluent;

public interface ISelectQuery
{
    FluentQueryBuilder<T> Select<T>(params string[] fields) where T : class, new();
    FluentQueryBuilder<T> SelectDistinct<T>(params string[] fields) where T : class, new();
    FluentQueryBuilder<T> Select<T>() where T : class, new();
    FluentQueryBuilder<T> SelectDistinct<T>() where T : class, new();
}

public interface IFluentRepository : ISelectQuery { }

public class FluentRepository : BaseRepository, IFluentRepository
{
    public FluentRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig)
    {
    }

    public FluentQueryBuilder<T> Select<T>(params string[] fields) where T : class, new()
    {
        string queryFields = "";

        foreach (string field in fields)
        {
            queryFields += $"{(string.IsNullOrWhiteSpace(queryFields) ? "" : ",")} {field}";
        }

        return new FluentQueryBuilder<T>(CreateCommand(""), $"SELECT {queryFields} ");
    }

    public FluentQueryBuilder<T> SelectDistinct<T>(params string[] fields) where T : class, new()
    {
        string queryFields = "";

        foreach (string field in fields)
        {
            queryFields += $"{(string.IsNullOrWhiteSpace(queryFields) ? "" : ",")} {field}";
        }

        return new FluentQueryBuilder<T>(CreateCommand(""), $"SELECT DISTINCT {queryFields} ");
    }

    public FluentQueryBuilder<T> Select<T>() where T : class, new()
    {
        string queryFields = "";

        foreach (PropertyInfo prop in typeof(T).GetProperties())
        {
            DatabaseColumn propAttr = prop.GetCustomAttribute(typeof(DatabaseColumn)) as DatabaseColumn;
            if (propAttr == null) continue;

            queryFields += $"{(string.IsNullOrWhiteSpace(queryFields) ? "" : ",")} {propAttr.Name}";
        }

        return new FluentQueryBuilder<T>(CreateCommand(""), $"SELECT {queryFields} ");
    }    

    public FluentQueryBuilder<T> SelectDistinct<T>() where T : class, new()
    {
        string queryFields = "";

        foreach (PropertyInfo prop in typeof(T).GetProperties())
        {
            DatabaseColumn propAttr = prop.GetCustomAttribute(typeof(DatabaseColumn)) as DatabaseColumn;
            if (propAttr == null) continue;

            queryFields += $"{(string.IsNullOrWhiteSpace(queryFields) ? "" : ",")} {propAttr.Name}";
        }

        return new FluentQueryBuilder<T>(CreateCommand(), $"SELECT DISTINCT {queryFields} ");
    }
}
