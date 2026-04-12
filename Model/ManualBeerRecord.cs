using SQLite;

namespace SachaApp.Model;

[Table("ManualBeers")]
public class ManualBeerRecord
{
    [PrimaryKey]
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Image { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Price { get; set; } = string.Empty;
}

