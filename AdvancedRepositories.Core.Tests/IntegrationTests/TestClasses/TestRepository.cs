using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Repositories.Advanced;

namespace AdvancedRepositories.Core.Tests.IntegrationTests.TestClasses;


public class TestRepository : AdvancedRepository
{
    public TestRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig)
    {
    }

    public DbResult<List<AttributedClass>> GetList_ByColumnAttributes()
        => Select<AttributedClass>()
            .From("TestTable")
            .GetList();

    public DbResult<List<AttributedClass>> GetList_ByColumnAttributesAndDefaultTable()
        => Select<AttributedClass>()
            .FromDefaultTable()
            .GetList();

    public DbResult<List<AttributedClass>> GetList_ByPropertyNames()
        => Select<AttributedClass>()
            .From("TestTable")
            .GetList(true);

    public DbResult<List<AttributedClass>> GetList_ByMappingFields()
        => Select<AttributedClass>()
            .From("TestTable")
            .GetList(x =>
            {
                x.Add("Id", "Id");
                x.Add("Name", "Name");
                x.Add("RegistrationDate", "RegistrationDate");
            });


}