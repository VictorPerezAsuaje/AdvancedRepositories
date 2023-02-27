using System.Data.SqlClient;

namespace AdvancedRepositories.Core.Configuration;

public class DatabaseConfiguration : BaseDatabaseConfiguration
{
    public DatabaseConfiguration() { }
    public DatabaseConfiguration(string name, string server, string? user = null, string? password = null)
    {
        DbName = name;
        Server = server;
        User = user ?? "";
        Password = password ?? "";
    }

    public override string ConnectionString()
    {
        if(!string.IsNullOrEmpty(ManualConnectionString)) return ManualConnectionString;

        SqlConnectionStringBuilder conBuilder = new SqlConnectionStringBuilder();

        conBuilder.DataSource = Server;
        conBuilder.InitialCatalog = DbName;

        if (!string.IsNullOrEmpty(User) && !string.IsNullOrEmpty(Password))
        {
            conBuilder.PersistSecurityInfo = true;
            conBuilder.UserID = User;
            conBuilder.Password = Password;
        }
        else
        {
            conBuilder.IntegratedSecurity = true;
            conBuilder.MultipleActiveResultSets = true;
        }

        return conBuilder.ConnectionString;
    }
}