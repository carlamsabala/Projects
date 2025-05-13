using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace MyWebModuleApp
{

    public class Program
    {
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            
            builder.Services.AddControllersWithViews();

            
            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=LogsCollector}/{action=Index}/{id?}");

            app.MapGet("/", () => "Welcome to the Municipal Logs Collector API");

            Console.WriteLine("Web module started on: ");
            foreach (var url in app.Urls)
            {
                Console.WriteLine(url);
            }

            app.Run();
        }
    }
}
