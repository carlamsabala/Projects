using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MainWebModuleApp
{
    
    [ApiController]
    [Route("api")]
    public class MyController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("Hello DelphiMVCFramework World");
        }

        [HttpGet("reversedstrings/{value}")]
        public IActionResult GetReversedString(string value)
        {
            if (value == null)
                return BadRequest("Value is required");
            string reversed = new string(value.Trim().Reverse().ToArray());
            return Ok(reversed);
        }

        [HttpGet("customers")]
        public IActionResult GetCustomers()
        {
            return Ok(Array.Empty<object>());
        }

        [HttpGet("customers/{id:int}")]
        public IActionResult GetCustomer(int id)
        {
            var customer = new { Id = id, Name = "Customer " + id };
            return Ok(customer);
        }

        [HttpPost("customers")]
        public IActionResult CreateCustomer([FromBody] dynamic customer)
        {
            return Created($"/api/customers/{customer.id}", customer);
        }

        [HttpPut("customers/{id:int}")]
        public IActionResult UpdateCustomer(int id, [FromBody] dynamic customer)
        {
            return Ok(customer);
        }

        [HttpDelete("customers/{id:int}")]
        public IActionResult DeleteCustomer(int id)
        {
            return NoContent();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();


            var app = builder.Build();

            app.UseStaticFiles();

            app.MapControllers();

            app.MapGet("/ping", () => "securite pong");

            Console.WriteLine($"Server is running on: http://{app.Urls.FirstOrDefault()}");

            app.Run();
        }
    }
}
