namespace SachaApp.Model;

public class Beer
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Price { get; set; }
    public string? Image { get; set; }
    public Rating? Rating { get; set; }
}