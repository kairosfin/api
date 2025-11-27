using Kairos.Shared.Contracts;
using Kairos.Shared.Enums;
using Mapster;

namespace Kairos.Gateway.Filters;

internal sealed record Response<T>(T? Data, string[] Messages);

internal sealed class ResponseFormatter(ILogger<ResponseFormatter> logger) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context, 
        EndpointFilterDelegate next)
    {
        try
        {
            var response = await next(context);

            if (response is not Output output)
            {
                return response;
            }

            var statusCode = output.Status switch
            {
                OutputStatus.Ok => StatusCodes.Status200OK,
                OutputStatus.Created => StatusCodes.Status201Created,
                OutputStatus.Empty => StatusCodes.Status204NoContent,
                OutputStatus.InvalidInput => StatusCodes.Status400BadRequest,
                OutputStatus.UnexistentId => StatusCodes.Status404NotFound,
                OutputStatus.BusinessLogicViolation => StatusCodes.Status422UnprocessableEntity,
                _ => StatusCodes.Status500InternalServerError,
            };

            if (statusCode == StatusCodes.Status204NoContent)
            {
                return Results.NoContent();
            }

            var res = output.Adapt<Response<object?>>();

            return Results.Json(res, statusCode: statusCode);
        } 
        catch (Exception ex)
        {
            var message = ex is OperationCanceledException 
                ? "Oops! The process has taken too long"
                : "Oops! An unexpected error occurred.";

            logger.LogError(ex, "{Error}", message);

            return Results.Json(
                data: new Response<object?>(null, [message]),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
