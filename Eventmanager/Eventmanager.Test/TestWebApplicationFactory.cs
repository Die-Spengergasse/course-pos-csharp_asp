using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Eventmanager.Test;

public class TestWebApplicationFactory<Tcontext> : WebApplicationFactory<Program> where Tcontext : DbContext
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly TimeProvider _timeProvider;

    public TestWebApplicationFactory(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.First(d => d.ServiceType == typeof(DbContextOptions<Tcontext>));
            services.Remove(descriptor);
            services.AddDbContext<Tcontext>(options =>
            {
                options.UseSqlite("DataSource=api_test.db");
            });
            var timeProvider = services.FirstOrDefault(d => d.ServiceType == typeof(TimeProvider));
            if (timeProvider is not null)
            {
                services.Remove(timeProvider);
                services.AddSingleton<TimeProvider>(opt => _timeProvider);
            }
        });
        builder.UseEnvironment("Testing");
    }
    public void InitializeDatabase(Action<Tcontext> action)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<Tcontext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        action(db);
    }
    public Tout QueryDatabase<Tout>(Func<Tcontext, Tout> query)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<Tcontext>();
        return query(db);
    }
    /// <summary>
    /// Send a GET Request and return the deserialized response.
    /// Useful for strongly typed JSON data with DTOs.
    /// </summary>
    public async Task<(HttpResponseMessage, T?)> GetHttpContent<T>(string requestUrl) where T : class
    {
        using var client = CreateClient();
        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode) return (response, default);
        var dataString = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<T>(dataString, _jsonOptions);
        if (data is null) throw new Exception("Deserialization failed");
        return (response, data);
    }

    /// <summary>
    /// Send a GET Request and return the response as a JsonElement. Useful for dynamic JSON data without DTOs.
    /// </summary>
    /// <param name="requestUrl"></param>
    /// <returns></returns>
    public async Task<(HttpResponseMessage, JsonElement)> GetHttpContent(string requestUrl)
    {
        using var client = CreateClient();
        var response = await client.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode) return (response, new JsonElement());
        var dataString = await response.Content.ReadAsStringAsync();
        var data = JsonDocument.Parse(dataString);
        return (response, data.RootElement);
    }

    /// <summary>
    /// Send a POST Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<(HttpResponseMessage, JsonElement)> PostHttpContent<Tcmd>(string requestUrl, Tcmd payload) where Tcmd : class
    {
        using var client = CreateClient();
        var jsonBody = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(requestUrl, jsonBody);
        var dataString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dataString)) return (response, new JsonElement());
        var data = JsonDocument.Parse(dataString);
        return (response, data.RootElement);
    }

    /// <summary>
    /// Send a PATCH Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<(HttpResponseMessage, JsonElement)> PatchHttpContent<Tcmd>(string requestUrl, Tcmd payload) where Tcmd : class
    {
        using var client = CreateClient();
        var jsonBody = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PatchAsync(requestUrl, jsonBody);
        var dataString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dataString)) return (response, new JsonElement());
        var data = JsonDocument.Parse(dataString);
        return (response, data.RootElement);
    }

    /// <summary>
    /// Send a PUT Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<(HttpResponseMessage, JsonElement)> PutHttpContent<Tcmd>(string requestUrl, Tcmd payload) where Tcmd : class
    {
        using var client = CreateClient();
        var jsonBody = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await client.PutAsync(requestUrl, jsonBody);
        var dataString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dataString)) return (response, new JsonElement());
        var data = JsonDocument.Parse(dataString);
        return (response, data.RootElement);
    }

    /// <summary>
    /// Send a DELETE Request with a strongly typed payload and return the deserialized response.
    /// </summary>
    public async Task<(HttpResponseMessage, JsonElement)> DeleteHttpContent(string requestUrl)
    {
        using var client = CreateClient();
        var response = await client.DeleteAsync(requestUrl);
        var dataString = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(dataString)) return (response, new JsonElement());
        var data = JsonDocument.Parse(dataString);
        return (response, data.RootElement);
    }
    /// <summary>
    /// Liest einen rfc problem detail aus. Dieser kann aus 2 Arten bestehen:
    /// { "detail": "Error message" } bei Servicefehlern
    /// { "errors": "MyProperty": ["Validation message" ] } bei Validierungsfehlern
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public bool ContentContainsErrorMessage(JsonElement content, string message) =>
        content.TryGetProperty("errors", out var errors)
        ? errors.EnumerateObject()
            .Any(obj => obj.Value.EnumerateArray()
                .Any(item => item.GetString()?.Contains(message, StringComparison.OrdinalIgnoreCase) == true))
        : content.TryGetProperty("detail", out var detail)
            ? detail.GetString()?.Contains(message, StringComparison.OrdinalIgnoreCase) == true
            : false;
    public HttpClient Client => CreateClient();
}
