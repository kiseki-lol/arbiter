namespace Kiseki.Arbiter.Models;

public class Health
{
    [JsonPropertyName("status")]
    public HealthCheckResponse Status { get; set; } = HealthCheckResponse.Failure;
}