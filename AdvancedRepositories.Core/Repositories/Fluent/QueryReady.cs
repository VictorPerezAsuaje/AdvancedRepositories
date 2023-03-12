using AdvancedRepositories.Core.Extensions;
using FluentRepositories.Attributes;
using System.Reflection;
using System.Data;

namespace AdvancedRepositories.Core.Repositories.Fluent;

public enum MapperOption { ColumnAttribute, PropertyName, MappedProps };
public class QueryReady<T> where T : class, new()
{
    FluentQueryBuilder<T> _queryBuilder;
    string _conditions = "";
    string _orderBy = "";
    IDbCommand _CommandWithQuery => _queryBuilder.GetCommandWithQuery(_conditions, _orderBy);
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

    public QueryReady<T> OrderByDesc(string columnName)
    {
        if (!string.IsNullOrWhiteSpace(_orderBy)) return this;

        _orderBy = $" ORDER BY {columnName} DESC ";
        return this;
    }

    public QueryReady<T> OrderBy(string columnName)
    {
        if (!string.IsNullOrWhiteSpace(_orderBy)) return this;

        _orderBy = $" ORDER BY {columnName} ASC ";
        return this;
    }

    /* Possible actions */
    public class FallbackMapperOptions
    {
        public bool ByColumnAttribute { get; set; } = false;
        public bool ByPropertyName { get; set; } = false;
        public Action<Dictionary<string, string>>? MapPropertyToDbField { get; set; }
    }

