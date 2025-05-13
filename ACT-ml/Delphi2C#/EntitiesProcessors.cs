using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;

namespace EntitiesProcessors
{
    
    public class WebContext
    {
        public RequestData Request { get; set; } = new RequestData();
        public ResponseData Response { get; set; } = new ResponseData();
    }

    public class RequestData
    {
        
        public T BodyAs<T>()
        {
            
            return Activator.CreateInstance<T>();
        }
        public string PathInfo { get; set; } = "/api/contacts";
        public string Body { get; set; }
    }

    public class ResponseData
    {
        public Dictionary<string, string> CustomHeaders { get; set; } = new Dictionary<string, string>();
    }

    
    public class MVCRenderer
    {
        public void Render(object obj, bool formatted = true)
        {
            
            Console.WriteLine(JsonConvert.SerializeObject(obj, Formatting.Indented));
        }
    }

    
    public class Link
    {
        public string HREF { get; set; }
        public string TYPE { get; set; }
        public string REL { get; set; }
    }

    
    public class TArticle
    {
        public string Description { get; set; } = "Default description";
        public List<Link> Links { get; set; } = new List<Link>();

        public void Insert()
        {
            
            Console.WriteLine("Article inserted.");
        }
    }

    
    public class TContact
    {
        // Properties for a contact entity.
    }

    public class TPhone
    {
        public int IDPerson { get; set; }
        public void Insert()
        {
            
            Console.WriteLine("Phone inserted.");
        }
    }

    public class TPerson
    {
        public long id { get; set; }
        public void Insert()
        {
            
            Console.WriteLine("Person inserted.");
            
            id = new Random().Next(1000, 9999);
        }
    }

    
    public static class TMVCActiveRecord
    {
        
        public static List<T> All<T>() where T : new() => new List<T>();

        
        public static T GetByPK<T>(int id) where T : new() => new T();

        public static List<T> Where<T>(string query, params object[] args) where T : new() => new List<T>();

        public static ConnectionWrapper CurrentConnection { get; } = new ConnectionWrapper();
    }

    public class ConnectionWrapper
    {
        public void StartTransaction() { Console.WriteLine("Transaction started."); }
        public void Commit() { Console.WriteLine("Transaction committed."); }
        public void Rollback() { Console.WriteLine("Transaction rolled back."); }
    }

    public enum MVCSerializationType { stDefault }

    public class MVCTJsonDataObjectsSerializer : IDisposable
    {
        public JObject ParseObject(string body)
        {
            return JObject.Parse(body);
        }

        public void JsonObjectToObject(JObject json, object target, MVCSerializationType serializationType, object options)
        {
            JsonConvert.PopulateObject(json.ToString(), target);
        }

        public void JsonArrayToList<T>(JArray jsonArray, List<T> list, MVCSerializationType serializationType, object options)
        {
            var items = jsonArray.ToObject<List<T>>();
            list.AddRange(items);
        }

        public void ObjectToJsonObject(object obj, JObject json, MVCSerializationType serializationType, object options)
        {
            var temp = JObject.FromObject(obj);
            json.Merge(temp);
        }

        public void ListToJsonArray<T>(List<T> list, JArray jsonArray, MVCSerializationType serializationType, object options)
        {
            jsonArray.Merge(JArray.FromObject(list));
        }

        public void Dispose()
        {
            // Dispose resources if needed.
        }
    }

    public List<T> WrapAsList<T>(List<T> list) => list;

    public class MVCRResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public MVCRResponse(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }

