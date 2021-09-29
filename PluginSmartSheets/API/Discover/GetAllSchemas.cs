using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginSmartSheets.API.Utility;
using PluginSmartSheets.DataContracts;
using PluginSmartSheets.API.Factory;
using PluginSmartSheets.Helper;
using Smartsheet.Api.Models;

namespace PluginSmartSheets.API.Discover
{
    public static partial class Discover
    {
        public static async IAsyncEnumerable<Schema> GetAllSchemas(IApiClient apiClient, Settings settings,
            int sampleSize = 5)
        {
            PaginatedResult<Sheet> sheets = await apiClient.ListSheets();

            foreach (var sheetData in sheets.Data)
            {
                var schema = new Schema
                {
                    Id = sheetData.Id.ToString(),
                    Name = sheetData.Name,
                    Description = "",
                    PublisherMetaJson = ""
                };

                var sheet = await apiClient.GetSheet(schema.Id);

                foreach (var col in sheet.Columns)
                {
                    var property = new Property
                    {
                        Id = col.Id.ToString(),
                        Name = col.Title,
                        IsKey = false,
                        IsNullable = true,
                        Type = Utility.GetType.GetPropertyType(col.Type.ToString()),
                        TypeAtSource = col.Type.ToString()
                    };

                    schema?.Properties.Add(property);
                }

                yield return await AddSampleAndCount(apiClient, schema, sampleSize);
            }
        }

        private static async Task<Schema> AddSampleAndCount(IApiClient apiClient, Schema schema,
            int sampleSize)
        {
            // add sample and count
            var records = Read.Read.ReadRecordsAsync(apiClient, schema).Take(sampleSize);
            schema.Sample.AddRange(await records.ToListAsync());

            return schema;
        }
    }
}