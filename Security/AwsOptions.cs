namespace Horizon.Events.Security;

public class AwsOptions
{
    public const string SectionName = "Aws";

    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
}