    public static class ActiveRecordMappingRegistry
    {
        private static Dictionary<string, IMVCEntityProcessor> _processors = new Dictionary<string, IMVCEntityProcessor>();
        public static void AddEntityProcessor(string entityName, IMVCEntityProcessor processor)
        {
            _processors[entityName] = processor;
            Console.WriteLine($"Entity processor for '{entityName}' registered.");
        }
    }
    public interface IMVCEntityProcessor
    {
        void CreateEntity(WebContext context, MVCRenderer renderer, string entityname, ref bool handled);
        void GetEntities(WebContext context, MVCRenderer renderer, string entityname, ref bool handled);
        void GetEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled);
        void UpdateEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled);
        void DeleteEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled);
    }
    public class ArticleProcessor : IMVCEntityProcessor
    {
        public void CreateEntity(WebContext context, MVCRenderer renderer, string entityname, ref bool handled)
        {
            TArticle article = context.Request.BodyAs<TArticle>();
            try
            {
                article.Insert();
                renderer.Render(article, false);
            }
            finally
            {
                
                (article as IDisposable)?.Dispose();
            }
            handled = true;
        }

        public void GetEntities(WebContext context, MVCRenderer renderer, string entityname, ref bool handled)
        {
            handled = true;
            
            List<TArticle> articles = TMVCActiveRecord.All<TArticle>();

            foreach (var article in articles)
            {
                string encoded = WebUtility.UrlEncode(article.Description);
                article.Links.Add(new Link
                {
                    HREF = "https://www.google.com/search?q=" + encoded,
                    TYPE = "text/html",
                    REL = "googlesearch"
                });
            }

            var dict = new Dictionary<string, object> { { "data", articles } };
            renderer.Render(dict);
        }

        public void GetEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled)
        {
            handled = false;
        }

        public void UpdateEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled)
        {
            handled = false;
        }

        public void DeleteEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled)
        {
            handled = false;
        }
    }

    public class ContactProcessor : IMVCEntityProcessor
    {
        public void CreateEntity(WebContext context, MVCRenderer renderer, string entityname, ref bool handled)
        {
            handled = true;
            long lID = 0;
            using (var ser = new MVCTJsonDataObjectsSerializer())
            {
                JObject lJSON = ser.ParseObject(context.Request.Body);
                try
                {
                    TPerson person = new TPerson();
                    ser.JsonObjectToObject(lJSON, person, MVCSerializationType.stDefault, null);

                    List<TPhone> phones = new List<TPhone>();
                    ser.JsonArrayToList(lJSON["phones"] as JArray, phones, MVCSerializationType.stDefault, null);

                    TMVCActiveRecord.CurrentConnection.StartTransaction();
                    try
                    {
                        person.Insert();
                        lID = person.id;
                        foreach (var phone in phones)
                        {
                            phone.IDPerson = (int)person.id;
                            phone.Insert();
                        }
                        TMVCActiveRecord.CurrentConnection.Commit();
                    }
                    catch (Exception)
                    {
                        TMVCActiveRecord.CurrentConnection.Rollback();
                        throw;
                    }
                }
                finally
                {
                    // Free the JSON object if needed.
                }
            }
            context.Response.CustomHeaders["X-REF"] = context.Request.PathInfo + "/" + lID.ToString();
            renderer.Render(new MVCRResponse(201, "Contact created with phones"));
        }

        public void GetEntities(WebContext context, MVCRenderer renderer, string entityname, ref bool handled)
        {
            handled = false; 
        }

        public void GetEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled)
        {
            TContact contact = TMVCActiveRecord.GetByPK<TContact>(id);
            List<TPhone> phones = TMVCActiveRecord.Where<TPhone>("id_person = ?", id.ToString());
            using (var ser = new MVCTJsonDataObjectsSerializer())
            {
                JObject lJSON = new JObject();
                ser.ObjectToJsonObject(contact, lJSON, MVCSerializationType.stDefault, null);
                if (lJSON["phones"] == null)
                    lJSON["phones"] = new JArray();
                ser.ListToJsonArray(phones, lJSON["phones"] as JArray, MVCSerializationType.stDefault, null);
                renderer.Render(lJSON, false);
            }
            handled = true;
        }

        public void UpdateEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled)
        {
            handled = false;
        }

        public void DeleteEntity(WebContext context, MVCRenderer renderer, string entityname, int id, ref bool handled)
        {
            handled = false;
        }
    }


    public static class EntitiesProcessorsRegistration
    {
        static EntitiesProcessorsRegistration()
        {
            ActiveRecordMappingRegistry.AddEntityProcessor("articles", new ArticleProcessor());
            ActiveRecordMappingRegistry.AddEntityProcessor("contacts", new ContactProcessor());
        }
    }
}
