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
                var sheetId = schema.Id;

                if (!string.IsNullOrWhiteSpace(schema.Query))
                {
                    sheetId = schema.Query;
                }
                
                var sheet = await apiClient.GetSheet(sheetId);
                
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

                // get sample and count
                yield return await AddSampleAndCount(apiClient, schema, sampleSize);
            }
        }
    }
}