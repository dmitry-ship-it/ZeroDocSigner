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
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(ex.Message);
            }
        });
}
