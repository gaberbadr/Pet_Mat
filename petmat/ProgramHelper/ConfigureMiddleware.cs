using Microsoft.Extensions.FileProviders;
using petmat.Middleware;

namespace petmat.ProgramHelper
{
    public static class ConfigureMiddleware
    {
        public static async Task<WebApplication> ConfigureMiddlewaresAsync(this WebApplication app)
        {
            // Use CORS first
            app.UseCors("AllowAll");

            // Configure Swagger
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                // Always enable Swagger for easy testing in production
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "petmat API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            // Enable serving static files from wwwroot
            app.UseStaticFiles();

            // Configure file serving with proper MIME types
            var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

            // Create directory if it doesn't exist
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".gif", ".webp",
                ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv",
                ".pdf", ".docx", ".xlsx", ".pptx", ".txt", ".rtf"
            };

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(filesPath),
                RequestPath = "/files",
                OnPrepareResponse = context =>
                {
                    var fileExtension = Path.GetExtension(context.File.Name).ToLowerInvariant();

                    // Block if not in allowed list
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        context.Context.Response.StatusCode = StatusCodes.Status404NotFound;
                        context.Context.Response.ContentLength = 0;
                        context.Context.Response.Body = Stream.Null;
                        return;
                    }

                    // Set caching headers for 1 year and correct MIME type
                    context.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000"; // 1 year cache
                    context.Context.Response.ContentType = fileExtension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        ".mp4" => "video/mp4",
                        ".webm" => "video/webm",
                        ".ogg" => "video/ogg",
                        ".mov" => "video/quicktime",
                        ".avi" => "video/x-msvideo",
                        ".mkv" => "video/x-matroska",
                        ".pdf" => "application/pdf",
                        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                        ".txt" => "text/plain",
                        ".rtf" => "application/rtf",
                        _ => "application/octet-stream"
                    };
                }
            });

            // Custom middleware - MUST come before UseStatusCodePages
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<UserActiveStatusMiddleware>();
            app.UseMiddleware<SecurityHeadersMiddleware>();

            // Status code pages - this will catch 404s and redirect to /error/{statusCode}
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            return app;
        }
    }
}