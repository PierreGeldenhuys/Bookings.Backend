namespace Bookings.Domain
{
    public record DateRange
    {
        public DateRange(DateOnly start, DateOnly end)
        {
            if (end < start)
                throw new ArgumentException("End must be after start");

            End = end;
            Start = start;
        }

        public DateOnly Start { get; }
        public DateOnly End { get; }
    }
}