namespace Kiseki.Arbiter.Models;

using System.Text.Json.Serialization;

public class Identification
{
    [JsonPropertyName("uuid")]
    public Guid GameServerUuid { get; set; } = Guid.Empty;
}