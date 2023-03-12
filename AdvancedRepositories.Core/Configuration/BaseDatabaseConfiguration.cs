
namespace AdvancedRepositories.Core.Configuration;

public enum DatabaseType { InMemory, SqlServer }
public abstract class BaseDatabaseConfiguration
{
    public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;
    public string DbName { get; set; }
    public string Server { get; set; }
    public string? User { get; set; }
    public string? Password { get; set; }
    protected string? ManualConnectionString { get; set; } = null;

    public void AddConnectionStringManually(string connectionString) 
        => ManualConnectionString = connectionString;
    public abstract string ConnectionString();
    
}
