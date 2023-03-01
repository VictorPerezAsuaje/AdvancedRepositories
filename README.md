# AdvancedRepositories

AdvancedRepositories born as a way to simplify the process of creating a repository with ADO.NET by providing middlewares, base classes, interfaces, etc.

It also provides a way to even avoid creating one altogether if all you want is filling a Data Transfer Object (DTO) data, by using the FluentRepository.

| TABLE OF CONTENT |
| ------ |
| [Initial Setup](#initial-setup) | 
| [Using IFluentRepository](#using-ifluentrepository) | 
| &nbsp;&nbsp;&nbsp;&nbsp;[Attributes + IFluentRepository](#attributes-ifluentrepository) | 
| [Inheriting from AdvancedRepository](#inheriting-from-advancedrepository) | 
| [QueryFilterBuilder](#QueryFilterBuilder) | 


## Initial setup

1. Install the nuget package by using the NuGet Package Manager, Nuget CLI or NET CLI.
```sh
  // Nuget CLI
  NuGet\Install-Package AdvancedRepositories
  
  // NET CLI
  dotnet add package AdvancedRepositories
```

2. Go to your Program.cs and add the following line with your database configuration.
```sh
  builder.Services.AddRepositoriesConfiguration(c =>
    {
        c.Server = ""; // Required
        c.DbName = ""; // Required
        c.User = ""; // Optional
        c.Password = ""; // Optional
    });
```

3. Now you can either create your repositories by inheriting from the AdvancedRepository class or use the IFluentRepository interface to retrieve data.

### Using IFluentRepository

In your controller, add a property for IFluentRepository and pass it via constructor for dependency injection.

```sh
public class YourController : Controller
{
    IFluentRepository _fluentRepo;
    
    public YourController(IFluentRepository fluentRepository) => _fluentRepo = fluentRepository;

    /* --- Other actions --- */
```

**And that's all!** If you don't want to set up any extra attributes you can use it right away to load a list of the element you want as shown below as an example. I have used a Spanish names for columns in the database, whereas I used english names for the class properties to make it - hopefully - clearer. We will talk about the attributes right after.

Let's assume the following class:

```sh
public class TagDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

```sh
DbResult<List<TagDTO>> result = _fluentRepo.Select<TagDTO>()
    .From("Etiquetas")
    .GetList(x =>
    {
        // Map class property name to database field
        // x.Add("ClassPropertyName", "DatabaseField");
        x.Add("Id", "Id");
        x.Add("Name", "Nombre");
    });
```

The DbResult class allows you to check whether or not the request successfully completed. If so, you will end up with a list of your desired class (in this case a TagDTO) that you can access through the .Value property, otherwise, you will get an error message with an exception you can log.

```sh
if(!result.IsSuccess){
    // Log the error by using result.Error or result.ExceptionContent depending on the type of failure
    return;
}

List<TagDTO> tags = result.Value;
```

#### Attributes + IFluentRepository

Now let's talk about attributes. The AdvancedRepositories comes with two attributes you can use in your Data Transfer Objects to simplify the process of getting the data. Following the example show previously for the TagDTO, we can also add attributes to the class and it's properties to represent both the default table name and map its columns:

```sh
// Remember: I used Spanish for table and column names.
[DefaultTable("Etiquetas")]
public class TagDTO
{
    [DatabaseColumn("Id")]
    public int Id { get; set; }

    [DatabaseColumn("Nombre")]
    public string Name { get; set; }
}
```

So now we can replicate the previous sample query as follows:
```sh
DbResult<List<TagDTO>> result = _fluentRepo.Select<TagDTO>().FromDefaultTable().GetList();
```


### Inheriting from AdvancedRepository

But there will be times when you will need to use other actions aside from reading data, right? That's when you might prefer to get more control over your queries by creating your own repository and inheriting from the AdvancedRepository class.

In the following example I'm going to create an ArticleRepository based on the Article class.

```sh
public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public DateTime CreatedOn { get; set; }
}
```

I will also create an interface that contains two Read methods to contrast the classic way to get data with ADO.NET vs using the AdvancedRepository, as well as an Insert method as a representative example of other CRUD actions.

```sh
public interface IArticleRepository 
{
    DbResult<List<Article>> FindMultipleClassic(string title);
    DbResult<List<Article>> FindMultipleAdvanced(Action<QueryFilterBuilder> filter);
    DbResult Insert(Article article);
}
```

```sh
public class ArticleRepository : AdvancedRepository, IArticleRepository
{
    public ArticleRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig){}

    // "Usual" way of setting up a find/get data with filters
    public DbResult<List<Article>> FindMultipleClassic(string title)
    {
        List<Article> articles = new List<Article>();        

        try
        {
            using(SqlConnection con = new SqlConnection("{YOUR-CONNECTION-STRING}"))
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
    
     // "Advanced" way of setting up a find/get data with filters
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

        if(result.IsSuccess) SaveChanges(); // Applies all commands until this point

        /* Add other commands that may need to use the object returned from the insert */

        return result;
    }
}
```

### QueryFilterBuilder

The QueryFilterBuilder class allows the repository to create filters for queries in a fluent fashion. You may have already noticed their appearance in the example shown in "Inheriting from AdvancedRepository" for the FindMultiple methods. It can be used both by the FluentRepository as well as by the AdvancedRepositories.

You can create quite complex filters by using it, so here I leave a few examples:

```sh

// Executed query:
// SELECT Nombre FROM Etiquetas WHERE Id = @Id2 OR Id BETWEEN @Min0 AND @Max1
_fluentRepo.Select<TagDTO>("Nombre")
    .FromDefaultTable()
    .AndWhere(a => a.ColumnName("Id").Between("10", "14"))
    .OrWhere(a => a.ColumnName("Id").EqualTo("9"))
    .OrderByDesc("Id")
    .GetList();
    
// Executed query:
// SELECT DISTINCT Nombre FROM Etiquetas WHERE (Nombre = @Nombre0 AND Id = @Id1) OR (Id = @Id2 AND Nombre = @Nombre3)
_fluentRepo.Select<TagDTO>("Nombre")
    .FromDefaultTable()
    .AndWhere(a => a.ColumnName("Id").Between("10", "14"))
    .OrWhere(a => a.ColumnName("Id").EqualTo("9"))
    .OrderByDesc("Id")
    .GetList();
```