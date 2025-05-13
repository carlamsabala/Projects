using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace frmGenerate
{

    public class SwagDoc
    {
        public SwagInfo Info { get; set; } = new SwagInfo();
        public string Host { get; set; }
        public string BasePath { get; set; }
        public List<string> Consumes { get; set; } = new List<string>();
        public List<string> Produces { get; set; } = new List<string>();
        public List<string> Schemes { get; set; } = new List<string>();
        public List<SwagDefinition> Definitions { get; set; } = new List<SwagDefinition>();
        public List<SwagPath> Paths { get; set; } = new List<SwagPath>();
        public JObject SwaggerJson { get; set; }

        public void GenerateSwaggerJson()
        {
            
            var doc = new JObject
            {
                ["swagger"] = "2.0",
                ["info"] = Info.ToJObject(),
                ["host"] = Host,
                ["basePath"] = BasePath,
                ["consumes"] = new JArray(Consumes),
                ["produces"] = new JArray(Produces),
                ["schemes"] = new JArray(Schemes)
            };

            var defs = new JObject();
            foreach (var def in Definitions)
            {
                defs[def.Name] = def.JsonSchema;
            }
            doc["definitions"] = defs;

            var paths = new JObject();
            foreach (var path in Paths)
            {
                paths[path.Uri] = path.ToJObject();
            }
            doc["paths"] = paths;

            SwaggerJson = doc;
        }
    }

    public class SwagInfo
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public string TermsOfService { get; set; }
        public string Description { get; set; }
        public SwagContact Contact { get; set; } = new SwagContact();
        public SwagLicense License { get; set; } = new SwagLicense();

        public JObject ToJObject()
        {
            return new JObject
            {
                ["title"] = Title,
                ["version"] = Version,
                ["termsOfService"] = TermsOfService,
                ["description"] = Description,
                ["contact"] = Contact.ToJObject(),
                ["license"] = License.ToJObject()
            };
        }
    }

    public class SwagContact
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Url { get; set; }
        public JObject ToJObject()
        {
            return new JObject
            {
                ["name"] = Name,
                ["email"] = Email,
                ["url"] = Url
            };
        }
    }

    public class SwagLicense
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public JObject ToJObject()
        {
            return new JObject
            {
                ["name"] = Name,
                ["url"] = Url
            };
        }
    }

    public class SwagDefinition
    {
        public string Name { get; set; }
        public JObject JsonSchema { get; set; }
        public JObject GenerateJsonRefDefinition()
        {
            return new JObject(new JProperty("$ref", "#/definitions/" + Name));
        }
    }

    public class SwagPath
    {
        public string Uri { get; set; }
        public List<SwagPathOperation> Operations { get; set; } = new List<SwagPathOperation>();
        public JObject ToJObject()
        {
            var obj = new JObject();
            foreach (var op in Operations)
            {
                obj[op.Operation.ToLower()] = op.ToJObject();
            }
            return obj;
        }
    }

    public class SwagPathOperation
    {
        public string Operation { get; set; } 
        public string OperationId { get; set; }
        public string Description { get; set; }
        public List<SwagRequestParameter> Parameters { get; set; } = new List<SwagRequestParameter>();
        public Dictionary<string, SwagResponse> Responses { get; set; } = new Dictionary<string, SwagResponse>();
        public List<string> Tags { get; set; } = new List<string>();

        public JObject ToJObject()
        {
            var obj = new JObject
            {
                ["operationId"] = OperationId,
                ["description"] = Description,
                ["tags"] = new JArray(Tags)
            };

            var jParams = new JArray();
            foreach (var param in Parameters)
            {
                jParams.Add(param.ToJObject());
            }
            obj["parameters"] = jParams;

            var jResponses = new JObject();
            foreach (var kvp in Responses)
            {
                jResponses[kvp.Key] = kvp.Value.ToJObject();
            }
            obj["responses"] = jResponses;

            return obj;
        }
    }

    public class SwagRequestParameter
    {
        public string Name { get; set; }
        public string InLocation { get; set; }   
        public string Description { get; set; }
        public bool Required { get; set; }
        public string TypeParameter { get; set; }  
        public SwagSchema Schema { get; set; } = new SwagSchema();
        public JObject ToJObject()
        {
            var obj = new JObject
            {
                ["name"] = Name,
                ["in"] = InLocation,
                ["description"] = Description,
                ["required"] = Required
            };

            if (!string.IsNullOrEmpty(TypeParameter))
            {
                obj["type"] = TypeParameter;
            }
            if (!string.IsNullOrEmpty(Schema.Name))
            {
                obj["schema"] = new JObject { ["$ref"] = "#/definitions/" + Schema.Name };
            }
            return obj;
        }
    }

    public class SwagSchema
    {
        public string Name { get; set; }
    }

    public class SwagResponse
    {
        public string StatusCode { get; set; }
        public string Description { get; set; }
        public SwagSchema Schema { get; set; } = new SwagSchema();
        public List<SwagHeaders> Headers { get; set; } = new List<SwagHeaders>();
        public JObject ToJObject()
        {
            var obj = new JObject
            {
                ["description"] = Description
            };

            if (!string.IsNullOrEmpty(Schema.Name))
            {
                obj["schema"] = new JObject { ["$ref"] = "#/definitions/" + Schema.Name };
            }

            if (Headers.Count > 0)
            {
                var jHeaders = new JObject();
                foreach (var header in Headers)
                {
                    jHeaders[header.Name] = header.ToJObject();
                }
                obj["headers"] = jHeaders;
            }
            return obj;
        }
    }

    public class SwagHeaders
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ValueType { get; set; }
        public JObject ToJObject()
        {
            return new JObject
            {
                ["description"] = Description,
                ["type"] = ValueType
            };
        }
    }

    public class Form1 : Form
    {
        private TextBox memo1;
        private Button btnGenerate;

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.memo1 = new TextBox();
            this.btnGenerate = new Button();
            
            this.memo1.Multiline = true;
            this.memo1.ScrollBars = ScrollBars.Both;
            this.memo1.Location = new Point(12, 12);
            this.memo1.Size = new Size(760, 400);
            this.memo1.Font = new Font("Consolas", 10);
            
            this.btnGenerate.Location = new Point(12, 420);
            this.btnGenerate.Size = new Size(150, 30);
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
             
            this.ClientSize = new Size(784, 461);
            this.Controls.Add(this.memo1);
            this.Controls.Add(this.btnGenerate);
            this.Text = "Swagger Generator";
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            
            SwagDoc swagDoc = new SwagDoc();

            
            swagDoc.Info.Title = "Sample API";
            swagDoc.Info.Version = "v1.2";
            swagDoc.Info.TermsOfService = "https://example.com/someurl/tos";
            swagDoc.Info.Description = "Sample API Description";
            swagDoc.Info.Contact.Name = "John Smith";
            swagDoc.Info.Contact.Email = "jsmith@example.com";
            swagDoc.Info.Contact.Url = "https://example.com/contact";
            swagDoc.Info.License.Name = "Some License";
            swagDoc.Info.License.Url = "https://example.com/license";

            
            swagDoc.Host = "example.com";
            swagDoc.BasePath = "/basepath";

            
            swagDoc.Consumes.Add("application/json");
            swagDoc.Produces.Add("text/xml");
            swagDoc.Produces.Add("application/json");

            
            swagDoc.Schemes.Add("https");

            
            SwagDefinition defSomeSubType = new SwagDefinition
            {
                Name = "SomeSubType",
                JsonSchema = CreateJsonSomeSubType()
            };
            swagDoc.Definitions.Add(defSomeSubType);

            SwagDefinition defSomeType = new SwagDefinition
            {
                Name = "SomeType",
                JsonSchema = CreateJsonSomeType(defSomeSubType.GenerateJsonRefDefinition())
            };
            swagDoc.Definitions.Add(defSomeType);

            
            SwagPath path = new SwagPath
            {
                Uri = "/path/request/{param1}"
            };

            
            SwagPathOperation operation = new SwagPathOperation
            {
                Operation = "post",
                OperationId = "RequestData",
                Description = "Requests some data"
            };

            
            SwagRequestParameter param1 = new SwagRequestParameter
            {
                Name = "param1",
                InLocation = "path",
                Description = "A param required",
                Required = true,
                TypeParameter = "string"
            };
            operation.Parameters.Add(param1);

            
            SwagRequestParameter param2 = new SwagRequestParameter
            {
                Name = "param2",
                InLocation = "query",
                Description = "A param that is not required",
                Required = false,
                TypeParameter = "string"
            };
            operation.Parameters.Add(param2);

           
            SwagRequestParameter param3 = new SwagRequestParameter
            {
                Name = "param3",
                InLocation = "body",
                Required = true
            };
            param3.Schema.Name = "SomeType";
            operation.Parameters.Add(param3);

            SwagResponse response200 = new SwagResponse
            {
                StatusCode = "200",
                Description = "Successfully retrieved data"
            };
            response200.Schema.Name = "SomeType";

            SwagHeaders header = new SwagHeaders
            {
                Name = "X-Rate-Limit-Limit",
                Description = "The number of allowed requests in the current period",
                ValueType = "integer"
            };
            response200.Headers.Add(header);
            operation.Responses.Add("200", response200);

            SwagResponse responseDefault = new SwagResponse
            {
                StatusCode = "
 