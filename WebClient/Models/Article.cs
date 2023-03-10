namespace WebClient.Models;

public class Article
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime? PublicationDate { get; set; }
}
