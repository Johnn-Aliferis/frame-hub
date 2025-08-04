using System.Net;
using System.Text;
using System.Text.Json;
using FrameHub.Modules.Media.API.DTO;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FrameHub.Tests.Modules.Media.API.Controllers;

public class MediaControllerTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();
    
    [Fact]
    public async Task GeneratePresignedUrl_WithoutJwt()
    {
        var body = new PresignedUrlRequestDto
        {
            FileName = "test.jpg"
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/media/presigned-url", content);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ConfirmUpload_WithoutJwt()
    {
        var body = new PhotoRequestDto
        {
            StorageKey = "test-storage-key",
            FileName = "test.jpg",
            IsProfilePicture = true,
            Tags = ""
        };
        
        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/media", content);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task DeleteMedia_WithoutJwt()
    {
        const long photoId = 1L;
        var response = await _client.DeleteAsync($"/api/media/{photoId}");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}