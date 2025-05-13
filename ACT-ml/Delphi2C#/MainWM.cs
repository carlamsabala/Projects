using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Linq;

namespace MunicipalLibraryApp
{

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddControllersWithViews();

            
            builder.Services.AddResponseCompression(options =>
            {
                
                options.EnableForHttps = true;
                
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
            });

            
            var app = builder.Build();

           
            app.UseResponseCompression();

            
            app.Use(async (context, next) =>
            {
               
                await next.Invoke();
            });

            
            var wwwFolder = Path.Combine(Directory.GetCurrentDirectory(), "www");
            if (Directory.Exists(wwwFolder))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    
                    RequestPath = "/app",
                    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(Path.GetFullPath(wwwFolder))
                });
            }

            app.MapControllers();

            app.MapGet("/ping", () => "Hello from Municipal Library API");

            
            Console.WriteLine("Municipal Library API is running on the following URLs:");
            foreach (var url in app.Urls)
            {
                Console.WriteLine(url);
            }
            app.Run();
        }
    }
}
