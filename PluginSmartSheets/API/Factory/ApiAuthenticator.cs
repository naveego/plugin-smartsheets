using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Naveego.Sdk.Logging;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PluginSmartSheets.DataContracts;
using PluginSmartSheets.Helper;
using Smartsheet.Api;

namespace PluginSmartSheets.API.Factory
{
    public class ApiAuthenticator: IApiAuthenticator
    {
        private SmartsheetClient Client { get; set; }
        private Settings Settings { get; set; }
        private string Token { get; set; }
        private DateTime ExpiresAt { get; set; }
        
        private const string AuthUrl = "https://api.hubapi.com/oauth/v1/token";
        
        public ApiAuthenticator(SmartsheetClient client, Settings settings)
        {
            Client = client;
            Settings = settings;
            ExpiresAt = DateTime.Now.AddDays(6);
            Token = Settings.RefreshToken;
        }

        public async Task<string> GetToken()
        {
            if (!string.IsNullOrWhiteSpace(Settings.ApiKey))
            {
                return Token;
            }
            
            // check if token is expired or will expire in 5 minutes or less
            if (DateTime.Compare(DateTime.Now.AddMinutes(5), ExpiresAt) >= 0)
            {
                return await GetNewToken();
            }
          
            return Token;
        }

        private async Task<string> GetNewToken()
        {
            try
            {
                IWebDriver driver = new ChromeDriver(@"C:\Dev\chromedriver\91\");
                
                driver.Url = $"https://app.smartsheet.com/b/authorize?client_id={Settings.ClientId}&response_type=code";


                try
                {
                    IWebElement loginEmail = driver.FindElement(By.Id("loginEmail"));
                    loginEmail.SendKeys("");

                    IWebElement loginContinue = driver.FindElement(By.Id("formControl"));
                    loginContinue.Click();

                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    var loginPassword =
                        wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("loginPassword")));
                    
                    loginPassword.SendKeys("");
                    
                    loginContinue =
                        wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("formControl")));
                    loginContinue.Click();
                    
                    var loginAllow = 
                        wait.Until(
                            SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("clsJspButton")))
                                .FindElement(By.Id("formControl"));
                    loginAllow.Click();
                }
                catch (Exception e)
                {
                    var debug = e.Message;
                    //Already logged in - just press allow
                }
            }
            catch (Exception e)
            {
                var debug = e.Message;
            }
            return "test";

            // try
            // {
            //     var formData = new List<KeyValuePair<string, string>>
            //     {
            //         new KeyValuePair<string, string>("grant_type", "refresh_token"),
            //         new KeyValuePair<string, string>("client_id", Settings.ClientId),
            //         new KeyValuePair<string, string>("client_secret", Settings.ClientSecret),
            //         new KeyValuePair<string, string>("refresh_token", Settings.RefreshToken),
            //         new KeyValuePair<string, string>("redirect_uri", Settings.RedirectUri)
            //     };
            //
            //     var body = new FormUrlEncodedContent(formData);
            //         
            //     var client = Client;
            //     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //
            //     var response = await Client.PostAsync(AuthUrl, body);
            //     response.EnsureSuccessStatusCode();
            //         
            //     var content = JsonConvert.DeserializeObject<TokenResponse>(await response.Content.ReadAsStringAsync());
            //         
            //     // update expiration and saved token
            //     ExpiresAt = DateTime.Now.AddSeconds(content.ExpiresIn);
            //     Token = content.AccessToken;
            //
            //     return Token;
            // }
            // catch (Exception e)
            // {
            //     Logger.Error(e, e.Message);
            //     throw;
            // }
        }
    }
}