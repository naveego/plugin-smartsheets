using System.Collections.Generic;
using System.Linq;
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
                    var runningPropertiesNames = new List<string>() { };
                    var allSheets = await apiClient.ListSheets();

                    var sheetNames = schema.Query.Split(',');
                    foreach (var sheetName in sheetNames)
                    {
                        var unsafeSheet = allSheets.Data.FirstOrDefault(x => x.Name == sheetName);
                        var sheet = await apiClient.GetSheet(unsafeSheet.Id.ToString());

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
                            if (!runningPropertiesNames.Contains(property.Name))
                            {
                                schema?.Properties.Add(property);
                                runningPropertiesNames.Add(property.Name);
                            }
                        }
                    }

                    // get sample and count
                    yield return await AddSampleAndCount(apiClient, schema, sampleSize);
                }
                else
                {

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
}