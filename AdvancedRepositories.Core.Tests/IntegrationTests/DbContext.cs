using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories.Fluent;
using Microsoft.Data.Sqlite;

namespace AdvancedRepositories.Core.Tests.IntegrationTests;

internal class DbContext
{
    public const string CONSTRING = "Data Source=Test.db;Mode=Memory;Cache=Shared";
    public static bool TableRecreated { get; set; } = false;

    static void CreateTestTable(string connectionString, bool createdBefore)
    {
        try
        {
            using (SqliteConnection con = new SqliteConnection(connectionString))
            {
                con.Open();

                if (createdBefore)
                {
                    string dropSql = @"DROP TABLE TestTable";

                    using (SqliteCommand cmd = new SqliteCommand(dropSql, con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                string createSql = @"CREATE TABLE TestTable (Id INT PRIMARY KEY NOT NULL, Name NVARCHAR(150), Age INT NOT NULL, RegistrationDate DATETIME NOT NULL)";

                using (SqliteCommand cmd = new SqliteCommand(createSql, con))
                {
                    cmd.ExecuteNonQuery();
                }

                string insertSql = @"INSERT INTO TestTable (Id, Name, Age, RegistrationDate) VALUES 
                                        (1, 'Victor', 26, '2023-01-01 00:00:00'), 
                                        (2, 'Alejandro', 27, '2023-01-02 00:00:00'), 
                                        (3, NULL, 28, '2023-01-03 00:00:00'), 
                                        (4, 'Asuaje', 29, '2023-01-04 00:00:00')";

                using (SqliteCommand cmd = new SqliteCommand(insertSql, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {

        }
    }

    static bool TableExists(string connectionString)
    {
        object obj = null;
        using (SqliteConnection con = new SqliteConnection(connectionString))
        {
            con.Open();
            string sql = "SELECT COUNT(name) FROM sqlite_master WHERE type='table' AND name='TestTable'";
            using (SqliteCommand cmd = new SqliteCommand(sql, con))
            {
                obj = cmd.ExecuteScalar();
            }
        }

        return Convert.ToInt32(obj) != 0;
    }

    static void EnsureTableCreated() 
    {
        bool tableExists = TableExists(CONSTRING);

        if (!tableExists || !TableRecreated)
        {
            CreateTestTable(CONSTRING, tableExists);
            TableRecreated = true;
        }
    }

    public static FluentRepository GenerateFluentRepository()
    {
        EnsureTableCreated();
        return new FluentRepository(GetBaseDbConfig());
    }

    public static BaseDatabaseConfiguration GetBaseDbConfig()
    {
        EnsureTableCreated();

        var db = new DatabaseConfiguration()
        {
            DatabaseType = DatabaseType.Sqlite,
        };

        db.AddConnectionStringManually(CONSTRING);

        return db;
    }
}
