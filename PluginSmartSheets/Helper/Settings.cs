using System;

namespace PluginSmartSheets.Helper
{
    public class Settings
    {
        public string ClientId { get; set; }
        public string RefreshToken { get; set; }
        public string ApiKey { get; set; }
        public string AccessToken { get; set; }
        public bool IgnoreRowsWithoutKeyValues { get; set; }

        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            if (String.IsNullOrEmpty(AccessToken))
            {
                throw new Exception("the Access Token property must be set");
            }
        }
    }
}