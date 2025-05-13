using System;
using Newtonsoft.Json.Linq;

namespace MVCFramework.Patches
{
    
    public static class JsonValueExtensions
    {
        
        public static JToken GetItem(this JToken token, int index)
        {
            if (token is JArray array)
            {
                return array[index];
            }
            throw new InvalidOperationException("JToken is not an array; cannot use indexer.");
        }

        
        public static string ToJSON(this JToken token)
        {
            
            return token.ToString();
        }

        
        public static int Count(this JToken token)
        {
            if (token is JArray array)
            {
                return array.Count;
            }
            if (token is JObject obj)
            {
                return obj.Count;
            }
            throw new InvalidOperationException("JToken does not have a count (not an array or object).");
        }
    }
}
