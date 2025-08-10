namespace Bookings.Api.Bookings
{
    public record BookingRequest(string Description, string Type, DateOnly Start, DateOnly End, string Email);
}