namespace AdvancedRepositories.Core.Repositories.Interfaces;

public interface IReadRepository<T> : IReadByIdRepository<T>, IReadAllRepository<T> where T : class
{
}
