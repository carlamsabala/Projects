using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace WineCellarApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policyBuilder =>
                {
                    policyBuilder
                      .AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            app.UseCors("CorsPolicy");

            
            var currentDir = Directory.GetCurrentDirectory();
            var wwwPath = Path.Combine(currentDir, "..", "..", "www");
            if (Directory.Exists(wwwPath))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    RequestPath = "/app",
                    FileProvider = new PhysicalFileProvider(Path.GetFullPath(wwwPath)),
                    
                });
            }

            app.MapControllers();

            app.MapGet("/ping", () => "Wine Cellar API is up!");

            System.Console.WriteLine("Server is running on: " + string.Join(", ", app.Urls));

            app.Run();
        }
    }
}
