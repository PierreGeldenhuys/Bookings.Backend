using Bookings.Domain;

namespace Bookings.Tests
{
    public class DateRangeTests
    {
        [Fact]
        public void Should_Construct()
        {
            // Arrange 
            var start = DateOnly.FromDateTime(DateTime.Now);
            var end = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            // Act
            var range = new DateRange(start, end);
            // Assert
            Assert.Equal(start, range.Start);
            Assert.Equal(end, range.End);
        }

        [Fact]
        public void Should_Not_Construct()
        {
            // Act and Assert
            Assert.Throws<ArgumentException>(() => TestObjects.InvalidDateRange);
        }
    }
}
