using Bookings.Domain;

namespace Bookings.Api.Bookings
{
    public record BookingResult(Guid Id, string Description, string Type, DateRange Period, string Email);
}
