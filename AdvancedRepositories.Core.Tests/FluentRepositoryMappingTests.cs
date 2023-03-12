using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Repositories.Fluent;
using FluentAssertions;
using FluentRepositories.Attributes;
using Microsoft.Data.Sqlite;

namespace AdvancedRepositories.Core.Tests;

[DefaultTable("TestTable")]
public class AttributedClass
{
    [DatabaseColumn("Id")]
    public int Id { get; set; }

    [DatabaseColumn("Name")]
    public string? Name { get; set; }
    public int Age { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string FieldDoesNotExistInDb { get; set; } = "Field does not exist in db";
}

public class FluentRepositoryMappingTests
{
    private void CreateTestTable(string connectionString)
    {
        try
        {
            using (SqliteConnection con = new SqliteConnection(connectionString))
            {
                con.Open();
                string sql = @"CREATE TABLE TestTable (
                                    Id INT PRIMARY KEY NOT NULL, 
                                    Name NVARCHAR(150), 
                                    Age INT NOT NULL,
                                    RegistrationDate DATETIME NOT NULL
                                    
                    )";
                SqliteCommand cmd = new SqliteCommand(sql, con);
                cmd.ExecuteNonQuery();

                string insertSql = @"INSERT INTO TestTable (Id, Name, Age, RegistrationDate) VALUES 
                                        (1, 'Victor',), 
                                        (2, 'Alejandro'), 
                                        (3, NULL), 
                                        (4, 'Asuaje')";
                cmd = new SqliteCommand(insertSql, con);
                cmd.ExecuteNonQuery();
            }

        }
        catch (Exception ex)
        {

        }
    }

    private bool TableExists(string connectionString)
    {
        object obj = null;
        using (SqliteConnection con = new SqliteConnection(connectionString))
        {
            con.Open();
            string sql = "SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name='TestTable'";
            SqliteCommand cmd = new SqliteCommand(sql, con);
            obj = cmd.ExecuteScalar();
        }

        return Convert.ToInt32(obj) != 0;
    }

    private FluentRepository GenerateFluentRepository()
    {
        BaseDatabaseConfiguration bdb = new DatabaseConfiguration();
        bdb.DatabaseType = DatabaseType.InMemory;
        string connectionString = "Data Source=testDb.db";
        bdb.AddConnectionStringManually(connectionString);

        if (!TableExists(connectionString))
            CreateTestTable(connectionString);

        return new FluentRepository(bdb);
    }

    static FluentRepository? fluentRepository;

    public FluentRepositoryMappingTests()
    {
        fluentRepository = fluentRepository ?? GenerateFluentRepository();        
    }

    [Fact]
    public void AutoList_Success()
    {
        // Given
        List<AttributedClass> expected = new()
        {
            new(){ Id = 1, Name = "Victor" },
            new(){ Id = 2, Name = "Alejandro" },
            new(){ Id = 3, Name = "Perez" },
            new(){ Id = 4, Name = "Asuaje" },
        };

        // When 
        DbResult<List<AttributedClass>> result = fluentRepository.AutoList<AttributedClass>();

        // Then
        result.IsSuccess.Should().Be(true);
        result.Value.Should().HaveCount(expected.Count);
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetList_ByColumnAttributes_Success()
    {
        // Given
        List<AttributedClass> expected = new()
        {
            new(){ Id = 1, Name = "Victor" },
            new(){ Id = 2, Name = "Alejandro" },
            new(){ Id = 3, Name = "Perez" },
            new(){ Id = 4, Name = "Asuaje" },
        };

        // When 
        DbResult<List<AttributedClass>> result = fluentRepository.Select<AttributedClass>()
            .From("TestTable")
            .GetList();

        // Then
        result.IsSuccess.Should().Be(true);
        result.Value.Should().HaveCount(expected.Count);
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetList_ByPropertyNames_Success()
    {
        // Given
        List<AttributedClass> expected = new()
        {
            new(){ Id = 1, Name = "Victor" },
            new(){ Id = 2, Name = "Alejandro" },
            new(){ Id = 3, Name = "Perez" },
            new(){ Id = 4, Name = "Asuaje" },
        };

        // When 
        DbResult<List<AttributedClass>> result = fluentRepository.Select<AttributedClass>()
            .From("TestTable")
            .GetList(true);

        // Then
        result.IsSuccess.Should().Be(true);
        result.Value.Should().HaveCount(expected.Count);
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetList_ByMappingFields_Success()
    {
        // Given
        List<AttributedClass> expected = new()
        {
            new(){ Id = 1, Name = "Victor" },
            new(){ Id = 2, Name = "Alejandro" },
            new(){ Id = 3, Name = "Perez" },
            new(){ Id = 4, Name = "Asuaje" },
        };

        // When 
        DbResult<List<AttributedClass>> result = fluentRepository.Select<AttributedClass>()
            .From("TestTable")
            .GetList(x =>
            {
                x.Add("Id", "Id");
                x.Add("Name", "Name");
            });

        // Then
        result.IsSuccess.Should().Be(true);
        result.Value.Should().HaveCount(expected.Count);
        result.Value.Should().BeEquivalentTo(expected);
    }
}
