using System;
using System.Text.Json.Nodes;

namespace Swag.Doc
{
    public enum SwagSecurityDefinitionApiKeyInLocation
    {
        NotDefined,
        Query,
        Header
    }

    [SecurityDefinition(SwagSecurityDefinitionType.ApiKey)]
    public class SwagSecurityDefinitionApiKey : SwagSecurityDefinition
    {
        private string _name;
        private SwagSecurityDefinitionApiKeyInLocation _inLocation;

        public override SwagSecurityDefinitionType GetTypeSecurity()
        {
            return SwagSecurityDefinitionType.ApiKey;
        }

        public override JsonObject GenerateJsonObject()
        {
            var json = new JsonObject();
            json["type"] = ReturnTypeSecurityToString();
            if (!string.IsNullOrEmpty(Description))
                json["description"] = Description;
            json["in"] = InLocationToString(_inLocation);
            json["name"] = _name;
            return json;
        }

        public override void Load(JsonObject json)
        {
            if (json.TryGetPropertyValue("description", out JsonNode desc))
                Description = desc.ToString();
            if (json.TryGetPropertyValue("name", out JsonNode name))
                _name = name.ToString();
            if (json.TryGetPropertyValue("in", out JsonNode inNode))
            {
                string val = inNode.ToString().ToLower();
                if (val == "query")
                    _inLocation = SwagSecurityDefinitionApiKeyInLocation.Query;
                else if (val == "header")
                    _inLocation = SwagSecurityDefinitionApiKeyInLocation.Header;
                else
                    _inLocation = SwagSecurityDefinitionApiKeyInLocation.NotDefined;
            }
        }

        public SwagSecurityDefinitionApiKeyInLocation InLocation
        {
            get => _inLocation;
            set => _inLocation = value;
        }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        private string InLocationToString(SwagSecurityDefinitionApiKeyInLocation location)
        {
            return location switch
            {
                SwagSecurityDefinitionApiKeyInLocation.Query => "query",
                SwagSecurityDefinitionApiKeyInLocation.Header => "header",
                _ => ""
            };
        }
    }
}
