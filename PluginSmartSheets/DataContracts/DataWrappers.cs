using System.Collections.Generic;
using Newtonsoft.Json;

namespace PluginSmartSheets.DataContracts
{
    public class ObjectResponseWrapper
    {
        [JsonProperty("results")]
        public List<ObjectResponse> Results { get; set; }
        
        [JsonProperty("paging")]
        public PagingResponse Paging { get; set; }
    }

    public class ObjectResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }
    }


    public class PropertyResponseWrapper
    {
        [JsonProperty("results")]
        public List<PropertyResponse> Results { get; set; }
        
        [JsonProperty("paging")]
        public PagingResponse Paging { get; set; }
    }

    public class PropertyResponse
    {
        [JsonProperty("name")]
        public string Id { get; set; }
        
        [JsonProperty("label")]
        public string Name { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("hasUniqueValue")]
        public bool IsKey { get; set; }
        
        [JsonProperty("calculated")]
        public bool Calculated { get; set; }
        
        [JsonProperty("type")]
        public string Type { get; set; }
        
        [JsonProperty("modificationMetadata")]
        public ModificationMetaData ModificationMetaData { get; set; }
    }

    public class PagingResponse
    {
        [JsonProperty("next")]
        public NextResponse Next { get; set; }
    }

    public class NextResponse
    {
        [JsonProperty("after")]
        public string After { get; set; }
        
        [JsonProperty("link")]
        public string Link { get; set; }
    }
}