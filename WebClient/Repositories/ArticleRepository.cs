using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Repositories;
using WebClient.Models;
using AdvancedRepositories.Core.Repositories.Advanced;
using AdvancedRepositories.Core.Repositories.Fluent;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using AdvancedRepositories.Core.Extensions;

namespace WebClient.Infrastructure;
public interface IArticleRepository 
{
    DbResult<List<Article>> FindMultiple(Action<QueryFilterBuilder> filter);
    DbResult<List<Article>> GetAll();
    DbResult Insert(Article article);
}

public class ArticleRepository : AdvancedRepository, IArticleRepository
{
    public ArticleRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig){}

    public DbResult<List<Article>> FindMultiple(Action<QueryFilterBuilder> filter)
    {
        List<Article> articles = new List<Article>();

        try
        {
            SqlCommand cmd = CreateCommand("SELECT Id, Titulo, Slug, FechaCreacion FROM Articulos ").Filter(filter);


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
        catch (Exception ex)
        {
            return DbResult.Exception<List<Article>>("Exception");
        }

        return DbResult.Ok(articles);
    }

    public DbResult<List<Article>> GetAll()
        => CreateAdvancedCommand("SELECT Id, Titulo, Slug, FechaCreacion FROM Articulos")
            .NoParameters()
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
