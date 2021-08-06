using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Naveego.Sdk.Logging;
using PluginSmartSheets.API.Utility;
using PluginSmartSheets.Helper;
using Smartsheet.Api;
using Smartsheet.Api.Internal.Http;
using Smartsheet.Api.Models;
using HttpClient = System.Net.Http.HttpClient;
using HttpMethod = System.Net.Http.HttpMethod;

namespace PluginSmartSheets.API.Factory
{
    public class ApiClient : IApiClient
    {
        private IApiAuthenticator Authenticator { get; set; }

        //private static HttpClient Client { get; set; }
        private static SmartsheetClient Client { get; set; }
        private Settings Settings { get; set; }

        private const string ApiKeyParam = "hapikey";

        public ApiClient(SmartsheetClient client, Settings settings)
        {
            Authenticator = new ApiAuthenticator(client, settings);
            Client = client;
            Settings = settings;

        }

        public async Task<PaginatedResult<Sheet>> ListSheets()
        {
            Client.AccessToken = Settings.AccessToken;

            return Client.SheetResources.ListSheets(
                new List<SheetInclusion> {SheetInclusion.SHEET_VERSION}, null, null);
        }
        
        public async Task<Sheet> GetSheet(string sheetId)
        {
            Client.AccessToken = Settings.AccessToken;

            return Client.SheetResources.GetSheet(Int64.Parse(sheetId), null, null, null, null, null, null, null);
        }
        
        public async Task TestConnection()
        {
            Client.AccessToken = Settings.AccessToken;

            PaginatedResult<Sheet> sheets = Client.SheetResources.ListSheets(
                new List<SheetInclusion> {SheetInclusion.SHEET_VERSION}, null, null);
        }
    }
}