namespace AdvancedRepositories.Core.Repositories.Fluent;

/// <summary>
///  Work in progress
/// </summary>
/// <typeparam name="T"></typeparam>
internal class FluentView<T> where T : class, new()
{
    string _From = "";

    internal FluentView(Action<FluentView<T>> viewConfig)
    {
        viewConfig(this);
    }

    internal class FluentViewTable
    {
        FluentView<T> _View;
        string _Join;
        internal FluentViewTable(FluentView<T> view, string join)
        {
            _View = view;
            _Join = join;
        }

        internal FluentView<T> On(string condition)
        {
            _View.AddToFrom($" {_Join} ON {condition} ");
            return _View;
        }
    }

    internal FluentViewTable Join(string table, string? alias = null)
        => new FluentViewTable(this, $" JOIN {table} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ");

    internal FluentViewTable LeftJoin(string table, string? alias = null)
        => new FluentViewTable(this, $" LEFT JOIN {table} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ");

    internal FluentViewTable RightJoin(string table, string? alias = null)
        => new FluentViewTable(this, $" RIGHT JOIN {table} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ");


    internal void AddToFrom(string data) => _From += data;
    internal string GetFrom() => _From;
}
