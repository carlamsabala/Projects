using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Swag.Common.Consts;
using Swag.Doc.Info;
using Swag.Doc.Tags;
using Swag.Doc.SecurityDefinition;
using Swag.Doc.Path;
using Swag.Doc.Definition;
using Swag.Doc.FileLoader;
using Swag.Common.Types;

namespace Swag.Doc
{
    public class SwagDoc
    {
        private SwagInfo _info;
        private List<SwagTag> _tags;
        private List<string> _consumes;
        private List<string> _produces;
        private string _basePath;
        private string _host;
        private HashSet<SwagTransferProtocolScheme> _schemes;
        private List<SwagPath> _paths;
        private List<SwagDefinition> _definitions;
        private List<SwagSecurityDefinition> _securityDefinitions;
        private SwagExternalDocs _externalDocs;
        private JToken _swaggerJson;
        private string _swaggerFilesFolder;
        private List<SwagRequestParameter> _parameters;

        public SwagDoc()
        {
            _info = new SwagInfo();
            _tags = new List<SwagTag>();
            _securityDefinitions = new List<SwagSecurityDefinition>();
            _consumes = new List<string>();
            _produces = new List<string>();
            _paths = new List<SwagPath>();
            _definitions = new List<SwagDefinition>();
            _externalDocs = new SwagExternalDocs();
            _parameters = new List<SwagRequestParameter>();
            _schemes = new HashSet<SwagTransferProtocolScheme>();
        }

        ~SwagDoc()
        {
            _consumes = null;
            _produces = null;
            _definitions = null;
            _paths = null;
            _info = null;
            _tags = null;
            _securityDefinitions = null;
            _externalDocs = null;
            _parameters = null;
            _swaggerJson = null;
        }

        public void GenerateSwaggerJson()
        {
            var json = new JObject();
            json["swagger"] = SwaggerVersion;
            json["info"] = _info.GenerateJsonObject();
            if (!string.IsNullOrEmpty(_host))
                json["host"] = _host;
            json["basePath"] = _basePath;
            if (_tags.Count > 0)
                json["tags"] = GenerateTagsJsonArray();
            if (_schemes.Count > 0)
                json["schemes"] = GenerateSchemesJsonArray();
            if (_consumes.Count > 0)
                json["consumes"] = GenerateMimeTypesJsonArray(_consumes);
            if (_produces.Count > 0)
                json["produces"] = GenerateMimeTypesJsonArray(_produces);
            if (_paths.Count > 0)
                json["paths"] = GeneratePathsJsonObject();
            if (_parameters.Count > 0)
                json["parameters"] = GenerateParametersJsonObject();
            if (_securityDefinitions.Count > 0)
                json["securityDefinitions"] = GenerateSecurityDefinitionsJsonObject();
            if (_definitions.Count > 0)
                json["definitions"] = GenerateDefinitionsJsonObject();
            if (_externalDocs != null)
                json["externalDocs"] = _externalDocs.GenerateJsonObject();
            _swaggerJson = json;
        }

        public void SaveSwaggerJsonToFile()
        {
            if (_swaggerJson == null)
                return;
            if (!Directory.Exists(_swaggerFilesFolder))
                Directory.CreateDirectory(_swaggerFilesFolder);
            string jsonString = _swaggerJson.ToString(Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(ReturnSwaggerFileName(), jsonString);
        }

        public void LoadFromFile(string filename)
        {
            var loader = new SwagFileLoader(this);
            loader.Load(filename);
        }

        public string SwaggerVersion => Consts.c_SwaggerVersion;

        public SwagInfo Info => _info;

        public string Host { get => _host; set => _host = value; }

        public string BasePath { get => _basePath; set => _basePath = value; }

        public HashSet<SwagTransferProtocolScheme> Schemes { get => _schemes; set => _schemes = value; }

        public List<string> Consumes => _consumes;

        public List<string> Produces => _produces;

        public List<SwagPath> Paths => _paths;

        public List<SwagDefinition> Definitions => _definitions;

        public List<SwagSecurityDefinition> SecurityDefinitions => _securityDefinitions;

        public List<SwagRequestParameter> Parameters => _parameters;

        public List<SwagTag> Tags => _tags;

        public SwagExternalDocs ExternalDocs => _externalDocs;

        public JToken SwaggerJson => _swaggerJson;

        public string SwaggerFilesFolder
        {
            get => _swaggerFilesFolder;
            set => _swaggerFilesFolder = value.EndsWith(Path.DirectorySeparatorChar.ToString()) ? value : value + Path.DirectorySeparatorChar;
        }

        private string ReturnSwaggerFileName()
        {
            return _swaggerFilesFolder + Consts.c_SwaggerFileName;
        }

        private JArray GenerateMimeTypesJsonArray(List<string> mimeTypes)
        {
            var arr = new JArray();
            foreach (var mime in mimeTypes)
                arr.Add(mime);
            return arr;
        }

        private JArray GenerateConsumesJsonArray()
        {
            return GenerateMimeTypesJsonArray(_consumes);
        }

        private JArray GenerateProducesJsonArray()
        {
            return GenerateMimeTypesJsonArray(_produces);
        }

        private JObject GenerateDefinitionsJsonObject()
        {
            var obj = new JObject();
            foreach (var def in _definitions)
            {
                if (def.JsonSchema != null)
                    obj[def.Name] = def.JsonSchema.DeepClone();
            }
            return obj;
        }

        private JObject GenerateParametersJsonObject()
        {
            var obj = new JObject();
            foreach (var param in _parameters)
                obj[param.Name] = param.GenerateJsonObject();
            return obj;
        }

        private JObject GeneratePathsJsonObject()
        {
            var obj = new JObject();
            foreach (var path in _paths)
                obj[path.Uri] = path.GenerateJsonObject();
            return obj;
        }

        private JArray GenerateTagsJsonArray()
        {
            var arr = new JArray();
            foreach (var tag in _tags)
                arr.Add(tag.GenerateJsonObject());
            return arr;
        }

        private JArray GenerateSchemesJsonArray()
        {
            var arr = new JArray();
            foreach (SwagTransferProtocolScheme scheme in Enum.GetValues(typeof(SwagTransferProtocolScheme)))
            {
                if (_schemes.Contains(scheme))
                    arr.Add(Consts.c_SwagTransferProtocolScheme[scheme]);
            }
            return arr;
        }

        private JObject GenerateSecurityDefinitionsJsonObject()
        {
            var obj = new JObject();
            foreach (var sec in _securityDefinitions)
                obj[sec.SchemeName] = sec.GenerateJsonObject();
            return obj;
        }
    }
}
