namespace Kiseki.Arbiter.Models;

using System.Text.Json.Serialization;

public class Identification
{
    [JsonPropertyName("id")]
    public Guid GameServerId { get; set; } = Guid.Empty;
}