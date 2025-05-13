using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using EntitiesU;          
using CustomerServiceU;  

namespace CustomersControllerU
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        
        [HttpGet]
        public IActionResult GetCustomers(
            [FromQuery(Name = "rql")] string rqlFilter = "",
            [FromServices] ICustomersService customersService)
        {
            var result = customersService.GetByRQL(rqlFilter);
            return Ok(result);
        }

        
        [HttpGet("{id}")]
        public IActionResult GetCustomerByID(
            int id,
            [FromServices] ICustomersService customersService)
        {
            var customer = customersService.GetByID(id);
            return Ok(customer);
        }

        
        [HttpPut("{id}")]
        public IActionResult UpdateCustomerByID(
            int id,
            [FromBody] TCustomer customer,
            [FromServices] ICustomersService customersService)
        {
            customersService.UpdateByID(id, customer);
            return Ok();
        }

        
        [HttpPost]
        public IActionResult CreateCustomer(
            [FromBody] TCustomer customer,
            [FromServices] ICustomersService customersService)
        {
            var newId = customersService.CreateCustomer(customer);
            return Created($"/api/customers/{newId}", null);
        }

        
        [HttpPost("_bulk")]
        public IActionResult BulkCreateCustomers(
            [FromBody] List<TCustomer> customers,
            [FromServices] ICustomersService customersService)
        {
            customersService.CreateCustomers(customers);
            return Created(string.Empty, null);
        }
    }
}
