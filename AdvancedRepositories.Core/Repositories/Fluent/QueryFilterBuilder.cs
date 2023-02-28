using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Repositories.Fluent;

public class QueryFilterBuilder
{
    SqlCommand _cmd { get; set; }
    int _ParamNumber => _cmd.Parameters.Count;
    string _Where = "";

    public QueryFilterBuilder(SqlCommand cmd) => _cmd = cmd;

    public QueryFilterBuilder(SqlCommand cmd, Action<QueryFilterBuilder> filterConfig)
    {
        _cmd = cmd;
        filterConfig(this);
    }

    void AddParameterWithValue(string parameterName, string value)
        => _cmd.Parameters.AddWithValue(parameterName, value);

    void AddToWhere(string content)
    {
        _Where += $" {content} ";
        _Where = _Where.Replace("  ", " ");
    }

    public class Column
    {
        QueryFilterBuilder _Filter;
        string _Name { get; set; }
        string _Comparer { get; set; }

        public Column(QueryFilterBuilder filter, string name, string comparer)
        {
            _Filter = filter;
            _Name = name;
            _Comparer = comparer;
        }

        public QueryFilterBuilder Not(string valor)
        {
            _Filter.AddToWhere($"{_Comparer} {_Name} NOT {valor}");
            return _Filter;
        }

        public QueryFilterBuilder Is(string valor)
        {
            _Filter.AddToWhere($"{_Comparer} {_Name} IS {valor}");
            return _Filter;
        }

        public QueryFilterBuilder IsNull()
        {
            _Filter.AddToWhere($"{_Comparer} {_Name} IS NULL");
            return _Filter;
        }

        public QueryFilterBuilder NotNull()
        {
            _Filter.AddToWhere($"{_Comparer} {_Name} NOT NULL");
            return _Filter;
        }

        public QueryFilterBuilder NotIn(params string[] valores)
        {
            if (valores.Length == 0) return _Filter;

            _Filter.AddToWhere($"{_Comparer} {_Name} NOT IN (");

            string inValues = "";
            for (int i = 0; i < valores.Length; i++)
            {
                inValues += $"{(i == 0 ? "" : ", ")}@{_Name}{_Filter._ParamNumber}";
                _Filter.AddParameterWithValue($"@{_Name}{_Filter._ParamNumber}", valores[i]);
            }

            _Filter.AddToWhere($"{inValues} )");
            return _Filter;
        }

        public QueryFilterBuilder In(params string[] valores)
        {
            if (valores.Length == 0) return _Filter;

            _Filter.AddToWhere($"{_Comparer} {_Name} IN (");

            string inValues = "";
            for (int i = 0; i < valores.Length; i++)
            {
                inValues += $"{(i == 0 ? "" : ", ")}@{_Name}{_Filter._ParamNumber}";
                _Filter.AddParameterWithValue($"@{_Name}{_Filter._ParamNumber}", valores[i]);
            }

            _Filter.AddToWhere($"{inValues} )");
            return _Filter;
        }

        private string CreateSimpleComparer(string comparer)
            => $" {_Comparer} {_Name} {comparer} @{_Name}{_Filter._ParamNumber}";

        private void ComparerWithParams(string comparer, string assignedValue)
        {
            _Filter.AddToWhere(CreateSimpleComparer(comparer));
            _Filter.AddParameterWithValue($"@{_Name}{_Filter._ParamNumber}", assignedValue);
        }

        public QueryFilterBuilder Like(string assignedValue)
        {
            _Filter.AddToWhere(CreateSimpleComparer("LIKE"));
            _Filter.AddParameterWithValue($"@{_Name}{_Filter._ParamNumber}", "%" + assignedValue + "%");
            return _Filter;
        }

        public QueryFilterBuilder Between(string minValue, string maxValue)
        {
            _Filter.AddToWhere($"{_Comparer} {_Name} BETWEEN @Min{_Filter._ParamNumber}");
            _Filter.AddParameterWithValue($"@Min{_Filter._ParamNumber}", minValue);

            _Filter.AddToWhere($"AND @Max{_Filter._ParamNumber}");
            _Filter.AddParameterWithValue($"@Max{_Filter._ParamNumber}", maxValue);

            return _Filter;
        }

        public QueryFilterBuilder EqualTo(string assignedValue)
        {
            ComparerWithParams("=", assignedValue);
            return _Filter;
        }

        public QueryFilterBuilder LessThan(string assignedValue)
        {
            ComparerWithParams("<", assignedValue);
            return _Filter;
        }

        public QueryFilterBuilder GreaterThan(string assignedValue)
        {
            ComparerWithParams(">", assignedValue);
            return _Filter;
        }

        public QueryFilterBuilder NotEqual(string assignedValue)
        {
            ComparerWithParams("<>", assignedValue);
            return _Filter;
        }

        public QueryFilterBuilder LessOrEqualTo(string assignedValue)
        {
            ComparerWithParams("<=", assignedValue);
            return _Filter;
        }

        public QueryFilterBuilder GreaterOrEqualTo(string assignedValue)
        {
            ComparerWithParams("=>", assignedValue);
            return _Filter;
        }

    }

    string ModifierOrWhere(string modifier = "")
    {
        if (string.IsNullOrEmpty(_Where)) return "WHERE";

        if (_Where.Trim().EndsWith("(")) return "";

        return modifier;
    }


    public Column ColumnName(string column)
        => new Column(this, column, ModifierOrWhere());

    public Column And(string column)
        => new Column(this, column, ModifierOrWhere("AND"));

    public Column Or(string column)
        => new Column(this, column, ModifierOrWhere("OR"));

    public QueryFilterBuilder GroupFilter(Action<List<Action<QueryFilterBuilder>>> grupoFiltros)
    {
        List<Action<QueryFilterBuilder>> acciones = new List<Action<QueryFilterBuilder>>();
        grupoFiltros(acciones);

        if (acciones.Count == 0) return this;

        AddToWhere($"{ModifierOrWhere()} (");
        acciones.ForEach(x => x(this));
        AddToWhere(")");
        return this;
    }

    public QueryFilterBuilder AndGroupFilter(Action<List<Action<QueryFilterBuilder>>> grupoFiltros)
    {
        List<Action<QueryFilterBuilder>> acciones = new List<Action<QueryFilterBuilder>>();
        grupoFiltros(acciones);

        if (acciones.Count == 0) return this;

        AddToWhere($"{ModifierOrWhere("AND")} (");
        acciones.ForEach(x => x(this));
        AddToWhere(")");
        return this;
    }

    public QueryFilterBuilder OrGroupFilter(Action<List<Action<QueryFilterBuilder>>> grupoFiltros)
    {
        List<Action<QueryFilterBuilder>> acciones = new List<Action<QueryFilterBuilder>>();
        grupoFiltros(acciones);

        if (acciones.Count == 0) return this;

        AddToWhere($"{ModifierOrWhere("OR")} (");
        acciones.ForEach(x => x(this));
        AddToWhere(")");
        return this;
    }

    internal string ApplyAndCondition(string condition)
        => $"{_Where} {condition.Replace("WHERE", "AND")} ";

    internal string ApplyOrCondition(string condition)
        => $" {_Where} {condition.Replace("WHERE", "OR")} ";

    public SqlCommand GetCommandWithFilter()
    {
        _cmd.CommandText += $" {_Where} ";
        return _cmd;
    }
}
