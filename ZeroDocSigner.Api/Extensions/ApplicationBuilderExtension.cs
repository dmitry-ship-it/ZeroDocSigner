namespace ZeroDocSigner.Api.Extensions;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseExceptionLogger(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var logger = context.RequestServices.GetRequiredService<ILogger<Exception>>();
                logger.LogWarning("{Exception}: {Message}. \n{StackTrace}", ex, ex.Message, ex.StackTrace);

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(ex.Message);
            }
        });

    public static IApplicationBuilder UseSwaggerWithUI(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        return app;
    }
}
