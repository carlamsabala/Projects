using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace AppControllerU
{
    
    [Route("/")]
    [ApiController]
    public class App1MainController : ControllerBase
    {
        
        [HttpGet("public")]
        public IActionResult PublicSection()
        {
            return Content("This is a public section", "text/plain");
        }
        
        
        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect("/index.html");
        }
    }
    
    
    [Route("admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        [HttpGet("role1")]
        public IActionResult OnlyRole1()
        {
            if (Request.Headers.TryGetValue("Accept", out StringValues acceptHeader) &&
                acceptHeader.Any(h => h.Contains("application/json", StringComparison.OrdinalIgnoreCase)))
            {
                var response = new Dictionary<string, object>
                {
                    ["message"] = "This is protected content accessible only by user1"
                };

                var queryParams = Request.Query.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString());
                response["querystringparameters"] = queryParams;
                
                return new JsonResult(response);
            }
            else
            {
                
                var customValue = HttpContext.User?.FindFirst("custom")?.Value ?? "default_custom_value";
                var userName = HttpContext.User?.Identity?.Name ?? "Anonymous";
                var roles = HttpContext.User?.FindAll("role").Select(r => r.Value).ToArray() ?? new string[] { "NoRoles" };
                var rolesText = string.Join(Environment.NewLine, roles);
                
                string result = $"{customValue}{Environment.NewLine}" +
                                $"Hey! Hello {userName}, now you are a logged user and this is a protected content!{Environment.NewLine}" +
                                $"As logged user you have the following roles:{Environment.NewLine}{rolesText}";
                return Content(result, "text/plain");
            }
        }
        
        [HttpGet("role2")]
        [Produces("text/html")]
        public IActionResult OnlyRole2()
        {
            var customValue = HttpContext.User?.FindFirst("custom")?.Value ?? "default_custom_value";
            var userName = HttpContext.User?.Identity?.Name ?? "Anonymous";
            var roles = HttpContext.User?.FindAll("role").Select(r => r.Value).ToArray() ?? new string[] { "NoRoles" };
            var rolesText = string.Join(Environment.NewLine, roles);
            
            string result = $"{customValue}{Environment.NewLine}" +
                            $"Hey! Hello {userName}, now you are a logged user and this is a protected content!{Environment.NewLine}" +
                            $"As logged user you have the following roles:{Environment.NewLine}{rolesText}";
            return Content(result, "text/plain");
        }
    }
}
