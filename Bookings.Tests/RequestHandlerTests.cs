using Bookings.Infrastructure.RequestHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Bookings.Tests
{
    public class RequestHandlerTests
    {
        public RequestHandlerTests() { }

        [Fact]
        public async Task Should_Return_Result()
        {
            // Arrange
            var expected = Results.Ok("Success");
            // Act
            var result = await RequestHandler.Execute(() => expected, "Create", "x");

            // Assert
            var okResult = Assert.IsType<Ok<string>>(result);
            Assert.Equal("Success", okResult.Value);
        }

        [Fact]
        public async Task Should_Return_Problem_When_Result_Null()
        {
            // Act
            var result = await RequestHandler.Execute(() => null!, "Create", "x");

            // Assert
            var problem = Assert.IsType<ProblemHttpResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.ProblemDetails);
            Assert.Equal("Create failed", details.Title);
            Assert.Contains("x", details.Detail);
            Assert.Equal(StatusCodes.Status500InternalServerError, details.Status);
        }

        [Fact]
        public async Task Should_Return_BadRequest_On_ArgumentException()
        {
            // Act
            var result = await RequestHandler.Execute(() => throw new ArgumentException("Invalid arg"), "Update", "x");

            // Assert
            var problem = Assert.IsType<ProblemHttpResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.ProblemDetails);
            Assert.Equal("Invalid request", details.Title);
            Assert.Equal("Invalid arg", details.Detail);
            Assert.Equal(StatusCodes.Status400BadRequest, details.Status);
        }

        [Fact]
        public async Task Should_Return_Server_Error_On_Exception()
        {
            // Act
            var result = await RequestHandler.Execute(() => throw new Exception("Boom"), "Delete", "x");

            // Assert
            var problem = Assert.IsType<ProblemHttpResult>(result);
            var details = Assert.IsType<ProblemDetails>(problem.ProblemDetails);
            Assert.Equal("An error occurred during Delete of x", details.Title);
            Assert.Equal("Boom", details.Detail);
            Assert.Equal(StatusCodes.Status500InternalServerError, details.Status);
        }
    }
}
