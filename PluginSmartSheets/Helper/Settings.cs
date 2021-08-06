using System;

namespace PluginSmartSheets.Helper
{
    public class Settings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RefreshToken { get; set; }
        public string RedirectUri { get; set; }
        public string ApiKey { get; set; }
        
        public string AccessToken { get; set; }

        /// <summary>
        /// Validates the settings input object
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Validate()
        {
            // if (string.IsNullOrWhiteSpace(ApiKey))
            // {
            if (String.IsNullOrEmpty(AccessToken))
            {
                throw new Exception("the Access Token property must be set");
            }
            //
            //     if (String.IsNullOrEmpty(ClientSecret))
            //     {
            //         throw new Exception("the ClientSecret property must be set");
            //     }
            //
            //     if (String.IsNullOrEmpty(RefreshToken))
            //     {
            //         throw new Exception("the RefreshToken property must be set");
            //     }
            //     
            //     if (String.IsNullOrEmpty(RedirectUri))
            //     {
            //         throw new Exception("the RedirectUri property must be set");
            //     }
            // }
        }
    }
}