using System.Net.Http;
using PluginSmartSheets.Helper;
using Smartsheet.Api;

namespace PluginSmartSheets.API.Factory
{
    public class ApiClientFactory: IApiClientFactory
    {
        private SmartsheetClient Client { get; set; }

        public ApiClientFactory(SmartsheetClient client)
        {
            Client = client;
        }

        public IApiClient CreateApiClient(Settings settings)
        {
            return new ApiClient(Client, settings);
        }
    }
}