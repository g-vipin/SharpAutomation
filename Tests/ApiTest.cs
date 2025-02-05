using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Allure.NUnit;
using FluentAssertions;

namespace SharpAutomation.Tests;
[AllureNUnit]
public class ApiTest
{
    [Test]
    public async Task TestApiAuth()
    {
        // Arrange
        var httpClientFactory = GlobalSetUp.GetService<IHttpClientFactory>();
        var client = httpClientFactory.CreateClient("ApiClient");
        var baseUrl = new Uri(client.BaseAddress!, "auth");
        var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);
        var json = "{ \"username\" : \"admin\", \"password\" : \"password123\"}";                             
        request.Content = new StringContent(json,Encoding.UTF8, "application/json");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        // Act
        var response = await client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync(); 
        var responseData =  JsonSerializer.Deserialize<AuthResponse>(content);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        Assert.That(responseData, Is.Not.Null);
        responseData.Token.Should().NotBeNullOrEmpty();
    }

}

public class AuthResponse
{
    [JsonPropertyName("token")]
    public string? Token { get; set;}
}