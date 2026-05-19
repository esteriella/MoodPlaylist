using Microsoft.EntityFrameworkCore;
using MoodPlaylistApi.Data;

namespace MoodPlaylistApi.Startup
{
    public static class Database
    {
        public static void ConfigureDatabase(this WebApplicationBuilder builder)
        {// Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    connectionString,
                    sqlOptions =>
                    {
                        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorCodesToAdd: null
                        );
                        sqlOptions.CommandTimeout(60);
                    }
                );
            }, ServiceLifetime.Scoped);
        }
    }
}
