namespace MyApp.Controllers { 
    [RoutePrefix("")] 
    public class PublicController : ApiController { [HttpGet] [Route("")] 
        public IHttpActionResult Index() { 
        
            return Ok("Hello World"); 
        } 
    } 
}