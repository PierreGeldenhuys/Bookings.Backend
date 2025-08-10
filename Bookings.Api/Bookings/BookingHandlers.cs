using Bookings.Domain;
using Bookings.Infrastructure.RequestHandling;

namespace Bookings.Api.Bookings
{
    public static class BookingHandlers
    {
        public static Task<IResult> Create(BookingRequest request, IBookingsRepository repository) =>
        RequestHandler.Execute(() =>
        {
            var booking = repository.Create(new Booking(
                request.Description,
                request.Type,
                new DateRange(request.Start, request.End),
                request.Email));

            if (booking.Value is null)
                return null!;

            return Results.Created($"/bookings/", new BookingResult(
                booking.Key, booking.Value.Description, booking.Value.Type, booking.Value.Period, booking.Value.Email));
        }, "Create", request.ToString());

        public static Task<IResult> Update(Guid id, BookingRequest request, IBookingsRepository repository) =>
            RequestHandler.Execute(() =>
            {
                if (id == Guid.Empty)
                    throw new ArgumentException("Invalid id");

                var updated = repository.Update(id, new Booking(
                    request.Description,
                    request.Type,
                    new DateRange(request.Start, request.End),
                    request.Email));

                if (updated.Value is null)
                    return Results.NoContent();

                return Results.Ok(new BookingResult(
                    updated.Key, updated.Value.Description, updated.Value.Type, updated.Value.Period, updated.Value.Email));
            }, "Update", id.ToString());

        public static Task<IResult> Delete(Guid id, IBookingsRepository repository) =>
            RequestHandler.Execute(() =>
            {
                if (id == Guid.Empty)
                    throw new ArgumentException($"Invalid id {id}");

                var deleted = repository.Delete(id);

                return deleted.Value is null ? Results.NoContent() : Results.Ok(new BookingResult(
                    deleted.Key, deleted.Value.Description, deleted.Value.Type, deleted.Value.Period, deleted.Value.Email));
            }, "Delete", id.ToString());

        public static Task<IResult> Get(IBookingsRepository repository) =>
            RequestHandler.Execute(() =>
            {
                return Results.Ok(repository.GetAll());
            }, "Get", "All");
    }
}
