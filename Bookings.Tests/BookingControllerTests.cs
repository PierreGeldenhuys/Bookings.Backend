using Bookings.Api.Bookings;
using System.Net;
using System.Net.Http.Json;

namespace Bookings.Tests;

public class CreateBookingTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public CreateBookingTests(TestWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Should_Create()
    {
        // Act        
        var response = await _client.PostAsJsonAsync("/bookings", TestObjects.ValidBookingRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BookingResult>();
        Assert.NotNull(result);
        Assert.True(result.Id != Guid.Empty);
        Assert.Equal(TestObjects.ValidBookingRequest.Description, result.Description);
        Assert.Equal(TestObjects.ValidBookingRequest.Email, result.Email);
        Assert.Equal(TestObjects.ValidBookingRequest.Start, result.Period.Start);
        Assert.Equal(TestObjects.ValidBookingRequest.End, result.Period.End);
        Assert.Equal(TestObjects.ValidBookingRequest.Type, result.Type);
    }

    [Fact]
    public async Task Should_Update()
    {
        //Arrange
        var created = await _client.PostAsJsonAsync("/bookings", TestObjects.ValidBookingRequest);
        var createResult = await created.Content.ReadFromJsonAsync<BookingResult>();
        Assert.NotNull(createResult);

        // Act        
        var response = await _client.PutAsJsonAsync($"/bookings/{createResult.Id}", TestObjects.ValidBookingRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BookingResult>();
        Assert.NotNull(result);
        Assert.Equal(createResult.Id, result.Id);
        Assert.Equal(TestObjects.ValidBookingRequest.Description, result.Description);
        Assert.Equal(TestObjects.ValidBookingRequest.Email, result.Email);
        Assert.Equal(TestObjects.ValidBookingRequest.Start, result.Period.Start);
        Assert.Equal(TestObjects.ValidBookingRequest.End, result.Period.End);
        Assert.Equal(TestObjects.ValidBookingRequest.Type, result.Type);
    }

    [Fact]
    public async Task Should_Delete()
    {
        //Arrange
        var created = await _client.PostAsJsonAsync("/bookings", TestObjects.ValidBookingRequest);
        var createResult = await created.Content.ReadFromJsonAsync<BookingResult>();
        Assert.NotNull(createResult);

        // Act        
        var response = await _client.DeleteAsync($"/bookings/{createResult.Id}");

        // Assert        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(await response.Content.ReadAsStringAsync());
    }
}