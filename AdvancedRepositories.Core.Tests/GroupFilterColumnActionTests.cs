using AdvancedRepositories.Core.Repositories.Fluent;
using FluentAssertions;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Tests;

// Naming convention [MethodUnderTest]_[Scenario]_[ExpectedResult]
public class GroupFilterColumnActionTests
{
    [Fact]
    public void GroupFilter_GroupConditionOrGroupCondition_Success()
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

    [Fact]
    public void GroupFilter_GroupConditionAndGroupCondition_Success()
    {
        // Given
        string expectedQuery = "WHERE ( Id IS NOT NULL AND Price <= @Price0 ) AND ( Id = @Id1 AND Name LIKE @Name2 )";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder
            .GroupFilter(x => {
                x.Add(y => y.ColumnName("Id").NotNull().And("Price").LessOrEqualTo("500"));
            })
            .AndGroupFilter(x => {
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

    [Fact]
    public void GroupFilter_NestedGroupCondition_Success()
    {
        // Given
        string expectedQuery = "WHERE ( ( Id = @Id0 AND Name LIKE @Name1 ) OR ( Color = @Color2 OR Color = @Color3 ) AND Quantity > @Quantity4 ) OR ( Price IS NOT NULL AND Price <= @Price5 )";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder
            .GroupFilter(x => {
                x.Add(y =>
                    y.GroupFilter(z =>
                        z.Add(a => a.ColumnName("Id").EqualTo("1").And("Name").Like("Chair")))
                    .OrGroupFilter(z =>
                        z.Add(a => a.And("Color").EqualTo("Dark brown").Or("Color").EqualTo("White"))
                        )
                    );

                x.Add(y => y.And("Quantity").GreaterThan("1"));
            })
            .OrGroupFilter(x => {
                x.Add(y => y.ColumnName("Price").NotNull().And("Price").LessOrEqualTo("500"));
            });

        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Select(x => x.ParameterName).Should().Equal("@Id0", "@Name1", "@Color2", "@Color3", "@Quantity4", "@Price5");
        parameters.Select(x => x.Value).Should().Equal("1", "%Chair%", "Dark brown", "White", "1", "500");
    }
}
