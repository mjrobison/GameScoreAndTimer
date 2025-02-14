using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scoreboard.Hubs;
using Scoreboard.Services;
using System.Threading;

namespace Scoreboard
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSignalR();
            builder.Services.AddHttpClient();

            // Register services
            builder.Services.AddSingleton<GameClockService>(); 
            builder.Services.AddSingleton<ScoreboardService>();
            builder.Services.AddSingleton<KeyListenerService>(); // Register KeyListenerService

            // Allow only the frontend origin (replace with your actual frontend URL)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigins",
                    builder => builder
                        .WithOrigins("http://localhost:3001")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()); // Required for SignalR
            });

            var app = builder.Build();
            app.UseRouting();

            // Apply CORS policy
            app.UseCors("AllowSpecificOrigins");
            app.MapHub<ScoreboardHub>("/scoreboard");

            // Get KeyListenerService and start it in a background thread
            var keyListener = app.Services.GetRequiredService<KeyListenerService>();
            Thread keyListenerThread = new Thread(() => keyListener.StartListening());
            keyListenerThread.IsBackground = true;
            keyListenerThread.Start();

            app.Run();
        }
    }
}
