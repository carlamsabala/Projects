using Microsoft.AspNetCore.Mvc;

namespace TestServer.Controllers
{
    [Route("privatecustom")]
    [ApiController]
    public class TestPrivateServerControllerCustomAuth : ControllerBase
    {
        [HttpGet("role1")]
        public IActionResult OnlyRole1()
        {
            return Ok("Here's Action1 from the private controller");
        }

        [HttpGet("role2")]
        public IActionResult OnlyRole2()
        {
            return Ok("Here's Action2 from the private controller");
        }
    }
}
