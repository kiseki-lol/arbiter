namespace Kiseki.Arbiter.Models;

public class Health
{
    [JsonPropertyName("status")]
    public Healthiness Status { get; set; } = Healthiness.Dead;
}