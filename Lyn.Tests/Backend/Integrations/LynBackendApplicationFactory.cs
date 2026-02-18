using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Resend;

namespace Lyn.Tests.Backend.Integrations;

public class LynBackendApplicationFactory :  WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(config =>
        {
            // IKKE bruk config.Sources.Clear() her
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:Key"] = "test_key_minimum_32_characters_long_for_testing",
                ["JwtSettings:Issuer"] = "TestIssuer",
                ["JwtSettings:Audience"] = "TestAudience",
                ["JwtSettings:TokenValidityMinutes"] = "5",
                ["Resend:ApiKey"] = "test-api-key",
                ["AWS:BucketName"] = "test-bucket",
                ["AWS:Region"] = "eu-north-1",
                ["ReleaseApiKey"] = "test-release-key"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // ===== Mock S3 =====
            var mockS3 = new Mock<IAmazonS3>();
            mockS3.Setup(s => s.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutObjectResponse());
            mockS3.Setup(s => s.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectResponse
                {
                    ResponseStream = new MemoryStream([1, 2, 3]),
                    ContentLength = 3
                });
            mockS3.Setup(s => s.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteObjectResponse());
            services.AddSingleton(mockS3.Object);

            // ===== Mock Resend =====
            var mockResend = new Mock<IResend>();
            mockResend
                .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null));
            services.AddSingleton(mockResend.Object);
        });
    }
}