namespace AdvancedRepositories.Core.Repositories.Interfaces;

public interface IDeleteRepository<T>
{
    DbResult Delete(int id);
}