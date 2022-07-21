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
            var IgnoreRowsWithoutKeyValues = await apiClient.GetIgnoreRowsWithoutKeyValues();
            var sheetId = schema.Id;

            if (!string.IsNullOrWhiteSpace(schema.Query))
            {
                var allSheets = await apiClient.ListSheets();
                var sheetNames = schema.Query.Split(',');

                foreach (var sheetName in sheetNames)
                {
                    var unsafeSheet = allSheets.Data.FirstOrDefault(x => x.Name == sheetName);
                    if (unsafeSheet != null)
                    {
                        var sheet = await apiClient.GetSheet(unsafeSheet.Id.ToString());
                        foreach (Row row in sheet.Rows)
                        {
                            var validRow = true;
                            var recordMap = new Dictionary<string, object>();
                            foreach (var property in schema.Properties)
                            {
                                try
                                {
                                    var currentColumn = sheet.Columns.First(x => x.Title == property.Name);
                                    Cell currCell = row.Cells.ToList().Find(x => x.ColumnId.ToString() == currentColumn.Id.ToString());

                                    //if GetPropertyType evaluates to a string type, convert object result to string
                                    //purpose is to handle some abstracted column types in SmartSheets which are sometimes obj, sometimes string

                                    if (IgnoreRowsWithoutKeyValues && property.IsKey == true &&
                                        (currCell?.Value == null || currCell?.Value.ToString() == ""))
                                    {
                                        validRow = false;
                                    }
                                    
                                    recordMap[property.Id] =
                                        (Utility.GetType.GetPropertyType(currCell?.ColumnType.ToString()) ==
                                         PropertyType.String)
                                            ? recordMap[property.Id] = currCell?
                                                .Value.ToString()
                                            : recordMap[property.Id] = currCell?
                                                .Value;
                                }
                                catch (Exception e)
                                {
                                    recordMap[property.Id] = "";
                                }
                            }

                            if (validRow)
                            {
                                yield return new Record
                                {
                                    Action = Record.Types.Action.Upsert,
                                    DataJson = JsonConvert.SerializeObject(recordMap)
                                };
                            }
                        }
                    }
                }
            }
            else
            {
                var sheet = await apiClient.GetSheet(sheetId);

                foreach (Row row in sheet.Rows)
                {
                    var validRow = true;
                    var recordMap = new Dictionary<string, object>();
                    foreach (var property in schema.Properties)
                    {
                        try
                        {
                            Cell currCell = row.Cells.ToList().Find(x => x.ColumnId.ToString() == property.Id);

                            //if GetPropertyType evaluates to a string type, convert object result to string
                            //purpose is to handle some abstracted column types in SmartSheets which are sometimes obj, sometimes string
                            if (IgnoreRowsWithoutKeyValues && property.IsKey == true &&
                                (currCell?.Value == null || currCell?.Value.ToString() == ""))
                            {
                                validRow = false;
                            }
                            recordMap[property.Id] =
                                (Utility.GetType.GetPropertyType(currCell.ColumnType.ToString()) == PropertyType.String)
                                    ? recordMap[property.Id] = currCell
                                        .Value.ToString()
                                    : recordMap[property.Id] = currCell
                                        .Value;
                        }
                        catch (Exception e)
                        {
                            recordMap[property.Id] = "";
                        }
                    }

                    if (validRow)
                    {
                        yield return new Record
                        {
                            Action = Record.Types.Action.Upsert,
                            DataJson = JsonConvert.SerializeObject(recordMap)
                        };
                    }
                    
                }
            }
        }
    }
}