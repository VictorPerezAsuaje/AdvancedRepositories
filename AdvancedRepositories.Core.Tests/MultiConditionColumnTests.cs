using AdvancedRepositories.Core.Repositories.Fluent;
using FluentAssertions;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Tests;

public class MultiConditionColumnTests
{
    [Fact]
    public void And_GreaterAndLessThan_Success()
    {
        // Given
        string expectedQuery = "WHERE Id > @Id0 AND Price < @Price1";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").GreaterThan("1").And("Price").LessThan("2");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Select(x => x.ParameterName).Should().Equal("@Id0", "@Price1");
        parameters.Select(x => x.Value).Should().Equal("1", "2");
    }

    [Fact]
    public void Or_InOrEqualTo_Success()
    {
        // Given
        string expectedQuery = "WHERE Id IN ( @Id0, @Id1, @Id2, @Id3 ) OR Name = @Name4";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").In("1", "2", "3", "4").Or("Name").EqualTo("Victor");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Select(x => x.ParameterName).Should().Equal("@Id0", "@Id1", "@Id2", "@Id3","@Name4");
        parameters.Select(x => x.Value).Should().Equal("1", "2", "3", "4", "Victor");
    }
}