using AdvancedRepositories.Core.Extensions;
using FluentRepositories.Attributes;
using System.Data.SqlClient;
using System.Reflection;

namespace AdvancedRepositories.Core.Repositories.Fluent;

public class QueryReady<T> where T : class, new()
{
    FluentQueryBuilder<T> _queryBuilder;
    string _conditions = "";
    string _orderBy = "";
    SqlCommand _CommandWithQuery => _queryBuilder.GetCommandWithQuery(_conditions, _orderBy);
    public string GetBuiltQuery() => _queryBuilder.BuildQuery(_conditions, _orderBy);

    internal QueryReady(FluentQueryBuilder<T> queryBuilder)
    {
        _queryBuilder = queryBuilder;
    }

    public QueryReady<T> Where(Action<QueryFilterBuilder> filterConfig)
    {
        var filter = new QueryFilterBuilder(_queryBuilder._cmd);
        filterConfig(filter);

        _conditions = filter.ApplyAndCondition(_conditions);
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
        if (!string.IsNullOrWhiteSpace(_orderBy)) return this;
        if (!ColumnExist(columnName)) return this;

        _orderBy = $" ORDER BY {columnName} DESC ";
        return this;
    }

    public QueryReady<T> OrderBy(string columnName)
    {
        if (!string.IsNullOrWhiteSpace(_orderBy)) return this;
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

    public DbResult<List<T>> GetList()
    {
        List<T> list = new List<T>();

        try
        {
            if(!_queryBuilder.ContainsQueryFields)
                throw new ArgumentNullException("No database fields specified. \nYou have to  specify fields on the Select<T>( -- Your fields here -- ) and use a generic action, for instance, GetList(). \nOtherwise, leave generic Select<T>() and specify them at the selected action, for instance, GetList(x => { -- Your fields here -- }).");

            SqlDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                T item = new T();

                foreach (PropertyInfo prop in typeof(T).GetPropsWithCustomType<DatabaseColumn>())
                {
                    DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();

                    if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                    prop.SetValue(item, rdr.GetValueType(prop.PropertyType, propAttr.Name));
                }

                list.Add(item);
            }
        }
        catch(Exception ex)
        {
            return DbResult.Exception<List<T>>("There was an exception while getting the items.", ex);
        }
        

        return DbResult.Ok(list);
    }

    public DbResult<List<T>> GetList(Action<Dictionary<string, string>> propertyDbNamePair)
    {
        List<T> list = new List<T>();

        try
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            propertyDbNamePair(map);

            _queryBuilder.ReplaceQueryFieldsWithMappedFields(map.Values.ToList());

            SqlDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                T item = new T();

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    string propertyName = prop.Name;

                    if (!map.TryGetValue(propertyName, out string dbPropName))
                        continue;

                    prop.SetValue(item, rdr.GetValueType(prop.PropertyType, dbPropName));
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

    public DbResult<T> GetOne()
    {
        T item = null;

        try
        {
            if (!_queryBuilder.ContainsQueryFields)
                throw new ArgumentNullException("No database fields specified. \nYou have to  specify fields on the Select<T>( -- Your fields here -- ) and use a generic action, for instance, GetList(). \nOtherwise, leave generic Select<T>() and specify them at the selected action, for instance, GetList(x => { -- Your fields here -- }).");

            SqlDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                if (item != null) 
                    return DbResult.Fail<T>("The query returned more than one item with the criteria specified");

                item = new T();

                foreach (PropertyInfo prop in typeof(T).GetPropsWithCustomType<DatabaseColumn>())
                {
                    DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();

                    if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                    prop.SetValue(item, rdr.GetValueType(prop.PropertyType, propAttr.Name));
                }
            }
        }
        catch (Exception ex)
        {
            return DbResult.Exception<T>("There was an exception while getting the item.", ex);
        }

        if (item == null) return DbResult.Fail<T>("There was no item that matched the criteria specified");

        return DbResult.Ok(item);
    }

    public DbResult<T> GetOne(Action<Dictionary<string, string>> propertyDbNamePair)
    {
        T item = null;

        try
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            propertyDbNamePair(map);

            _queryBuilder.ReplaceQueryFieldsWithMappedFields(map.Values.ToList());

            SqlDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                if (item != null) 
                    return DbResult.Fail<T>("The query returned more than one item with the criteria specified");

                item = new T();

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    string propertyName = prop.Name;

                    if (!map.TryGetValue(propertyName, out string dbPropName))
                        continue;

                    prop.SetValue(item, rdr.GetValueType(prop.PropertyType, dbPropName));
                }
            }
        }
        catch (Exception ex)
        {
            return DbResult.Exception<T>("There was an exception while getting the item.", ex);
        }

        if(item == null) return DbResult.Fail<T>("There was no item that matched the criteria specified");

        return  DbResult.Ok(item);
    }
}


