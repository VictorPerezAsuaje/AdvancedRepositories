namespace FluentRepositories.Attributes;

public class DefaultTable : Attribute
{
    public string Name { get; set; }
    public DefaultTable(string name)
    {
        Name = name;
    }
}

