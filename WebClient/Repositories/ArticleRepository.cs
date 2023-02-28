using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories;
using WebClient.Models;
using AdvancedRepositories.Core.Repositories.Advanced;
using AdvancedRepositories.Core.Repositories.Fluent;
using System.Data.SqlClient;
using AdvancedRepositories.Core.Extensions;

namespace WebClient.Infrastructure;
public interface IArticleRepository 
{
    DbResult<List<Article>> FindMultipleClassic(string title);
    DbResult<List<Article>> FindMultipleAdvanced(Action<QueryFilterBuilder> filter);
    DbResult Insert(Article article);
}

public class ArticleRepository : AdvancedRepository, IArticleRepository
{
    public ArticleRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig){}

    public DbResult<List<Article>> FindMultipleClassic(string title)
    {
        List<Article> articles = new List<Article>();        

        try
        {
            using(SqlConnection con = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=RepositoriesTestDb;Trusted_Connection=True;MultipleActiveResultSets=true;"))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = "SELECT Id, Titulo, Slug, FechaCreacion FROM Articulos";

                if(!string.IsNullOrWhiteSpace(title))
                {
                    cmd.CommandText += " WHERE Titulo LIKE @Title";
                    cmd.Parameters.AddWithValue("@Title", "%"+title+"%");
                }

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    articles.Add(new Article()
                    {
                        Id = rdr.GetValueType<int>("Id"),
                        Slug = rdr.GetValueType<string>("Slug"),
                        Title = rdr.GetValueType<string>("Titulo"),
                        CreatedOn = rdr.GetValueType<DateTime>("FechaCreacion")
                    });
                }
            }            
        }
        catch (Exception ex)
        {
            return DbResult.Exception<List<Article>>("Exception");
        }

        return DbResult.Ok(articles);
    }
    public DbResult<List<Article>> FindMultipleAdvanced(Action<QueryFilterBuilder> filter)
        => CreateAdvancedCommand("SELECT Id, Titulo, Slug, FechaCreacion FROM Articulos")
            .ApplyFilter(filter)
            .GetList<Article>(x =>
            {
                x.Add("Id", "Id");
                x.Add("Title", "Titulo");
                x.Add("Slug", "Slug");
                x.Add("CreatedOn", "FechaCreacion");
            });

    public DbResult Insert(Article article)
    {
        DbResult<object> result = CreateAdvancedCommand("INSERT INTO Articulos (Titulo, Slug, FechaCreacion) OUTPUT Inserted.ID VALUES (@Titulo, @Slug, @FechaCreacion)")
            .WithParameters(x =>
            {
                x.Add("@Titulo", article.Title);
                x.Add("@Slug", article.Title.Replace(" ", "-"));
                x.Add("@FechaCreacion", DateTime.Now);
            })
            .ExecuteScalar();

        /* Add other commands / actions / IO before confirming the previous command */

        if(result.IsSuccess) SaveChanges();

        /* Add other commands that may use the object returned from the insert */

        return result;
    }
}
