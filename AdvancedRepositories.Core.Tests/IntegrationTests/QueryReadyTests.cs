using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories.Fluent;
using AdvancedRepositories.Core.Tests.IntegrationTests.TestClasses;
using FluentAssertions;

namespace AdvancedRepositories.Core.Tests.IntegrationTests;

public class QueryReadyTests
{
    private FluentRepository FluentRepository => DbContext.GenerateFluentRepository();

    [Fact]
    public void Select_From_Success()
    {
        // Given
        string expectedQuery = "SELECT Id, Name FROM TestTable ";

        // When 
        string result = FluentRepository.Select<AttributedClass>()
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

        // When 
        string result = FluentRepository.Select<AttributedClass>()
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

        // When 
        string result = FluentRepository.SelectDistinct<AttributedClass>()
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

        // When 
        string result = FluentRepository.SelectDistinct<AttributedClass>()
            .FromDefaultTable()
            .GetBuiltQuery();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
    }

    [Fact]
    public void Select_From_OrderBy_Success()
    {
        // Given
        string expectedQuery = "SELECT Id, Name FROM TestTable ORDER BY Id ASC ";

        // When 
        string result = FluentRepository.Select<AttributedClass>()
            .From("TestTable")
            .OrderBy("Id")
            .GetBuiltQuery();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
    }

    [Fact]
    public void Select_From_OrderByDesc_Success()
    {
        // Given
        string expectedQuery = "SELECT Id, Name FROM TestTable ORDER BY Id DESC ";

        // When 
        string result = FluentRepository.Select<AttributedClass>()
            .From("TestTable")
            .OrderByDesc("Id")
            .GetBuiltQuery();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
    }

}
