namespace FluentRepositories.Attributes;

public class DatabaseColumn : Attribute
{
    public string Name { get; set; }
    public DatabaseColumn(string name)
    {
        Name = name;
    }
}

