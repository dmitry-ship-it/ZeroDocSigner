using ZeroDocSigner.Api.Authentication.Exceptions;

namespace ZeroDocSigner.Api.Extensions;

public static class ApplicationBuilderExtension
{
    public static IApplicationBuilder UseExceptionLogger(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Exception>>();

            try
            {
                await next();
            }
            catch (AuthenticationException)
            {
                logger.LogWarning("Authentication exception occurred.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            catch (MissingCertificateException)
            {
                logger.LogWarning("User certificate with private key was not found.");
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
            }
            catch (Exception ex)
            {
                logger.LogWarning("{Exception}: {Message}. \n{StackTrace}", ex.GetType().Name, ex.Message, ex.StackTrace);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
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
