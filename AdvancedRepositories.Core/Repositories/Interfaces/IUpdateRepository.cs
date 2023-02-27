namespace AdvancedRepositories.Core.Repositories.Interfaces;

public interface IUpdateRepository<T>
{
    DbResult Update(int id, T updatedEntity);
}
