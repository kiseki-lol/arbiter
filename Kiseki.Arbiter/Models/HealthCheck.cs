namespace Kiseki.Arbiter.Models;

public class HealthCheck
{
    [JsonPropertyName("status")]
    public HealthCheckStatus Status { get; set; }
}