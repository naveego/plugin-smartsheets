using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginSmartSheets.API.Utility;
using PluginSmartSheets.API.Factory;
using Smartsheet.Api.Models;

namespace PluginSmartSheets.API.Read
{
    public static partial class Read
    {
        public static async IAsyncEnumerable<Record> ReadRecordsAsync(IApiClient apiClient, Schema schema)
        {
            var sheet = await apiClient.GetSheet(schema.Id);
            
            foreach (Row row in sheet.Rows)
            {
                var recordMap = new Dictionary<string, object>();
                foreach (var property in schema.Properties)
                {
                    
                    try
                    {
                        recordMap[property.Id] = row.Cells.ToList().Find(x => x.ColumnId.ToString() == property.Id).Value;
                        
                    }
                    catch (Exception e)
                    {
                        recordMap[property.Id] = "";
                    }
                }
                yield return new Record
                {
                    Action = Record.Types.Action.Upsert,
                    DataJson = JsonConvert.SerializeObject(recordMap)
                };       
            }
        }
    }
}