using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Controller.Customers
{
   
    [Route("api")]
    [ApiController]
    public class CustomersController : ControllerBase, IDisposable
    {
        private SqliteConnection _fdConn;

        public CustomersController()
        {
            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = "../../../data/activerecorddb.db"
            }.ToString();

            _fdConn = new SqliteConnection(connectionString);
            _fdConn.Open();

            ActiveRecordConnectionsRegistry.AddDefaultConnection(_fdConn);
        }

        
        [HttpGet("customers")]
        public IActionResult GetCustomers()
        {
            
            List<TCustomer> customers = TMVCActiveRecord.SelectRQL<TCustomer>("sort(+id)", 200);
            return Ok(customers);
        }

        
        [HttpGet("customers/{id}")]
        public IActionResult GetCustomer(int id)
        {
            TCustomer customer = TMVCActiveRecord.GetByPK<TCustomer>(id);
            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        
        [HttpPost("customers")]
        public IActionResult CreateCustomer([FromBody] TCustomer customer)
        {
            if (customer == null)
                return BadRequest();

            try
            {
                customer.Insert();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            
            return StatusCode(201);
        }

        
        [HttpPut("customers/{id}")]
        public IActionResult UpdateCustomer(int id, [FromBody] TCustomer customer)
        {
            if (customer == null)
                return BadRequest();

            
            customer.ID = id;

            try
            {
                customer.Update();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            return Ok(customer);
        }

        
        [HttpDelete("customers/{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            TCustomer customer = TMVCActiveRecord.GetByPK<TCustomer>(id);
            if (customer == null)
                return NotFound();

            try
            {
                customer.Delete();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

            
            return Ok(new { result = "register successefully deleted" });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ActiveRecordConnectionsRegistry.RemoveDefaultConnection();
                if (_fdConn != null)
                {
                    _fdConn.Close();
                    _fdConn.Dispose();
                    _fdConn = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    
    public static class ActiveRecordConnectionsRegistry
    {
        private static SqliteConnection _defaultConnection;
        public static void AddDefaultConnection(SqliteConnection connection)
        {
            _defaultConnection = connection;
        }

        public static void RemoveDefaultConnection()
        {
            _defaultConnection = null;
        }

        public static SqliteConnection DefaultConnection => _defaultConnection;
    }

        public abstract class TMVCActiveRecord
    {
        public abstract void Insert();
        public abstract void Update();
        public abstract void Delete();

        
        public static List<T> SelectRQL<T>(string query, int limit) where T : TMVCActiveRecord, new()
        {
            
            return new List<T>();
        }

        
        public static T GetByPK<T>(int id) where T : TMVCActiveRecord, new()
        {
            
            return new T();
        }
    }

    
    public class TCustomer : TMVCActiveRecord
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        

        public override void Insert()
        {
            // Insert logic here.
        }

        public override void Update()
        {
            // Update logic here.
        }

        public override void Delete()
        {
            // Delete logic here.
        }
    }
}
