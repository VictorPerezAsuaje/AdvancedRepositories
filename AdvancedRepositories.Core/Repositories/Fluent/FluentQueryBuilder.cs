using AdvancedRepositories.Core.Extensions;
using FluentRepositories.Attributes;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Repositories.Fluent;

public class FluentQueryBuilder<T> where T : class, new()
{
    internal SqlCommand _cmd;
    internal QueryReady<T> _queryReady;
    internal string _from;
    string _queryFields;
    string? _overridenQueryFields;
    string _top;

    public FluentQueryBuilder(SqlCommand cmd, string queryFields)
    {
        _cmd = cmd;
        _queryFields = queryFields;
    }

    public FluentQueryBuilder<T> Top(int number)
    {
        if (_top != null) return this;
        if (number < 1) return this;

        _top = $" TOP({number}) ";
        _queryFields = _queryFields.Replace("SELECT", $"SELECT {_top} ");
        return this;
    }

    public QueryReady<T>? FromDefaultTable()
    {
        DefaultTable tableAttr = Attribute.GetCustomAttribute(typeof(T), typeof(DefaultTable)) as DefaultTable;

        if (tableAttr == null) return null;

        _from = $" FROM {tableAttr.Name} ";
        return new QueryReady<T>(this);
    }

    public QueryReady<T> From(string table)
    {
        _from = $" FROM {table} ";
        return new QueryReady<T>(this);
    }

    public QueryReady<T> From(string table, string alias, Action<FluentView<T>> viewConfig)
    {
        _from = $" FROM {table} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ";
        FluentView<T> view = new FluentView<T>(viewConfig);
        _from += view.GetFrom();
        return new QueryReady<T>(this);
    }

    internal SqlCommand BuildQuery(string conditions, string orderBy)
    {
        _cmd.CommandText = $"{_overridenQueryFields ?? _queryFields} {_from} {conditions} {orderBy}".ClearMultipleSpaces();
        return _cmd;
    }

    internal bool ContainsQueryField(string fieldName)
        => (_overridenQueryFields ?? _queryFields).Contains(fieldName);
    
    internal bool ContainsQueryFields()
        => (_overridenQueryFields ?? _queryFields)
        .Replace("SELECT", "").Replace("DISTINCT", "").Trim()
        .Length > 0;

    internal void ReplaceQueryFieldsWithMappedFields(List<string> dbFields)
    {
        if (dbFields.Count == 0 && !ContainsQueryFields())
            throw new ArgumentNullException("No database fields specified. \nYou have to specify fields on the Select<T>( -- Your fields here -- ) and use a generic action, for instance, GetList(). \nOtherwise, leave generic Select<T>() and specify them at the selected action, for instance, GetList(x => { -- Your fields here -- }).");

        string rewrittenQuery = "SELECT ";

        if (_queryFields.Contains("DISTINCT")) 
            rewrittenQuery += "DISTINCT ";

        for(int i = 0; i < dbFields.Count; i++)
        {
            rewrittenQuery += $"{(i == 0 ? "" : ", ")} {dbFields[i]}";
        }

        _queryFields = rewrittenQuery;
    }

    internal void OverrideColumnNames(string columnName, string newColumnName, string? alias = null)
    {
        if (!string.IsNullOrWhiteSpace(_overridenQueryFields))
        {
            _overridenQueryFields = $"{_overridenQueryFields.Replace(columnName, newColumnName)} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ";
            return;
        }

        _overridenQueryFields = $"{_queryFields.Replace(columnName, newColumnName)} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ";
    }
}
