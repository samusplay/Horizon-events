using Amazon.S3;
using Amazon.S3.Model;
using Horizon.Events.Common.CQRS;
using Horizon.Events.Security;
using Microsoft.Extensions.Options;

namespace Horizon.Events.Events.UploadUrl;

// Command: pide una URL firmada para subir la portada de un evento
public record GenerateUploadUrlCommand(string ContentType, Guid CreatorId) : ICommand<GenerateUploadUrlResult>;

// Resultado: la URL para subir (PUT) y la URL pública final
public record GenerateUploadUrlResult(string UploadUrl, string PublicUrl, int ExpiresInSeconds);

internal class GenerateUploadUrlHandler(IAmazonS3 s3Client, IOptions<AwsOptions> awsOptions)
    : ICommandHandler<GenerateUploadUrlCommand, GenerateUploadUrlResult>
{
    private const int ExpirationMinutes = 5;

    public Task<GenerateUploadUrlResult> Handle(GenerateUploadUrlCommand command, CancellationToken cancellationToken)
    {
        var options = awsOptions.Value;

        var extension = command.ContentType switch
        {
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "image/webp" => "webp",
            _ => "jpg"
        };
        // Cada creador tiene su propia carpeta dentro de covers/
        var key = $"covers/{command.CreatorId}/{Guid.NewGuid()}.{extension}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = options.BucketName,
            Key = key,
            Verb = HttpVerb.PUT,
            ContentType = command.ContentType,
            Expires = DateTime.UtcNow.AddMinutes(ExpirationMinutes)
        };

        var uploadUrl = s3Client.GetPreSignedURL(request);
        var publicUrl = $"https://{options.BucketName}.s3.{options.Region}.amazonaws.com/{key}";

        var result = new GenerateUploadUrlResult(uploadUrl, publicUrl, ExpirationMinutes * 60);
        return Task.FromResult(result);
    }
}