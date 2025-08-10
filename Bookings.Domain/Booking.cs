namespace Bookings.Domain
{
    public record Booking
    {
        public Booking(string description, string type, DateRange period, string email)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(description, nameof(description));
            ArgumentException.ThrowIfNullOrWhiteSpace(type, nameof(type));
            ArgumentNullException.ThrowIfNull(period, nameof(period));
            ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

            Description = description;
            Type = type;
            Period = period;
            Email = email;
        }

        public string Description { get; }
        public string Type { get; }
        public DateRange Period { get; }
        public string Email { get; }
    }
}