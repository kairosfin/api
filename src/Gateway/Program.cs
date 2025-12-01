using Carter;
using HealthChecks.UI.Client;
using Kairos.Account;
using Kairos.Gateway;
using Kairos.Gateway.Filters;
using Kairos.MarketData;
using Kairos.Shared;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    builder.Configuration.AddEnvironmentVariables();

    if (builder.Environment.IsDevelopment() 
        || builder.Environment.IsEnvironment("Local"))
    {
        builder.Configuration.AddUserSecrets<Program>();
    }

    builder.Services
        .AddShared(
            builder.Configuration,
            builder.Host)
        .AddMarketData(builder.Configuration)
        .AddGateway(builder.Configuration)
        .AddAccount(builder.Configuration);
}

WebApplication app = builder.Build();
{
    app
        .UseResponseCompression()
        .UseSwagger(o => o.RouteTemplate = "api/{documentName}/swagger.{json|yaml}")
        .UseSwaggerUI(o =>
        {
            o.SwaggerEndpoint("/api/v1/swagger.json", "Kairos v1");
            o.InjectStylesheet("/swagger.css");
            o.RoutePrefix = "docs";
        });

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app
        .UseRouting()
        .UseStaticFiles()
        .UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse, 
        })
        .UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });

    app
        .MapGroup(string.Empty)
        .AddEndpointFilter<ResponseFormatter>()
        .MapCarter()
        .MapHealthChecksUI(o =>
        {
            o.UIPath = "/health";
            o.PageTitle = "Health | Kairos";
        });

    await app.RunAsync();
}