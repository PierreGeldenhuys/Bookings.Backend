using Bookings.Api.Bookings;
using Bookings.Domain;

namespace Bookings.Tests
{
    public class BookingsInMemRepositoryTests
    {
        private readonly IBookingsRepository _repo;
        public static Booking _randomBooking => new($"{Guid.NewGuid()}", $"{Guid.NewGuid()}", TestObjects.ValidDateRange, $"{Guid.NewGuid()}");

        public BookingsInMemRepositoryTests()
        {
            _repo = new BookingsInMemRepository();
        }

        [Fact]
        public void Should_Create_Idempotent()
        {
            _repo.Create(TestObjects.ValidBooking);
            var booking = _repo.Create(TestObjects.ValidBooking);
            Assert.True(Guid.Empty != booking.Key);
            Assert.NotNull(booking.Value);
            Assert.Equal(TestObjects.ValidBooking, booking.Value);

            var all = _repo.GetAll();
            Assert.NotNull(all);
            Assert.Single(all);
            Assert.True(_repo.KeyExists(BookingKey.From(booking.Value)));
        }

        [Fact]
        public void Should_Update()
        {
            var created = _repo.Create(TestObjects.ValidBooking);
            var update = new Booking("1", "2", new(DateOnly.MinValue, DateOnly.MaxValue), "y@z.com");
            var updated = _repo.Update(created.Key, update);

            Assert.Equal(created.Key, updated.Key);
            Assert.NotNull(updated.Value);
            Assert.Equal(update, updated.Value);
            Assert.False(_repo.KeyExists(BookingKey.From(created.Value!)));
            Assert.True(_repo.KeyExists(BookingKey.From(updated.Value)));
        }

        [Fact]
        public void Should_Delete()
        {
            var created = _repo.Create(TestObjects.ValidBooking);
            var deleted = _repo.Delete(created.Key);

            Assert.Equal(created.Key, deleted.Key);
            Assert.NotNull(deleted.Value);
            Assert.Equal(created.Value, deleted.Value);
            Assert.False(_repo.KeyExists(BookingKey.From(created.Value!)));
        }

        [Fact]
        public void Should_Get()
        {
            var created = _repo.Create(TestObjects.ValidBooking);
            Assert.NotEqual(Guid.Empty, created.Key);
            Assert.NotNull(created.Value);
            var got = _repo.Get(created.Key);

            Assert.Equal(created.Key, got.Key);
            Assert.NotNull(got.Value);
            Assert.Equal(created.Value, got.Value);
        }

        [Fact]
        public void Should_GetAll()
        {
            _repo.Create(_randomBooking);
            _repo.Create(_randomBooking);
            _repo.Create(_randomBooking);
            _repo.Create(_randomBooking);

            var all = _repo.GetAll();

            Assert.NotEmpty(all);
            Assert.Equal(4, all.Count);
            foreach (var one in all)
            {
                Assert.NotEqual(Guid.Empty, one.Key);
                Assert.NotNull(one.Value);
            }
        }

        [Fact]
        public async Task Should_GetAndCreate_Concurrently()
        {
            // Arrange
            var seeded = _repo.Create(TestObjects.ValidBooking);
            const int ops = 1_000;

            // Act: kick off reads and writes simultaneously
            var tasks = Enumerable.Range(1, ops)
                .Select(i => Task.Run(() =>
                {
                    if (i % 2 == 0)
                    {
                        // even -> write
                        var booking = _repo.Create(_randomBooking);
                        Assert.NotEqual(Guid.Empty, booking.Key);
                        Assert.NotNull(booking.Value);
                        Assert.True(_repo.KeyExists(BookingKey.From(booking.Value)));
                    }
                    else
                    {
                        // odd -> read
                        var got = _repo.Get(seeded.Key);
                        Assert.Equal(seeded.Key, got.Key);
                        Assert.NotNull(got.Value);
                    }
                }))
                .ToArray();

            await Task.WhenAll(tasks);

            var got = _repo.Get(seeded.Key);
            Assert.Equal(seeded.Key, got.Key);
            Assert.NotNull(got.Value);

            var all = _repo.GetAll();
            Assert.NotEmpty(all);
            Assert.Equal(501, all.Count);
            foreach (var one in all)
            {
                Assert.True(Guid.Empty != one.Key);
                Assert.NotNull(one.Value);
            }
        }

        [Fact]
        public async Task Should_GetAndUpdate_Concurrently()
        {
            // Arrange
            var seeded = _repo.Create(TestObjects.ValidBooking);
            const int ops = 1_000;

            // Act: kick off reads and writes simultaneously
            var tasks = Enumerable.Range(1, ops)
                .Select(i => Task.Run(() =>
                {
                    if (i % 2 == 0)
                    {
                        // even -> write
                        var update = new Booking(
                            i.ToString(),
                            i.ToString(),
                            new(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-i)), DateOnly.FromDateTime(DateTime.UtcNow.AddDays(i))),
                            i.ToString()
                            );
                        var updated = _repo.Update(seeded.Key, update);
                        Assert.Equal(seeded.Key, updated.Key);
                        Assert.NotNull(updated.Value);
                        Assert.Equal(update, updated.Value);
                    }
                    else
                    {
                        // odd -> read
                        var got = _repo.Get(seeded.Key);
                        Assert.Equal(seeded.Key, got.Key);
                        Assert.NotNull(got.Value);
                    }
                }))
                .ToArray();

            await Task.WhenAll(tasks);

            var got = _repo.Get(seeded.Key);
            Assert.Equal(seeded.Key, got.Key);
            Assert.NotNull(got.Value);
        }

        [Fact]
        public async Task Should_GetAndDelete_Concurrently()
        {
            // Arrange
            var seeded = _repo.Create(TestObjects.ValidBooking);
            const int ops = 1_000;

            // Act: kick off reads and writes simultaneously
            var tasks = Enumerable.Range(1, ops)
                .Select(i => Task.Run(() =>
                {
                    if (i % 2 == 0)
                    {
                        // even -> write and delete
                        var created = _repo.Create(_randomBooking);
                        Assert.NotEqual(Guid.Empty, created.Key);
                        Assert.NotNull(created.Value);
                        var deleted = _repo.Delete(created.Key);
                        Assert.True(Guid.Empty != deleted.Key);
                        Assert.NotNull(deleted.Value);
                        Assert.Equal(created.Value, deleted.Value);
                    }
                    else
                    {
                        // odd -> read
                        var got = _repo.Get(seeded.Key);
                        Assert.Equal(seeded.Key, got.Key);
                        Assert.NotNull(got.Value);
                    }
                }))
                .ToArray();

            await Task.WhenAll(tasks);

            var got = _repo.Get(seeded.Key);
            Assert.Equal(seeded.Key, got.Key);
            Assert.NotNull(got.Value);
        }
    }
}
