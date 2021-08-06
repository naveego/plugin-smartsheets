using PluginSmartSheets.Helper;

namespace PluginSmartSheets.API.Factory
{
    public interface IApiClientFactory
    {
        IApiClient CreateApiClient(Settings settings);
    }
}