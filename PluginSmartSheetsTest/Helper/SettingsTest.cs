using System;
using PluginSmartSheets.Helper;
using Xunit;

namespace PluginHubspotTest.Helper
{
    public class SettingsTest
    {
        [Fact]
        public void ValidateValidTest()
        {
            // setup
            var settings = new Settings
            {
               AccessToken = "TOKEN"
            };

            // act
            settings.Validate();

            // assert
        }

        [Fact]
        public void ValidateNoClientIdTest()
        {
            // setup
            var settings = new Settings
            {
                ClientId = null,
            };

            // act
            Exception e = Assert.Throws<Exception>(() => settings.Validate());

            // assert
            Assert.Contains("The Client ID property must be set", e.Message);
        }
    }
}