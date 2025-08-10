namespace Bookings.Domain
{
    /* 
    Learnings: record structs are great for O(1) lookups since they are not allocated on the heap    
    When you use a ConcurrentDictionary<BookingKey, Guid>:
    record struct -> Each key is stored inline in the dictionary’s key slot. No extra heap allocation per key.
    record class -> Dictionary stores a reference to a heap-allocated object for each key. Many keys = more GC work.
    For O(1) lookups in high-load systems, avoiding unnecessary heap allocations can have a measurable effect.
    */

    public readonly record struct BookingKey(
    string Description,
    string Type,
    DateOnly Start,
    DateOnly End,
    string Email)
    {
        public static BookingKey From(Booking b) => new(
            Description: b.Description,
            Type: b.Type,
            Start: b.Period.Start,
            End: b.Period.End,
            Email: b.Email
        );
    }
}
