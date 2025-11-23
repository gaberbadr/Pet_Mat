using Microsoft.Extensions.FileProviders;
using petmat.Hubs;
using petmat.Middleware;

namespace petmat.ProgramHelper
{
    public static class ConfigureMiddleware
    {
        public static async Task<WebApplication> ConfigureMiddlewaresAsync(this WebApplication app)
        {
            // ORDER MATTERS - CORS must be first!
            app.UseCors("AllowAll");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "petmat API V1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();

            // Enable serving static files
            app.UseStaticFiles();

            // Configure file serving
            var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
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

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        context.Context.Response.StatusCode = StatusCodes.Status404NotFound;
                        context.Context.Response.ContentLength = 0;
                        context.Context.Response.Body = Stream.Null;
                        return;
                    }

                    context.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000";
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

            app.UseRouting();

            // Rate limiter BEFORE authentication
            app.UseRateLimiter();

            // Authentication/Authorization BEFORE custom middleware
            app.UseAuthentication();
            app.UseAuthorization();

            // Custom middleware
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseMiddleware<UserActiveStatusMiddleware>();
            app.UseMiddleware<SecurityHeadersMiddleware>();

            // Status code pages
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            // ⚠Map endpoints at the end
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<ChatHub>("/chatHub"); // SignalR Hub
            });

            return app;
        }
    }
}