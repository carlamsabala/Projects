using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TestServer.Controllers
{
    public class EMyException : Exception
    {
        public EMyException(string message) : base(message) { }
    }

    [Route("exception/aftercreate")]
    public class TestServerControllerExceptionAfterCreate : ControllerBase
    {
        public TestServerControllerExceptionAfterCreate()
        {
            MVCControllerAfterCreate();
        }

        protected virtual void MVCControllerAfterCreate()
        {
            throw new EMyException("This is an exception raised in the MVCControllerAfterCreate");
        }

        protected virtual void MVCControllerBeforeDestroy() { }

        [HttpGet("nevercalled")]
        public IActionResult NeverCalled()
        {
            return StatusCode(500, "This method should not be called...");
        }
    }

    [Route("exception/beforedestroy")]
    public class TestServerControllerExceptionBeforeDestroy : ControllerBase
    {
        protected virtual void MVCControllerAfterCreate() { }

        protected virtual void MVCControllerBeforeDestroy()
        {
            throw new EMyException("This is an exception raised in the MVCControllerBeforeDestroy");
        }

        [HttpGet("nevercalled")]
        public IActionResult NeverCalled()
        {
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                MVCControllerBeforeDestroy();
            }
            base.Dispose(disposing);
        }
    }

    [Route("actionfilters/beforeaction")]
    public class TestServerControllerActionFilters : ControllerBase
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (string.Equals(context.ActionDescriptor.RouteValues["action"], "NeverCalled", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new StatusCodeResult(404);
            }
            base.OnActionExecuting(context);
        }

        [HttpGet("alwayscalled")]
        public IActionResult AlwaysCalled()
        {
            Response.StatusCode = 200;
            return Ok();
        }

        [HttpGet("nevercalled")]
        public IActionResult NeverCalled()
        {
            throw new Exception("Should never be called!");
        }
    }
}
