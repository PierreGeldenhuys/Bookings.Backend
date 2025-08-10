using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bookings.Infrastructure.RequestHandling
{
    public abstract class RequestHandler
    {
        public static async Task<IResult> Execute(Func<IResult> handlerLogic, string operationName, string resourceIdentifier)
        {
            try
            {
                var result = handlerLogic();
                return result ?? Results.Problem(new ProblemDetails
                {
                    Title = $"{operationName} failed",
                    Detail = $"Handler logic returned null for {operationName} of {resourceIdentifier}",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
            catch (ArgumentException ex)
            {
                return Results.Problem(new ProblemDetails
                {
                    Title = "Invalid request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(new ProblemDetails
                {
                    Title = $"An error occurred during {operationName} of {resourceIdentifier}",
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
