using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace MyMvcApp.Controllers
{
    
    [Route("api")]
    [ApiController]
    public class MainController : ControllerBase
    {
        
        [HttpGet]
        public IActionResult Index()
        {
            
            return Content("Hello DelphiMVCFramework World");
        }

        
        [HttpGet("reversedstrings/{value}")]
        public IActionResult GetReversedString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return BadRequest("Value cannot be empty.");
            }
            
            string reversed = new string(value.Trim().Reverse().ToArray());
            return Content(reversed);
        }

        
        [HttpGet("customers")]
        public IActionResult GetCustomers()
        {
           
            return Ok("GetCustomers not implemented");
        }

        
        [HttpGet("customers/{id:int}")]
        public IActionResult GetCustomer(int id)
        {
            
            return Ok($"GetCustomer {id} not implemented");
        }

        
        [HttpPost("customers")]
        public IActionResult CreateCustomer()
        {
            
            return Ok("CreateCustomer not implemented");
        }

        
        [HttpPut("customers/{id:int}")]
        public IActionResult UpdateCustomer(int id)
        {
            
            return Ok($"UpdateCustomer {id} not implemented");
        }

        
        [HttpDelete("customers/{id:int}")]
        public IActionResult DeleteCustomer(int id)
        {
            
            return Ok($"DeleteCustomer {id} not implemented");
        }

        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            
            base.OnActionExecuted(context);
        }
    }
}
