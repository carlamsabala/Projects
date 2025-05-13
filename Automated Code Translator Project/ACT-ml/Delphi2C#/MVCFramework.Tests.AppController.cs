using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace MVCFramework.Tests
{
    // This model corresponds to the Delphi TAppUser
    public class AppUser
    {
        public int Cod { get; set; }
        public string Name { get; set; }
        public string Pass { get; set; }
    }

    // The controller routes are defined using attributes.
    // In Delphi they were defined with [MVCPath] and [MVCHTTPMethod]
    [ApiController]
    [Route("/")]
    public class AppController : ControllerBase
    {
        // GET /hello
        [HttpGet("hello")]
        public IActionResult HelloWorld()
        {
            return Ok("Hello World called with GET");
        }

        // GET /user
        [HttpGet("user")]
        public IActionResult GetUser()
        {
            var user = new AppUser
            {
                Cod = 1,
                Name = "Ezequiel",
                Pass = "123"
            };

            // Render returns the user as JSON (automatically by ASP.NET Core)
            return Ok(user);
        }

        // POST /user/save
        [HttpPost("user/save")]
        public async Task<IActionResult> PostUser()
        {
            // The framework will bind the JSON body to an AppUser object.
            // Using System.Text.Json (or you may use Newtonsoft.Json if preferred)
            var user = await System.Text.Json.JsonSerializer.DeserializeAsync<AppUser>(Request.Body);
            if (user != null && user.Cod > 0)
            {
                return Ok("Success!");
            }
            else
            {
                return BadRequest("Error!");
            }
        }

        // GET /users
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = new List<AppUser>();
            for (int i = 0; i <= 10; i++)
            {
                users.Add(new AppUser
                {
                    Cod = i,
                    Name = $"Ezequiel {i}",
                    Pass = i.ToString()
                });
            }

            return Ok(users);
        }

        // POST /users/save
        [HttpPost("users/save")]
        public async Task<IActionResult> PostUsers()
        {
            // Deserialize a JSON array of AppUser objects
            var users = await System.Text.Json.JsonSerializer.DeserializeAsync<List<AppUser>>(Request.Body);
            if (users != null && users.Count > 0)
            {
                return Ok("Success!");
            }
            else
            {
                return BadRequest("Error!");
            }
        }

        // POST /file/upload
        [HttpPost("file/upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ReceiveFile([FromForm] IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("No file found!");
            }

            // Compute the MD5 hash of the uploaded file stream
            using (var md5 = MD5.Create())
            using (var stream = file.OpenReadStream())
            {
                var hashBytes = await md5.ComputeHashAsync(stream);
                // Convert hash to a hexadecimal string
                var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return Ok(hashString);
            }
        }

        // POST /body-url-encoded
        [HttpPost("body-url-encoded")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult PostBodyURLEncoded([FromForm] Dictionary<string, string> formData)
        {
            var response = new JObject();
            response["field1"] = formData.TryGetValue("field1", out var field1) ? field1 : "";
            response["field2"] = formData.TryGetValue("field2", out var field2) ? field2 : "";
            response["field3"] = formData.TryGetValue("field3", out var field3) ? field3 : "";
            return Ok(response);
        }
    }
}
