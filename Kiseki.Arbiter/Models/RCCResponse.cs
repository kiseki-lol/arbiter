namespace Kiseki.Arbiter.Models;

public class RCCResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = "";
    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}