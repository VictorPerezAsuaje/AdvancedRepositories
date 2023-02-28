using FluentRepositories.Attributes;

namespace WebClient.Models;

[DefaultTable("Etiquetas")]
public class TagDTO
{
    [DatabaseColumn("Id")]
    public int Id { get; set; }

    [DatabaseColumn("Nombre")]
    public string Name { get; set; }
}
