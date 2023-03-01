using AdvancedRepositories.Core.Repositories.Fluent;
using FluentAssertions;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Tests;

// Naming convention [MethodUnderTest]_[Scenario]_[ExpectedResult]
public class GroupFilterColumnActionTests
{
    [Fact]
    public void GroupFilter_AndConditionOrCondition_Success()
    {
        // Given
        string expectedQuery = "WHERE ( Id IS NOT NULL AND Price <= @Price0 ) OR ( Id = @Id1 AND Name LIKE @Name2 )";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder
            .GroupFilter(x => {
                x.Add(y => y.ColumnName("Id").NotNull().And("Price").LessOrEqualTo("500"));
            })
            .OrGroupFilter(x => {
                x.Add(y => y.ColumnName("Id").EqualTo("1").And("Name").Like("Victor"));
            });

        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Select(x => x.ParameterName).Should().Equal("@Price0", "@Id1", "@Name2");
        parameters.Select(x => x.Value).Should().Equal("500", "1", "%Victor%");
    }

}
