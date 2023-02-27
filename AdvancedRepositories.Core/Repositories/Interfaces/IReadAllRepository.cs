namespace AdvancedRepositories.Core.Repositories.Interfaces;

public interface IReadAllRepository<T> 
{
    DbResult<List<T>> GetAll();
}
