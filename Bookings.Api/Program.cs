using Bookings.Api.Bookings;
using Bookings.Domain;

var builder = WebApplication.CreateBuilder();
builder.Services.AddSingleton<IBookingsRepository, BookingsInMemRepository>();
var app = builder.Build();

var bookings = app.MapGroup("/bookings");
bookings.MapPost("", BookingHandlers.Create);
bookings.MapPut("/{id}", BookingHandlers.Update);
bookings.MapDelete("/{id}", BookingHandlers.Delete);
bookings.MapGet("", BookingHandlers.Get);
app.Run();

public partial class Program() { } // Makes the class testable by exposing it
