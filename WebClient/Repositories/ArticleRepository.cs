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
    DbResult<List<Article>> FindClassic(string title);
    DbResult<List<Article>> FindAdvanced(Action<QueryFilterBuilder> filter);
    DbResult Insert(Article article);
    DbResult Update(Article article, int id);
    DbResult Delete(int id);
}

public class ArticleRepository : AdvancedRepository, IArticleRepository
{
    public ArticleRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig){}

    public DbResult<List<Article>> FindClassic(string title)
    {
        List<Article> articles = new List<Article>();        

        try
        {
            using(SqlConnection con = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=RepositoriesTestDb;Trusted_Connection=True;MultipleActiveResultSets=true;"))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;

                cmd.CommandText = "SELECT Id, Titulo, Slug, FechaCreacion, FechaPublicacion FROM Articulos";

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
                        CreatedOn = rdr.GetValueType<DateTime>("FechaCreacion"),
                        PublicationDate = rdr.TypeOrNull<DateTime>("FechaPublicacion")
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

    public DbResult<List<Article>> FindAdvanced(Action<QueryFilterBuilder> filter)
        => Select<Article>()
            .From("Articulos")
            .Where(filter)
            .GetList(x =>
            {
                x.Add("Id", "Id");
                x.Add("Title", "Titulo");
                x.Add("Slug", "Slug");
                x.Add("CreatedOn", "FechaCreacion");
                x.Add("PublicationDate", "FechaPublicacion");
            });

    public DbResult Insert(Article article)
    {
        DbResult<object> result = InsertInto("Articulos")
            .FieldValues(
                ("Titulo", article.Title), 
                ("Slug", article.Title.Replace(" ", "-")), 
                ("FechaCreacion", DateTime.Now))             
            .ExecuteScalar();

        /* Add other commands / actions / IO before confirming the previous command */

        if (result.IsSuccess) SaveChanges();

        /* Add other commands that may use the object returned from the insert */

        return result;
    }

    public DbResult Update(Article article, int id)
        => UpdateFrom("Articulos")
            .FieldValues(
                ("Titulo", article.Title),
                ("Slug", article.Title.Replace(" ", "-")))
            .Where(x => x.ColumnName("Id").EqualTo(id.ToString()))
            .Execute();

    public DbResult Delete(int id)
        => DeleteFrom("Articulos")
        .Where(x => x.ColumnName("Id").EqualTo(id.ToString()))
        .Execute();
}
