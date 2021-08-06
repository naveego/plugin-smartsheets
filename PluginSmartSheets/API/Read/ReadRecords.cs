using System;
using System.Collections.Generic;
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

            var columnIdMap = new Dictionary<object, string>();
            
            foreach (Column col in sheet.Columns)
            {
                columnIdMap.Add(col.Id, col.Title);
            }
            
            foreach (Row row in sheet.Rows)
            {
                var recordMap = new Dictionary<string, object>();

                foreach (Cell cell in row.Cells)
                {
                    try
                    {
                        recordMap[columnIdMap[cell.ColumnId]] = cell.DisplayValue;
                    }
                    catch (Exception e)
                    {
                        recordMap[columnIdMap[cell.ColumnId]] = "";
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