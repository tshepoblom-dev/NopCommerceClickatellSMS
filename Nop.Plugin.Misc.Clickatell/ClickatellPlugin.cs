using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using RestSharp;

namespace Nop.Plugin.Misc.Clickatell
{
    public class ClickatellPlugin : BasePlugin, IMiscPlugin
    {

        #region Fields

        private readonly ClickatellSettings _clickatellSettings;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;

        #endregion


        public ClickatellPlugin(ClickatellSettings clickatellSettings,
            ILogger logger,
            IOrderService orderService,
            ISettingService settingService,
            IWebHelper webHelper,
            IWorkContext workContext,
            ILocalizationService localizationService)
        {
            this._clickatellSettings = clickatellSettings;
            this._logger = logger;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._localizationService = localizationService;
        }


        #region Methods
        /// Send SMS 
        /// </summary>
        /// <param name="message">Text</param>
        /// <param name="to">recipient number</param>
        /// <param name="channel">channel wherewith to send message, either SMS or WhatsApp</param>
        /// <param name="settings">Clickatel config setting</param>
        /// <returns>True if SMS was successfully sent; otherwise false</returns>
        public async Task<bool> SendSms(string message, string to, string channel = "sms", ClickatellSettings settings = null)
        {
            try
            {
                var clickatellSettings = settings ?? _clickatellSettings;
                if (!clickatellSettings.Enabled)
                    return false;

                var client = new RestClient("https://platform.clickatell.com/v1/message");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", clickatellSettings.ApiKey);
                request.AddHeader("Content-Type", "application/json");
                List<object> messages = new List<object>();
                messages.Add(new { channel = channel, content = message, to = to });
                request.AddParameter("application/json", JsonConvert.SerializeObject(messages), ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.Debug("Clickatell SMS OK: " + response.Content);
                    return true;
                }
                else
                {
                    _logger.Debug(response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Clickatell SMS Error: ", ex);
                return false;
            }
        }
        public async Task<bool> SendSms(string jsonBody, ClickatellSettings settings = null)
        {
            try
            {
                var clickatellSettings = settings ?? _clickatellSettings;
                if (!clickatellSettings.Enabled)
                    return false;

                var client = new RestClient("https://platform.clickatell.com/v1/message");
                client.Timeout = -1;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Authorization", clickatellSettings.ApiKey);
                request.AddHeader("Content-Type", "application/json");
                List<object> messages = new List<object>();
                request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
                IRestResponse response = await client.ExecuteAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    return true;
                else
                {
                    _logger.Debug(response.Content);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Clickatell SMS to Vendors Error", ex);
                return false;
            }
        }
      
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Clickatell/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new ClickatellSettings());

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.ApiKey", "API KEY");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.ApiKey.Hint", "API KEY HINT");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.ApiId", "API ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.ApiId.Hint", "Specify Clickatell API ID.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Enabled", "Enabled");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Enabled.Hint", "Check to enable SMS provider.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Password", "Password");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Password.Hint", "Specify Clickatell API password.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.PhoneNumber", "Phone number");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.PhoneNumber.Hint", "Enter your phone number.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.TestMessage", "Message text");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.TestMessage.Hint", "Enter text of the test message.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Username", "Username");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Username.Hint", "Specify Clickatell API username.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.SendTest", "Send");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.SendTest.Hint", "Send test message");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.TestFailed", "Test message sending failed");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.Clickatell.TestSuccess", "Test message was sent");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<ClickatellSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.ApiId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.ApiId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Enabled");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Enabled.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Password");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Password.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.PhoneNumber");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.PhoneNumber.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.TestMessage");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.TestMessage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Username");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.Fields.Username.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.SendTest");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.SendTest.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.TestFailed");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.Clickatell.TestSuccess");

            base.Uninstall();
        }

        #endregion
    }
}
