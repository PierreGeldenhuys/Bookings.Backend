namespace Bookings.Domain
{
    public interface IBookingsRepository
    {
        public Dictionary<Guid, Booking> GetAll();
        public KeyValuePair<Guid, Booking?> Get(Guid id);
        public KeyValuePair<Guid, Booking?> Create(Booking booking);
        public KeyValuePair<Guid, Booking?> Update(Guid Id, Booking booking);
        public KeyValuePair<Guid, Booking?> Delete(Guid Id);
    }
}
