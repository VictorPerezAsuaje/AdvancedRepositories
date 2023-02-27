namespace AdvancedRepositories.Core.Repositories.Interfaces;

public interface ICreateRepository<T>
{
    DbResult Create(T entity);
}
