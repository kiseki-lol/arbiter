namespace Kiseki.Arbiter.Models;

using System.Text.Json.Serialization;

public class Health
{
    [JsonPropertyName("status")]
    public int Status { get; set; } = -1;
}