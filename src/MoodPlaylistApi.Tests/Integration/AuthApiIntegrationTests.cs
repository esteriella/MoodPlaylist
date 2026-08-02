using MoodPlaylistApi.Dtos.Auth;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MoodPlaylistApi.Tests.Integration;

[Collection("PostgreSQL")]
[Trait("Category", "Integration")]
public sealed class AuthApiIntegrationTests(PostgreSqlFixture database)
{
    [Fact(DisplayName = "Register and login endpoints persist authentication through PostgreSQL")]
    public async Task AuthenticationEndpoints_ValidRequests_ReturnExpectedApiResponses()
    {
        await database.ResetDatabaseAsync();
        await using var factory = new MoodPlaylistApiFactory(database);
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        var registration = new RegisterDto
        {
            Name = "Ada",
            Email = "ada@example.com",
            Password = "Password1!"
        };

        var registerResponse = await client.PostAsJsonAsync("/auth/register", registration);
        using var registerBody = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var loginResponse = await client.PostAsJsonAsync("/auth/login", new LoginDto
        {
            Email = registration.Email,
            Password = registration.Password
        });
        using var loginBody = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.Equal("Ada", registerBody.RootElement.GetProperty("data").GetProperty("name").GetString());
        Assert.False(string.IsNullOrWhiteSpace(
            registerBody.RootElement.GetProperty("data").GetProperty("token").GetString()));
        Assert.Equal(HttpStatusCode.Created, loginResponse.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(
            loginBody.RootElement.GetProperty("data").GetProperty("refreshToken").GetString()));
    }

    [Fact(DisplayName = "Register endpoint applies API model validation before accessing PostgreSQL")]
    public async Task Register_InvalidRequest_ReturnsValidationErrorContract()
    {
        await database.ResetDatabaseAsync();
        await using var factory = new MoodPlaylistApiFactory(database);
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var response = await client.PostAsJsonAsync("/auth/register", new
        {
            name = "A",
            email = "not-an-email",
            password = "weak"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Unable to process your request", body);
    }
}
