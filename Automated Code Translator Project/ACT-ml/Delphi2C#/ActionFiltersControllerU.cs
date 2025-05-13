using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ActionFiltersControllerU
{
    [Route("/")]
    [ApiController]
    public class ActionFiltersController : ControllerBase
    {
        private readonly ILogger<ActionFiltersController> _logger;

        public ActionFiltersController(ILogger<ActionFiltersController> logger)
        {
            _logger = logger;
            _logger.LogInformation("MVCControllerAfterCreate");
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                
                context.Result = BadRequest("You cannot use this service in the Weekend");
                return;
            }

            var resultContext = await next();

            var actionName = context.ActionDescriptor.DisplayName;
            var path = context.HttpContext.Request.Path;
            var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            _logger.LogInformation($"ACTION CALLED: {actionName} mapped to {path} from {clientIp}");
        }


        [HttpGet("people/{id}")]
        [Produces("application/json")]
        public IActionResult GetPerson(int id)
        {
            var person = new Person
            {
                Id = id,
                FirstName = "Daniele",
                LastName = "Teti",
                DOB = new DateTime(1975, 5, 2),
                Married = true
            };

            return Ok(person);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class Person
    {
        public int Id { get; set; } 
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public bool Married { get; set; }
    }
}
