using SQLite;

namespace SachaApp.Model;

[Table("BeerTags")]
public class BeerTagRecord
{
    [PrimaryKey]
    public int BeerId { get; set; }

    public int TagValue { get; set; }
}

