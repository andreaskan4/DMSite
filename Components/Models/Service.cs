namespace DMsite.Models;

public class Service
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Duration { get; set; } = "";
    public string? Description { get; set; }
}
