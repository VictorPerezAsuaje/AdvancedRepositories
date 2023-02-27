namespace AdvancedRepositories.Core.Repositories.Interfaces;

public interface IReadByIdRepository<T> where T : class
{
    DbResult<T> GetById(int id);  
}
