namespace Kiseki.Arbiter.Models;

public class Signal
{
    [JsonPropertyName("uuid")]
    public Guid Uuid { get; set; }

    [JsonPropertyName("command")]
    public Command Command { get; set; }

    [JsonPropertyName("data")]
    public Dictionary<string, string>? Data { get; set; }
}