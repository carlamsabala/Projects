
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVCFramework.ActiveRecord; 
using MVCFramework.Commons;
using MVCFramework.Swagger.Commons; 

namespace MVCFramework.ActiveRecordController
{
    
    public delegate bool MVCActiveRecordAuthFunc(
        WebContext context, 
        Type activeRecordType, 
        MVCActiveRecordAction action);

    
    public enum MVCActiveRecordAction
    {
        Create,
        Retrieve,
        Update,
        Delete
    }

    
    [ApiController]
    [Route("[controller]")]
    public class MVCActiveRecordController : ControllerBase
    {
        private readonly ILogger<MVCActiveRecordController> _logger;
        private readonly MVCActiveRecordAuthFunc _authorization;
        private readonly string _urlSegment;

        public MVCActiveRecordController(ILogger<MVCActiveRecordController> logger,
                                           MVCActiveRecordAuthFunc authorization = null,
                                           string urlSegment = "")
        {
            _logger = logger;
            _authorization = authorization;
            _urlSegment = urlSegment;
        }

        protected virtual int GetMaxRecordCount()
        {
            
            return 20;
        }

        protected virtual bool CheckAuthorization(Type activeRecordType, MVCActiveRecordAction action)
        {
            if (_authorization != null)
            {
                return _authorization(new WebContext(HttpContext), activeRecordType, action);
            }
            return true;
        }

        [HttpGet("{entityname}")]
        public IActionResult GetEntities(string entityname)
        {
            try
            {
                
                if (ActiveRecordMappingRegistry.Instance.TryGetProcessor(entityname, out var processor))
                {
                    bool handled = false;
                    processor.GetEntities(new WebContext(HttpContext), this, entityname, ref handled);
                    if (handled)
                    {
                        return Ok();
                    }
                }

                if (!ActiveRecordMappingRegistry.Instance.TryGetEntityClass(entityname, out Type arType))
                {
                    return NotFound($"Cannot find entity nor processor for entity '{entityname}'.");
                }
                if (!CheckAuthorization(arType, MVCActiveRecordAction.Retrieve))
                {
                    return Forbid("Not authorized to read " + entityname);
                }

                string rql = Request.Query["rql"];
                _logger.LogDebug("[RQL PARSE]: {RQL}", rql);

                var arInstance = (MVCActiveRecord)Activator.CreateInstance(arType, true);
                var mapping = arInstance.GetMapping();

                var arList = MVCActiveRecord.SelectRQL(arType, rql, GetMaxRecordCount());

                var meta = new Dictionary<string, string>
                {
                    ["page_size"] = arList.Count.ToString()
                };
                if (Request.Query["count"].ToString().ToLower() == "true")
                {
                    meta.Add("count", MVCActiveRecord.Count(arType, rql).ToString());
                }

                var response = new
                {
                    data = arList,
                    meta = meta
                };
                return Ok(response);
            }
            catch (RQLCompilerNotFoundException ex)
            {
                _logger.LogError(ex, "RQL Compiler not found. Did you include the appropriate RQL parser?");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entities for {EntityName}", entityname);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{entityname}/searches")]
        public IActionResult GetEntitiesByRQL(string entityname)
        {
            return GetEntities(entityname);
        }

        [HttpPost("{entityname}/searches")]
        public IActionResult GetEntitiesByRQLwithPOST(string entityname, [FromBody] dynamic body)
        {
            string rql = body?.rql ?? "";
            Request.QueryString = new Microsoft.AspNetCore.Http.QueryString("?rql=" + Uri.EscapeDataString(rql));
            return GetEntities(entityname);
        }

