namespace Bookings.Tests
{
    public class BookingTests
    {
        [Fact]
        public void Should_Construct()
        {
            var booking = TestObjects.ValidBooking;
            Assert.Equal("Suzuki Jimny 2021", booking.Description);
            Assert.Equal("Vehicle", booking.Type);
            Assert.Equal(TestObjects.ValidDateRange, booking.Period);
            Assert.Equal("p@g.com", booking.Email);
        }

        [Fact]
        public void Should_Not_Construct()
        {
            Assert.Throws<ArgumentException>(() => TestObjects.InvalidBookingDescription);
            Assert.Throws<ArgumentException>(() => TestObjects.InvalidBookingType);
            Assert.Throws<ArgumentNullException>(() => TestObjects.InvalidBookingPeriod);
            Assert.Throws<ArgumentException>(() => TestObjects.InvalidBookingEmail);
        }
    }
}