    /// <summary>
    /// <para>Gets a list of items mapping it by all means specified. The order of mapping is:</para>
    /// <list "number">
    ///    <item>
    ///        <description>If mapping fields are specified, use mapping fields.</description>
    ///    </item>
    ///    <item>
    ///        <description>If can not found using mapping fields, fallback to DatabaseColumn Attribute</description>
    ///    </item>
    ///    <item>
    ///        <description>If can not found using the attribute, try mapping by property name</description>
    ///    </item>
    ///</list>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public DbResult<List<T>> GetList(Action<FallbackMapperOptions> options)
    {
        List<T> list = new List<T>();

        FallbackMapperOptions mapper = new();
        options(mapper);

        try
        {
            if (!mapper.ByPropertyName && !mapper.ByColumnAttribute)
                throw new ArgumentNullException("No mapper option specified.");

            Dictionary<string, string>? mapByFields = null;

            if (mapper.MapPropertyToDbField != null)
            {
                mapByFields = new();
                mapper.MapPropertyToDbField(mapByFields);
            }


            IDataReader rdr = _CommandWithQuery.ExecuteReader();

            List<string> columns = Enumerable.Range(0, rdr.FieldCount).Select(x => rdr.GetName(x)).ToList();

            while (rdr.Read())
            {
                T item = new T();
                bool hasProperties = false;

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (mapByFields != null)
                    {
                        if (mapByFields.TryGetValue(prop.Name, out string dbPropName))
                        {
                            prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, dbPropName));
                            hasProperties = true;
                            continue;
                        }
                    }

                    DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();

                    if (mapper.ByColumnAttribute && propAttr != null)
                    {
                        if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                        prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, propAttr.Name));
                        hasProperties = true;
                        continue;
                    }

                    if (mapper.ByPropertyName)
                    {
                        if (!columns.Contains(prop.Name)) continue;

                        prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, prop.Name));
                        hasProperties = true;
                        continue;
                    }
                }

                if (hasProperties) list.Add(item);
            }
        }
        catch(Exception ex)
        {
            return DbResult.Exception<List<T>>("There was an exception while getting the items.", ex);
        }

        return DbResult.Ok(list);
    }

    /// <summary>
    /// <para>Method to return a list of items of type T implicitly based on the attributes [DefaultTable] and [ColumnName]. </para>
    /// 
    /// <para>You must have at least one [ColumnName] attribute on a property of the T type, otherwise it returns an ArgumentNullException. The same happens if you do not define the [DefaultTable] attribute for the class.</para>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public DbResult<List<T>> GetList(bool byPropertyName = false)
    {
        if (byPropertyName) return GetListByPropertyName();

        List<T> list = new List<T>();

        try
        {
            if(!_queryBuilder.ContainsQueryFields)
                throw new ArgumentNullException("No database fields specified. \nYou have to  specify fields on the Select<T>( -- Your fields here -- ) and use a generic action, for instance, GetList(). \nOtherwise, leave generic Select<T>() and specify them at the selected action, for instance, GetList(x => { -- Your fields here -- }).");

            IDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                T item = new T();
                bool hasProperties = false;

                foreach (PropertyInfo prop in typeof(T).GetPropsWithCustomType<DatabaseColumn>())
                {
                    DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();

                    if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                    prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, propAttr.Name));
                    hasProperties = true;
                }

                if (hasProperties) list.Add(item);
            }
        }
        catch(Exception ex)
        {
            return DbResult.Exception<List<T>>("There was an exception while getting the items.", ex);
        }
        
        return DbResult.Ok(list);
    }

    /// <summary>
    /// <para>Method to return a list of items of type T with explicit specifications on the fields you want to query. </para>
    /// <para>If you have specified fields in the Select<T>(), they will be overriden by the mapped fields specified within this method.</para>
    /// </summary>
    /// <param name="propertyDbNamePair"></param>
    /// <returns></returns>
    public DbResult<List<T>> GetList(Action<Dictionary<string, string>> propertyDbNamePair)
    {
        List<T> list = new List<T>();

        try
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            propertyDbNamePair(map);

            _queryBuilder.ReplaceQueryFieldsWithMappedFields(map.Values.ToList());

            IDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                T item = new T();
                bool hasProperties = false;

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (!map.TryGetValue(prop.Name, out string dbPropName))
                        continue;

                    prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, dbPropName));
                    hasProperties = true;
                }

                if(hasProperties) list.Add(item);
            }
        }
        catch (Exception ex)
        {
            return DbResult.Exception<List<T>>("There was an exception while getting the items.", ex);
        }

        return DbResult.Ok(list);
    }

    /// <summary>
    /// <para>Method to return a list of items of type T mapping field to property name automatically. </para>
    /// </summary>
    /// <param name="defaultToDbField"></param>
    /// <returns></returns>
    internal DbResult<List<T>> GetListByPropertyName()
    {
        List<T> list = new List<T>();

        try
        {
            if (!_queryBuilder.ContainsQueryFields)
                throw new ArgumentNullException("No database fields specified. \nYou have to  specify fields on the Select<T>( -- Your fields here -- ) and use a generic action, for instance, GetList(). \nOtherwise, leave generic Select<T>() and specify them at the selected action, for instance, GetList(x => { -- Your fields here -- }).");

            IDataReader rdr = _CommandWithQuery.ExecuteReader();

            List<string> columns = Enumerable.Range(0, rdr.FieldCount).Select(x => rdr.GetName(x)).ToList();

            while (rdr.Read())
            {
                T item = new T();
                bool hasProperties = false;

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (!columns.Contains(prop.Name)) continue;

                    prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, prop.Name));
                    hasProperties = true;
                }

                if (hasProperties) list.Add(item);
            }
        }
        catch (Exception ex)
        {
            return DbResult.Exception<List<T>>("There was an exception while getting the items.", ex);
        }


        return DbResult.Ok(list);
    }


    /// <summary>
    /// <para>Gets an item mapping it by all means specified. The order of mapping is:</para>
    /// <list "number">
    ///    <item>
    ///        <description>If mapping fields are specified, use mapping fields.</description>
    ///    </item>
    ///    <item>
    ///        <description>If can not found using mapping fields and you have activated the DatabaseColumn, fallback to DatabaseColumn Attribute</description>
    ///    </item>
    ///    <item>
    ///        <description>If can not found using the attribute, try mapping by property name</description>
    ///    </item>
    ///</list>
    /// 
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public DbResult<T> GetOne(Action<FallbackMapperOptions> options)
    {
        T item = null;

        FallbackMapperOptions mapper = new();
        options(mapper);

        try
        {
            if (!mapper.ByPropertyName && !mapper.ByColumnAttribute)
                throw new ArgumentNullException("No mapper option specified.");

            Dictionary<string, string>? mapByFields = null;

            if (mapper.MapPropertyToDbField != null)
            {
                mapByFields = new();
                mapper.MapPropertyToDbField(mapByFields);
            }


            IDataReader rdr = _CommandWithQuery.ExecuteReader();

            List<string> columns = Enumerable.Range(0, rdr.FieldCount).Select(x => rdr.GetName(x)).ToList();

            while (rdr.Read())
            {
                if (item != null)
                    return DbResult.Fail<T>("The query returned more than one item with the criteria specified");

                bool hasProperties = false;


                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (mapByFields != null)
                    {
                        if (mapByFields.TryGetValue(prop.Name, out string dbPropName))
                        {
                            prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, dbPropName));
                            hasProperties = true;
                            continue;
                        }
                    }

                    DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();

                    if (mapper.ByColumnAttribute && propAttr != null)
                    {
                        if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                        prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, propAttr.Name));
                        hasProperties = true;
                        continue;
                    }

                    if (mapper.ByPropertyName)
                    {
                        if (!columns.Contains(prop.Name)) continue;

                        prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, prop.Name));
                        hasProperties = true;
                        continue;
                    }
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

    /// <summary>
    /// <para>Method to return an item of type T implicitly based on the attributes [DefaultTable] and [ColumnName]. </para>
    /// 
    /// <para>You must have at least one [ColumnName] attribute on a property of the T type, otherwise it returns an ArgumentNullException. The same happens if you do not define the [DefaultTable] attribute for the class.</para>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public DbResult<T> GetOne(bool byPropertyName = false)
    {
        if (byPropertyName) return GetOneByPropertyName();

        T item = null;

        try
        {
            if (!_queryBuilder.ContainsQueryFields)
                throw new ArgumentNullException("No database fields specified. \nYou have to  specify fields on the Select<T>( -- Your fields here -- ) and use a generic action, for instance, GetList(). \nOtherwise, leave generic Select<T>() and specify them at the selected action, for instance, GetList(x => { -- Your fields here -- }).");

            IDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                if (item != null) 
                    return DbResult.Fail<T>("The query returned more than one item with the criteria specified");

                item = new T();

                foreach (PropertyInfo prop in typeof(T).GetPropsWithCustomType<DatabaseColumn>())
                {
                    DatabaseColumn propAttr = prop.GetCustomAttribute<DatabaseColumn>();

                    if (!_queryBuilder.ContainsQueryField(propAttr.Name)) continue;

                    prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, propAttr.Name));
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

    /// <summary>
    /// <para>Method to return an item of type T mapping field to property name automatically. </para>
    /// </summary>
    /// <param name="propertyDbNamePair"></param>
    /// <returns></returns>
    internal DbResult<T> GetOneByPropertyName()
    {
        T item = null;

        try
        {
            if (!_queryBuilder.ContainsQueryFields)
                throw new ArgumentNullException("No database fields specified. \nYou have to  specify fields on the Select<T>( -- Your fields here -- ) and use a generic action, for instance, GetList(). \nOtherwise, leave generic Select<T>() and specify them at the selected action, for instance, GetList(x => { -- Your fields here -- }).");

            IDataReader rdr = _CommandWithQuery.ExecuteReader();

            List<string> columns = Enumerable.Range(0, rdr.FieldCount).Select(x => rdr.GetName(x)).ToList();

            while (rdr.Read())
            {
                if (item != null)
                    return DbResult.Fail<T>("The query returned more than one item with the criteria specified");

                item = new T();

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (!columns.Contains(prop.Name)) continue;

                    prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, prop.Name));
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

    /// <summary>
    /// <para>Method to return an items of type T with explicit specifications on the fields you want to query. </para>
    /// </summary>
    /// <param name="propertyDbNamePair"></param>
    /// <returns></returns>
    public DbResult<T> GetOne(Action<Dictionary<string, string>> propertyDbNamePair)
    {
        T item = null;

        try
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            propertyDbNamePair(map);

            _queryBuilder.ReplaceQueryFieldsWithMappedFields(map.Values.ToList());

            IDataReader rdr = _CommandWithQuery.ExecuteReader();
            while (rdr.Read())
            {
                if (item != null) 
                    return DbResult.Fail<T>("The query returned more than one item with the criteria specified");

                item = new T();

                foreach (PropertyInfo prop in typeof(T).GetProperties())
                {
                    if (!map.TryGetValue(prop.Name, out string dbPropName))
                        continue;

                    prop.SetValue(item, rdr.TypeOrNull(prop.PropertyType, dbPropName));
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


