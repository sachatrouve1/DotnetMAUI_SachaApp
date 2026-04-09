using System.Text.Json.Serialization;

namespace SachaApp.Model;

public class Rating
{
    public double Average { get; set; }
    [JsonConverter(typeof(FlexibleStringJsonConverter))]
    public string? Reviews { get; set; }
}