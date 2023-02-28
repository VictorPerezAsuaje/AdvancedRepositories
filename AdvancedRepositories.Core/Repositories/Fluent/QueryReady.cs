using FluentRepositories.Attributes;
using System.Data.SqlClient;
using System.Reflection;

namespace AdvancedRepositories.Core.Repositories.Fluent;

public class QueryReady<T> where T : class, new()
{
    FluentQueryBuilder<T> _queryBuilder;
    string _conditions = "";
    string _orderBy = "";

    internal QueryReady(FluentQueryBuilder<T> queryBuilder)
    {
        _queryBuilder = queryBuilder;
    }

    public QueryReady<T> AndWhere(Action<QueryFilterBuilder> filterConfig)
    {
        var filter = new QueryFilterBuilder(_queryBuilder._cmd);
        filterConfig(filter);

        _conditions = filter.ApplyAndCondition(_conditions);
        return this;
    }

    public QueryReady<T> OrWhere(Action<QueryFilterBuilder> filterConfig)
    {
        var filter = new QueryFilterBuilder(_queryBuilder._cmd);
        filterConfig(filter);

        _conditions = filter.ApplyOrCondition(_conditions);
        return this;
    }

    bool ColumnExist(string columnName)
    {
        foreach (PropertyInfo prop in typeof(T).GetProperties())
        {
            DatabaseColumn propAttr = prop.GetCustomAttribute(typeof(DatabaseColumn)) as DatabaseColumn;

            if (propAttr?.Name == columnName) return true;
        }

        return false;
    }

    public QueryReady<T> OrderByDesc(string columnName)
    {
        if (string.IsNullOrWhiteSpace(_orderBy)) return this;
        if (!ColumnExist(columnName)) return this;

        _orderBy = $" ORDER BY {columnName} DESC ";
        return this;
    }

    public QueryReady<T> OrderBy(string columnName)
    {
        if (string.IsNullOrWhiteSpace(_orderBy)) return this;
        if (!ColumnExist(columnName)) return this;

        _orderBy = $" ORDER BY {columnName} ASC ";
        return this;
    }


    /* Map column names for joined tables */

    public QueryReady<T> RemoveColumnName(string columnName)
    {
        if (!ColumnExist(columnName)) return this;

        _queryBuilder.OverrideColumnNames($", {columnName}", "");
        _queryBuilder.OverrideColumnNames(columnName, "");
        return this;
    }

    public QueryReady<T> MapColumnName(string columnName, string newColumnName, string? alias = null)
    {
        if (!ColumnExist(columnName)) return this;

        _queryBuilder.OverrideColumnNames(columnName, newColumnName, alias);
        return this;
    }

    /* Possible actions */

    public List<T> GetList()
    {
        List<T> list = new List<T>();

        SqlDataReader rdr = _queryBuilder.BuildQuery(_conditions, _orderBy).ExecuteReader();
        while (rdr.Read())
        {
            T item = new T();

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                DatabaseColumn propAttr = prop.GetCustomAttribute(typeof(DatabaseColumn)) as DatabaseColumn;

                if (propAttr == null) continue;

                if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                prop.SetValue(item, rdr[propAttr.Name], null);
            }

            list.Add(item);
        }

        return list;
    }

    public List<T> GetByFieldName(string columnName, object columnValue)
    {
        List<T> list = new List<T>();

        _conditions += $" {(_conditions.Contains("WHERE") ? "AND" : "WHERE")} AND {columnName} = @fieldValue ";

        SqlCommand cmd = _queryBuilder.BuildQuery(_conditions, _orderBy);
        cmd.Parameters.AddWithValue("@fieldValue", columnValue);

        SqlDataReader rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            T item = new T();

            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                DatabaseColumn propAttr = prop.GetCustomAttribute(typeof(DatabaseColumn)) as DatabaseColumn;

                if (propAttr == null) continue;

                if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                prop.SetValue(item, rdr[propAttr.Name], null);
            }

            list.Add(item);
        }

        return list;
    }

    public T GetOneById(string columnIdName, object columnIdValue)
    {
        T item = null;

        _conditions += $" {(_conditions.Contains("WHERE") ? "AND" : "WHERE")} {columnIdName} = @fieldValue ";

        SqlCommand cmd = _queryBuilder.BuildQuery(_conditions, _orderBy);
        cmd.Parameters.AddWithValue("@fieldValue", columnIdValue);

        SqlDataReader rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            if (item != null) return item;

            item = new T();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                DatabaseColumn propAttr = prop.GetCustomAttribute(typeof(DatabaseColumn)) as DatabaseColumn;

                if (propAttr == null) continue;

                if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;


                prop.SetValue(item, rdr[propAttr.Name], null);
            }
        }

        return item;
    }
}


