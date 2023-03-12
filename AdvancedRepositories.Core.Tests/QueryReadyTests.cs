using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories.Fluent;
using FluentAssertions;

namespace AdvancedRepositories.Core.Tests;

public class QueryReadyTests
{
    private FluentRepository GenerateFluentRepository()
    {
        BaseDatabaseConfiguration bdb = new DatabaseConfiguration();
        bdb.DatabaseType = DatabaseType.InMemory;
        bdb.AddConnectionStringManually("Data Source=testDb.db");
        return new FluentRepository(bdb);
    }

    [Fact]
    public void Select_From_Success()
    {
        // Given
        string expectedQuery = "SELECT Id, Name FROM TestTable ";
        FluentRepository fluentRepository = GenerateFluentRepository();

        // When 
        string result = fluentRepository.Select<AttributedClass>()
            .From("TestTable")
            .GetBuiltQuery();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
    }

    [Fact]
    public void Select_FromDefaultTable_Success()
    {
        // Given
        string expectedQuery = "SELECT Id, Name FROM TestTable ";
        FluentRepository fluentRepository = GenerateFluentRepository();

        // When 
        string result = fluentRepository.Select<AttributedClass>()
            .FromDefaultTable()
            .GetBuiltQuery();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
    }

    [Fact]
    public void SelectDistinct_From_Success()
    {
        // Given
        string expectedQuery = "SELECT DISTINCT Id, Name FROM TestTable ";
        FluentRepository fluentRepository = GenerateFluentRepository();

        // When 
        string result = fluentRepository.SelectDistinct<AttributedClass>()
            .From("TestTable")
            .GetBuiltQuery();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
    }

    [Fact]
    public void SelectDistinct_FromDefaultTable_Success()
    {
        // Given
        string expectedQuery = "SELECT DISTINCT Id, Name FROM TestTable ";
        FluentRepository fluentRepository = GenerateFluentRepository();

        // When 
        string result = fluentRepository.SelectDistinct<AttributedClass>()
            .FromDefaultTable()
            .GetBuiltQuery();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
    }

}
