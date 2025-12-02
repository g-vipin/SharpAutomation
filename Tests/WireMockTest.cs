using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
namespace SharpAutomation.Tests;

public class WireMockTest
{
    [Test]
    public async Task Should_Return_User()
    {
        var server = WireMockServer.Start();

        server.Given(
            Request.Create().WithPath("/users/123").UsingGet()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithBody("{\"id\": 123, \"name\": \"Vipin\"}")
        );

        var http = new HttpClient();
        var response = await http.GetStringAsync($"{server.Urls[0]}/users/123");

        Assert.That(response, Does.Contain("Vipin"));

        server.Stop();
        server.Dispose();
    }
}
