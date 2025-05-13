using Microsoft.AspNetCore.Mvc;

namespace Controller2U
{

    [Route("controller2")]
    [ApiController]
    public class MyController2 : ControllerBase
    {
       
        [HttpGet]
        public IActionResult Index()
        {
            
            return Ok("Hello DelphiMVCFramework World");
        }
    }
}
