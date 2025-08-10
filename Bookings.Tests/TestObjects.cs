using Bookings.Api.Bookings;
using Bookings.Domain;

namespace Bookings.Tests
{
    public static class TestObjects
    {
        public static DateRange ValidDateRange => new(DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)));
        public static DateRange InvalidDateRange => new(DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));
        public static Booking ValidBooking => new("Suzuki Jimny 2021", "Vehicle", ValidDateRange, "p@g.com");
        public static Booking InvalidBookingDescription => new(string.Empty, "Vehicle", ValidDateRange, "p@g.com");
        public static Booking InvalidBookingType => new("Suzuki Jimny 2021", string.Empty, ValidDateRange, "p@g.com");
        public static Booking InvalidBookingEmail => new("Suzuki Jimny 2021", "Vehicle", ValidDateRange, string.Empty);
        public static Booking InvalidBookingPeriod => new("Suzuki Jimny 2021", "Vehicle", null!, string.Empty);
        public static BookingRequest ValidBookingRequest => new("Suzuki Jimny 2021", "Vehicle", ValidDateRange.Start, ValidDateRange.End, "p@g.com");
    }
}
