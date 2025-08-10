using Bookings.Domain;
using System.Collections.Concurrent;

namespace Bookings.Api.Bookings
{
    public class BookingsInMemRepository : IBookingsRepository
    {
        private readonly ConcurrentDictionary<Guid, Booking> _store = new();
        private readonly ConcurrentDictionary<BookingKey, Guid> _lookup = new();

        public KeyValuePair<Guid, Booking?> Create(Booking booking)
        {
            var bookingKey = BookingKey.From(booking);

            var id = _lookup.AddOrUpdate(
                bookingKey,
                key => Guid.NewGuid(),
                (key, value) => value);

            var stored = _store.AddOrUpdate(
                id,
                key => booking,
                (key, value) => value);

            return new(id, stored);
        }

        public KeyValuePair<Guid, Booking?> Update(Guid id, Booking booking)
        {
            if (!_store.ContainsKey(id))
                return new(id, null);

            if (_store.TryGetValue(id, out var existing) && Equals(existing, booking))
                return new(id, existing);

            var updated = _store.AddOrUpdate(
                id,
                key => booking,
                (key, value) => booking);

            var newKey = BookingKey.From(updated);
            _lookup.AddOrUpdate(
                newKey,
                key => id,
                (key, value) => id);

            return new(id, updated);
        }

        public Dictionary<Guid, Booking> GetAll()
        {
            return _store.ToDictionary(); // Snapshot
        }

        public KeyValuePair<Guid, Booking?> Get(Guid id)
        {
            _store.TryGetValue(id, out var found);
            return new(id, found);
        }

        public KeyValuePair<Guid, Booking?> Delete(Guid id)
        {
            _store.TryRemove(id, out var deleted);

            if (deleted != null)
                _lookup.TryRemove(BookingKey.From(deleted), out var removedId);

            return new(id, deleted);
        }
    }
}
