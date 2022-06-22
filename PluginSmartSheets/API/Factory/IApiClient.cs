using System.Net.Http;
using System.Threading.Tasks;
using Smartsheet.Api;
using Smartsheet.Api.Models;

namespace PluginSmartSheets.API.Factory
{
    public interface IApiClient
    {
        Task TestConnection();

        Task<PaginatedResult<Sheet>> ListSheets();

        Task<Sheet> GetSheet(string sheetId);
        Task<bool> GetIgnoreRowsWithoutKeyValues();
    }
}