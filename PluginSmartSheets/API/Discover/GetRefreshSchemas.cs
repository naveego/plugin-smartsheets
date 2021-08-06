using System.Collections.Generic;
using Google.Protobuf.Collections;
using Naveego.Sdk.Plugins;
using PluginSmartSheets.API.Utility;
using PluginSmartSheets.API.Factory;
using Smartsheet.Api.Models;

namespace PluginSmartSheets.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetRefreshSchemas(IApiClient apiClient,
            RepeatedField<Schema> refreshSchemas, int sampleSize = 5)
        {
            foreach (var schema in refreshSchemas)
            {
                var sheet = await apiClient.GetSheet(schema.Id);
                
                foreach (Column col in sheet.Columns)
                {
                    var property = new Property
                    {
                        Id = col.Id.ToString(),
                        Name = col.Title,
                        IsKey = false,
                        IsNullable = true,
                        Type = Utility.GetType.GetPropertyType(col.Type.ToString()),
                        TypeAtSource = col.Type.ToString(),
                    };
                    
                    schema?.Properties.Add(property);
                }

                var refreshSchema = schema;
                
                // get sample and count
                yield return await AddSampleAndCount(apiClient, refreshSchema, sampleSize);
            }
        }
    }
}