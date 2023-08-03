namespace Kiseki.Arbiter.Models;

using System.Text.Json.Serialization;

public class AppSettings
{
    [JsonPropertyName("access_key")]
    public string AccessKey { get; set; } = "";

    [JsonPropertyName("public_key_path")]
    public string PublicKeyPath { get; set; } = "";

    [JsonPropertyName("license_path")]
    public string LicensePath { get; set; } = "";

    [JsonPropertyName("service_port")]
    public int ServicePort { get; set; } = 0;

    [JsonPropertyName("base_job_port")]
    public int BasePort { get; set; } = 0;
}