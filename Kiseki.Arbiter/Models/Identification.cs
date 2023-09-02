namespace Kiseki.Arbiter.Models;

public class Identification
{
    [JsonPropertyName("uuid")]
    public Guid GameServerUuid { get; set; } = Guid.Empty;
}