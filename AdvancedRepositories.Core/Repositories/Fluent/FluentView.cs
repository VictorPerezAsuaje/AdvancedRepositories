namespace AdvancedRepositories.Core.Repositories.Fluent;

public class FluentView<T> where T : class, new()
{
    string _From = "";

    public FluentView(Action<FluentView<T>> viewConfig)
    {
        viewConfig(this);
    }

    public class FluentViewTable
    {
        FluentView<T> _View;
        string _Join;
        public FluentViewTable(FluentView<T> view, string join)
        {
            _View = view;
            _Join = join;
        }

        public FluentView<T> On(string condition)
        {
            _View.AddToFrom($" {_Join} ON {condition} ");
            return _View;
        }
    }

    public FluentViewTable Join(string table, string? alias = null)
        => new FluentViewTable(this, $" JOIN {table} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ");

    public FluentViewTable LeftJoin(string table, string? alias = null)
        => new FluentViewTable(this, $" LEFT JOIN {table} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ");

    public FluentViewTable RightJoin(string table, string? alias = null)
        => new FluentViewTable(this, $" RIGHT JOIN {table} {(!string.IsNullOrWhiteSpace(alias) ? $"AS {alias}" : "")} ");


    internal void AddToFrom(string data) => _From += data;
    internal string GetFrom() => _From;
}
