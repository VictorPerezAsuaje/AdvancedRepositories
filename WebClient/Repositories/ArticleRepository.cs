using AdvancedRepositories.Core.Configuration;
using AdvancedRepositories.Core.Extensions;
using AdvancedRepositories.Core.Repositories;
using AdvancedRepositories.Core.Repositories.Interfaces;
using WebClient.Models;
using System.ComponentModel;
using System.Data.SqlClient;

namespace WebClient.Infrastructure;
public interface IArticleRepository 
{
    DbResult<List<Article>> GetAll();
    DbResult Insert(Article article);
}

public class ArticleRepository : BaseRepository, IArticleRepository
{
    public ArticleRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig){}

    public DbResult<List<Article>> GetAll()
        => DbResult.Ok(
            CreateReader("SELECT Id, Titulo, Slug, FechaCreacion FROM Articulos")
            .GetList<Article>(x =>
            {
                x.Add("Id", "Id");
                x.Add("Title", "Titulo");
                x.Add("Slug", "Slug");
                x.Add("CreatedOn", "FechaCreacion");
            }));

    public DbResult Insert(Article article)
        => CreateCommand("INSERT INTO Articulos (Titulo, Slug, FechaCreacion) VALUES (@Titulo, @Slug, @FechaCreacion)", 
            x =>
            {
                x.Add("@Titulo", article.Title);
                x.Add("@Slug", article.Title.Replace(" ", "-"));
                x.Add("@FechaCreacion", DateTime.Now);
            }).Execute();
}
