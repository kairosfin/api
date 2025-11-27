namespace Kairos.Shared.Infra.HttpClient;

public sealed class QueryParamHttpHandler(string paramName, string paramValue) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(paramValue) && request.RequestUri is not null)
        {
            var uri = request.RequestUri;
            
            if (!uri.Query.Contains($"{paramName}=", StringComparison.OrdinalIgnoreCase))
            {
                var separator = string.IsNullOrEmpty(uri.Query) ? "?" : "&";
                var queryParam = $"{paramName}={Uri.EscapeDataString(paramValue)}";
                request.RequestUri = new Uri(uri + separator + queryParam);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}