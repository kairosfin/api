using HealthChecks.UI.Client;
using Kairos.Account;
using Kairos.Gateway;
using Kairos.Shared;
using Kairos.Shared.Contracts.Account;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
{
    builder.Configuration.AddEnvironmentVariables();

    builder.Services
        .AddGateway(builder.Configuration)
        .AddAccount()
        .AddShared();
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
        .UseHttpsRedirection()
        .UseStaticFiles()
        .UseHealthChecks("/_health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

    app
        .MapPost(
            "/account/open",
            async (IMediator mediator, [FromBody] OpenAccount command) =>
            {
                return await mediator.Send(command);
            })
        .WithName("OpenAccount")
        .WithTags("Account") 
        .WithOpenApi();

    await app.RunAsync();
}