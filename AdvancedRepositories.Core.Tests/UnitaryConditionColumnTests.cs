using AdvancedRepositories.Core.Repositories.Fluent;
using FluentAssertions;
using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Tests;

// Naming convention [MethodUnderTest]_[Scenario]_[ExpectedResult]

public class UnitaryConditionColumnTests
{
    [Fact]
    public void Not_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id NOT @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").Not("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

    [Fact]
    public void Is_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id IS @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").Is("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

    [Fact]
    public void IsNull_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id IS NULL";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").IsNull();
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().BeEmpty();
    }

    [Fact]
    public void NotNull_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id IS NOT NULL";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").NotNull();
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().BeEmpty();
    }

    [Fact]
    public void NotIn_MultipleParameters_Success()
    {
        // Given
        string expectedQuery = "WHERE Id NOT IN ( @Id0, @Id1, @Id2, @Id3 )";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").NotIn("1", "2", "3", "4");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);

        parameters.Select(x => x.ParameterName).Should().Equal("@Id0","@Id1", "@Id2", "@Id3");
        parameters.Select(x => x.Value).Should().Equal("1", "2", "3", "4");
    }

    [Fact]
    public void In_MultipleParameters_Success()
    {
        // Given
        string expectedQuery = "WHERE Id IN ( @Id0, @Id1, @Id2, @Id3 )";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").In("1", "2", "3", "4");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Select(x => x.ParameterName).Should().Equal("@Id0", "@Id1", "@Id2", "@Id3");
        parameters.Select(x => x.Value).Should().Equal("1", "2", "3", "4");
    }

    [Fact]
    public void Like_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id LIKE @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").Like("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Select(x => x.ParameterName).Should().Equal("@Id0");
        parameters.Select(x => x.Value).Should().Equal("%1%");
    }

    [Fact]
    public void Between_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id BETWEEN @Min0 AND @Max1";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").Between("1", "2");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Select(x => x.ParameterName).Should().Equal("@Min0", "@Max1");
        parameters.Select(x => x.Value).Should().Equal("1", "2");
    }

    [Fact]
    public void EqualsTo_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id = @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").EqualTo("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

    [Fact]
    public void LessThan_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id < @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").LessThan("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

    [Fact]
    public void GreaterThan_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id > @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").GreaterThan("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

    [Fact]
    public void NotEqual_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id <> @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").NotEqual("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

    [Fact]
    public void LessOrEqualTo_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id <= @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").LessOrEqualTo("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

    [Fact]
    public void GreaterOrEqualTo_OneParameter_Success()
    {
        // Given
        string expectedQuery = "WHERE Id >= @Id0";
        SqlCommand cmdToFill = new();
        QueryFilterBuilder builder = new(cmdToFill);

        // When
        builder.ColumnName("Id").GreaterOrEqualTo("1");
        string result = builder.GetCondition();
        List<SqlParameter> parameters = cmdToFill.Parameters.Cast<SqlParameter>().ToList();

        // Then
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo(expectedQuery);
        parameters.Should().Contain(x => x.ParameterName == "@Id0" && x.Value == "1");
    }

}
