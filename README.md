# AdvancedRepositories

AdvancedRepositories born as a way to simplify the process of creating a repository with ADO.NET by providing middlewares, base classes, interfaces, etc.

It also provides a way to even avoid creating one altogether if all you want is filling a Data Transfer Object (DTO) data, by using the FluentRepository.

| TABLE OF CONTENT |
| ------ |
| [Initial Setup](#initial-setup) | 
| [Note about DbResult](#note-about-dbresult) | 
| [Using IFluentRepository](#using-ifluentrepository) | 
| &nbsp;&nbsp;&nbsp;&nbsp;[Attributes + IFluentRepository](#attributes-ifluentrepository) | 
| [Inheriting from AdvancedRepository](#inheriting-from-advancedrepository) | 
| &nbsp;&nbsp;&nbsp;&nbsp;[Create](#create) | 
| &nbsp;&nbsp;&nbsp;&nbsp;[Update](#update) | 
| &nbsp;&nbsp;&nbsp;&nbsp;[Delete](#delete) | 
| &nbsp;&nbsp;&nbsp;&nbsp;[Classic vs Advanced Read](#classic-vs-advanced-read) | 
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

Alternatively you can specify your connection manually:
```sh
  builder.Services.AddRepositoriesConfiguration(c => c.AddConnectionStringManually("{YOUR-CONNECTION-STRING}"));
```

3. Now you can either create your repositories by inheriting from the AdvancedRepository class or use the IFluentRepository interface to retrieve data.

### Note about DbResult

The DbResult class will be used everywhere in the project and examples. It allows you to check whether or not the request successfully completed. It can be both typed and untyped.

```sh
  // Typed 
  public DbResult<Article> GetOne();

  // Untyped
  public DbResult Update();
```

It does not matter if it's typed or not, you will have 3 possible results (that you can extend if you want and create your own if you feel like it):

```sh

  // The request was successfull
  DbResult.Ok()
  DbResult.Ok<T>(/* T instance */)

  // The request failed
  DbResult.Fail("Error related to failure")
  DbResult.Fail<T>("Error related to failure")

  // There was an exception in the request
  DbResult.Exception("Error related to exception", /* Exception instance */)
  DbResult.Exception<T>("Error related to failure", /* Exception instance */)

```

You can check if the query was successful or not by using the "IsSuccess" and "IsFailure" properties. If the query was successful, you will end up with content of the type that you specify between the "<>", so you can access it through the .Value property.

If it was a failure, you can get the error message with the "Error" property.

```sh
    public DbResult<int> ReturnOne() => DbResult.Ok(1);

    public void AnotherMethod()
    {
        DbResult<int> result = ReturnOne();

        if(result.IsSuccess) 
            Console.WriteLine($"Value: {result.Value}"); // Value: 1
        else
            Console.WriteLine($"Something went wrong: {result.Error}");

    }
    
```

You can even check the type of the DbResult when it fails, so you can control exceptions and log them wherever you want:

```sh

    if(result.Type == DbResultType.Exception){
        string error = result.Error;
        Exception ex = result.ExceptionContent;

        /* And now feel free to log it as you see fit */
    }
        
```

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

And if you literally do not need any specify order and you have applied the aforementioned attributes:
```sh
// Without conditions
DbResult<List<TagDTO>> result = _fluentRepository.AutoList<TagDTO>();

// With [QueryFilterBuilder](#QueryFilterBuilder) conditions
DbResult<List<TagDTO>> result = _fluentRepository.AutoList<TagDTO>(x => /* Your conditions here */ );

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

Let's define your typical CRUD repository. The only difference is that I'll add two "Find" methods, to exemplify the difference between the classic way to get data with ADO.NET vs using the AdvancedRepository (which uses ADO.NET under the hood as well).

```sh
public interface IArticleRepository 
{
    // Read - Classic 
    DbResult<List<Article>> FindClassic(string title);

    // Read - "Advanced" 
    DbResult<List<Article>> FindAdvanced(Action<QueryFilterBuilder> filter);

    DbResult Insert(Article article);
    DbResult Update(Article article, int id);
    DbResult Delete(int id);
}
```

Let's also assume the following Repository class:

```sh
public class ArticleRepository : AdvancedRepository, IArticleRepository
{
    public ArticleRepository(BaseDatabaseConfiguration dbConfig) : base(dbConfig){}

    /* All the crud methods from the interface we are going to see below */
}
```

#### Create

For this example, I have considered that maybe you might need the Id of the inserted item for other commands. In this case you can use ExecuteScalar() as shown below:

```sh
    public DbResult Insert(Article article)
    {
        DbResult<object> result = InsertInto("Articulos")
            .FieldValues(
                ("Titulo", article.Title), 
                ("Slug", article.Title.Replace(" ", "-")), 
                ("FechaCreacion", DateTime.Now))             
            .ExecuteScalar();

        /* Add other commands / actions / IO you want them to occur during the same transaction but don't depend on the Id returned from the Insert */

        if (result.IsSuccess) SaveChanges();

        /* Add other commands that may use the object returned from the insert */

        return result;
    }
```

#### Update
You can use the [QueryFilterBuilder](#QueryFilterBuilder) to specify a condition.

```sh
    public DbResult Update(Article article, int id)
        => UpdateFrom("Articulos")
            .FieldValues(
                ("Titulo", article.Title),
                ("Slug", article.Title.Replace(" ", "-")))
            .Where(x => x.ColumnName("Id").EqualTo(id.ToString()))
            .Execute();
```

#### Delete

Just as the Update one, you can use the [QueryFilterBuilder](#QueryFilterBuilder) to specify a condition.

```sh
    public DbResult Delete(int id)
        => DeleteFrom("Articulos")
        .Where(x => x.ColumnName("Id").EqualTo(id.ToString()))
        .Execute();
```

#### Classic vs Advanced Read

This is a typical Find method (that also uses the BaseRepository commodities):

```sh
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
                    CreatedOn = rdr.TypeOrNull<DateTime>("FechaCreacion")
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
```

And this would be the same but "Advanced":

```sh
// "Advanced" way of setting up a find/get data with filters
public DbResult<List<Article>> FindMultipleAdvanced(Action<QueryFilterBuilder> filter)
    => Select<Article>("Id", "Titulo", "Slug", "FechaCreacion")
            .From("Articulos")
            .Where(filter)
            .GetList(x =>
            {
                x.Add("Id", "Id");
                x.Add("Title", "Titulo");
                x.Add("Slug", "Slug");
                x.Add("CreatedOn", "FechaCreacion");
            });
```

This is obviously subjective, but you have more semantic way of doing the same things with methods that resemble SQL.

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