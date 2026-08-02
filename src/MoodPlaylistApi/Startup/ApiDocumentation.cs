using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace MoodPlaylistApi.Startup
{
    public static class ApiDocumentation
    {
        public static void AddApiDocumentation(this WebApplicationBuilder builder)
        {
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MoodPlaylist API",
                    Version = "v1",
                    Description = "Create mood-based music recommendations, manage personal playlists, and discover playlists shared by the community.",
                    Contact = new OpenApiContact { Name = "MoodPlaylist API Support" },
                    License = new OpenApiLicense { Name = "MIT" }
                });

                var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste the JWT returned by the login or registration endpoint."
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            });
        }

        public static void UseApiDocumentation(this WebApplication app)
        {
            app.UseStaticFiles();
            app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");
            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "swagger";
                options.SwaggerEndpoint("/openapi/v1.json", "MoodPlaylist API v1");
                options.DocumentTitle = "MoodPlaylist API — Swagger";
                options.DisplayRequestDuration();
                options.EnableFilter();
                options.EnablePersistAuthorization();
                options.InjectStylesheet("/docs/swagger-dark.css");
            });

            app.MapScalarApiReference("/scalar", options => options
                .WithTitle("MoodPlaylist API")
                .WithOpenApiRoutePattern("/openapi/{documentName}.json")
                .AddDocument("v1", "MoodPlaylist API v1")
                .ForceDarkMode()
                .ShowOperationId()
                .SortTagsAlphabetically()
                .SortOperationsByMethod()
                .AddPreferredSecuritySchemes("Bearer")
                .DisableAgent())
                .AllowAnonymous();
        }
    }
}
