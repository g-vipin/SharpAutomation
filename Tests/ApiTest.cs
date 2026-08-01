using System.Text.Json.Serialization;
using Allure.NUnit;
using FluentAssertions;
using SharpAutomation.API;

namespace SharpAutomation.Tests;
[AllureNUnit]
public class ApiTest
{
    [Test]
    public async Task TestApiAuth()
    {
        // Arrange
        var client = GlobalSetUp.GetService<IModularMonolithApiClient>();
        var payload = new AuthRequest("admin", "password123");

        // Act
        var responseData = await client.SendJsonAsync<AuthRequest, AuthResponse>(
            HttpMethod.Post,
            "auth",
            payload,
            TestContext.CurrentContext.CancellationToken);

        // Assert
        Assert.That(responseData, Is.Not.Null);
        responseData.Token.Should().NotBeNullOrEmpty();
    }

}

public sealed record AuthRequest(
    [property: JsonPropertyName("username")] string Username,
    [property: JsonPropertyName("password")] string Password);

public class AuthResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set;}
}
