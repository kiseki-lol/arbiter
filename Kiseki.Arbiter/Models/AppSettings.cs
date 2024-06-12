namespace Kiseki.Arbiter.Models;

public class AppSettings
{
    [JsonPropertyName("access_key")]
    public string AccessKey { get; set; } = "";

    [JsonPropertyName("public_key_path")]
    public string PublicKeyPath { get; set; } = "";

    [JsonPropertyName("license_path")]
    public string? LicensePath { get; set; }

    [JsonPropertyName("service_port")]
    public int ServicePort { get; set; } = 64989;

    [JsonPropertyName("base_job_port")]
    public int BaseJobPort { get; set; } = 53640;

    [JsonPropertyName("base_soap_port")]
    public int BaseSoapPort { get; set; } = 65000;   
}