        [HttpGet("{entityname}/{id:int}")]
        public IActionResult GetEntity(string entityname, int id)
        {
            try
            {
                if (ActiveRecordMappingRegistry.Instance.TryGetProcessor(entityname, out var processor))
                {
                    bool handled = false;
                    processor.GetEntity(new WebContext(HttpContext), this, entityname, id, ref handled);
                    if (handled)
                        return Ok();
                }
                if (!ActiveRecordMappingRegistry.Instance.TryGetEntityClass(entityname, out Type arType))
                {
                    return NotFound($"Cannot find entity {entityname}");
                }
                var arInstance = (MVCActiveRecord)Activator.CreateInstance(arType);
                if (!CheckAuthorization(arType, MVCActiveRecordAction.Retrieve))
                {
                    return Forbid("Not authorized to read " + entityname);
                }
                if (arInstance.LoadByPK(id))
                {
                    return Ok(new { data = arInstance });
                }
                else
                {
                    return NotFound($"{entityname.ToLower()} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity {EntityName} with ID {Id}", entityname, id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{entityname}")]
        public IActionResult CreateEntity(string entityname)
        {
            try
            {
                if (ActiveRecordMappingRegistry.Instance.TryGetProcessor(entityname, out var processor))
                {
                    bool handled = false;
                    processor.CreateEntity(new WebContext(HttpContext), this, entityname, ref handled);
                    if (handled)
                        return Created("", null);
                }
                if (!ActiveRecordMappingRegistry.Instance.TryGetEntityClass(entityname, out Type arType))
                {
                    return NotFound($"Cannot find entity {entityname}");
                }
                var arInstance = (MVCActiveRecord)Activator.CreateInstance(arType);
                if (!CheckAuthorization(arType, MVCActiveRecordAction.Create))
                {
                    return Forbid("Not authorized to create " + entityname);
                }

                Request.BindBody(arInstance);

                arInstance.Insert();
                Response.Headers["X-REF"] = $"{Request.Path}/{arInstance.GetPK().ToString()}";
                if (Request.Query["refresh"].ToString().ToLower() == "true")
                {
                    return Created(Request.Path, arInstance);
                }
                else
                {
                    return Created(Request.Path, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating entity {EntityName}", entityname);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{entityname}/{id:int}")]
        public IActionResult UpdateEntity(string entityname, int id)
        {
            try
            {
                if (ActiveRecordMappingRegistry.Instance.TryGetProcessor(entityname, out var processor))
                {
                    bool handled = false;
                    processor.UpdateEntity(new WebContext(HttpContext), this, entityname, id, ref handled);
                    if (handled)
                        return Ok();
                }
                if (!ActiveRecordMappingRegistry.Instance.TryGetEntityClass(entityname, out Type arType))
                {
                    return NotFound($"Cannot find class for entity {entityname}");
                }
                var arInstance = (MVCActiveRecord)Activator.CreateInstance(arType);
                if (!CheckAuthorization(arType, MVCActiveRecordAction.Update))
                {
                    return Forbid("Not authorized to update " + entityname);
                }
                if (!arInstance.LoadByPK(id))
                {
                    return NotFound($"Cannot find entity {entityname}");
                }
                Request.BindBody(arInstance);
                arInstance.SetPK(id);
                arInstance.Update();
                Response.Headers["X-REF"] = Request.Path;
                if (Request.Query["refresh"].ToString().ToLower() == "true")
                {
                    return Ok(new { data = arInstance });
                }
                else
                {
                    return Ok($"{entityname.ToLower()} updated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating entity {EntityName} with ID {Id}", entityname, id);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{entityname}/{id:int}")]
        public IActionResult DeleteEntity(string entityname, int id)
        {
            try
            {
                if (ActiveRecordMappingRegistry.Instance.TryGetProcessor(entityname, out var processor))
                {
                    bool handled = false;
                    processor.DeleteEntity(new WebContext(HttpContext), this, entityname, id, ref handled);
                    if (handled)
                        return Ok();
                }
                if (!ActiveRecordMappingRegistry.Instance.TryGetEntityClass(entityname, out Type arType))
                {
                    return NotFound($"Cannot find class for entity {entityname}");
                }
                var arInstance = (MVCActiveRecord)Activator.CreateInstance(arType);
                if (!CheckAuthorization(arType, MVCActiveRecordAction.Delete))
                {
                    return Forbid("Not authorized to delete " + entityname);
                }
                if (arInstance.LoadByPK(id))
                {
                    arInstance.SetPK(id);
                    arInstance.Delete();
                }
                return Ok($"{entityname.ToLower()} deleted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting entity {EntityName} with ID {Id}", entityname, id);
                return BadRequest(ex.Message);
            }
        }
    }

    
    public class MVCActiveRecordListResponse
    {
        public TMVCActiveRecordList Items { get; }
        public Dictionary<string, string> Metadata { get; }
        public bool Owns { get; }

        public MVCActiveRecordListResponse(TMVCActiveRecordList list, bool owns = true)
        {
            Items = list;
            Owns = owns;
            Metadata = new Dictionary<string, string>();
        }
    }

    #region Stub Types and Extensions

    
    public class WebContext
    {
        public Microsoft.AspNetCore.Http.HttpContext HttpContext { get; }
        public WebContext(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            HttpContext = httpContext;
        }
    }

    
    public class ActiveRecordMappingRegistry
    {
        private static readonly ActiveRecordMappingRegistry _instance = new ActiveRecordMappingRegistry();
        public static ActiveRecordMappingRegistry Instance => _instance;

        private readonly Dictionary<string, Type> _entityMap = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IMVCEntityProcessor> _processorMap = new Dictionary<string, IMVCEntityProcessor>(StringComparer.OrdinalIgnoreCase);

        public bool TryGetEntityClass(string urlSegment, out Type activeRecordType)
        {
            return _entityMap.TryGetValue(urlSegment.ToLower(), out activeRecordType);
        }

        public bool TryGetProcessor(string urlSegment, out IMVCEntityProcessor processor)
        {
            return _processorMap.TryGetValue(urlSegment.ToLower(), out processor);
        }

        
    }

    
    public class TMVCActiveRecordList : List<MVCActiveRecord>
    {
    }

    
    public static class RequestExtensions
    {
        public static void BindBody<T>(this Microsoft.AspNetCore.Http.HttpRequest request, T instance)
        {
            
            // This is just a stub.
        }
    }

    #endregion
}
