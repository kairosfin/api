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
                OutputStatus.NotFound => StatusCodes.Status404NotFound,
                OutputStatus.PolicyViolation => StatusCodes.Status422UnprocessableEntity,
                OutputStatus.CredentialsRequired => StatusCodes.Status401Unauthorized,
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
            logger.LogError(ex, "{Error}", ex.Message);
            logger.LogError(ex, "{Error}", ex.Message);

            return Results.Json(
                data: new Response<object?>(null, [ex.Message]),
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
