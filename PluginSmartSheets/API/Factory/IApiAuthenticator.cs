using System.Threading.Tasks;

namespace PluginSmartSheets.API.Factory
{
    public interface IApiAuthenticator
    {
        Task<string> GetToken();
    }
}