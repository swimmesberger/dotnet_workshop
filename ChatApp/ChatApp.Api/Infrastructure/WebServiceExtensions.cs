using CAP;
using CAP.Infrastructure;

namespace ChatApp.Api.Infrastructure;

public static class WebServiceExtensions {
    public static IHostApplicationBuilder AddWebApi(
        this IHostApplicationBuilder builder
    ) {
        // register timeout/output caching services
        builder.Services.AddRequestTimeouts();
        builder.Services.AddOutputCache();
        builder.Services.AddProblemDetails();
        builder.Services.AddOpenApi();
        return builder;
    }

    public static WebApplication MapWebApi(this WebApplication app) {
        // Configure the HTTP request pipeline.
        app.UseHttpsRedirection();
        if (app.Environment.IsDevelopment()) {
            // OpenApi document generation endpoint + UI is only required in development
            app.MapOpenApi();
            app.UseSwaggerUI(x => {
                x.EnableTryItOutByDefault();
                // custom path to support AddOpenApi way of generating OpenAPI
                x.SwaggerEndpoint("/openapi/v1.json", "CAP API V1");
            });
        }

        // # Pre-configure the HTTP request pipeline.
        // add possibility to define endpoint specific request timeouts
        app.UseRequestTimeouts();
        // add possibility to add endpoint specific output caching
        app.UseOutputCache();
        // handle domain exceptions
        app.UseExceptionHandler(new ExceptionHandlerOptions {
            StatusCodeSelector = ex => ex switch {
                EntityNotFoundException => StatusCodes.Status404NotFound,
                ArgumentException => StatusCodes.Status400BadRequest,
                // handle optimistic locking
                ConcurrencyException => StatusCodes.Status409Conflict,
                NotImplementedException => StatusCodes.Status501NotImplemented,
                _ => StatusCodes.Status500InternalServerError
            }
        });
        // # Configure the HTTP request pipeline.
        app.MapGroup("/api").MapEndpoints();
        return app;
    }
}
