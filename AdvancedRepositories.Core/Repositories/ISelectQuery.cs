namespace AdvancedRepositories.Core.Repositories.Fluent;

internal interface ISelectQuery
{
    FluentQueryBuilder<T> Select<T>(params string[] fields) where T : class, new();
    FluentQueryBuilder<T> SelectDistinct<T>(params string[] fields) where T : class, new();
    FluentQueryBuilder<T> Select<T>() where T : class, new();
    FluentQueryBuilder<T> SelectDistinct<T>() where T : class, new();
}