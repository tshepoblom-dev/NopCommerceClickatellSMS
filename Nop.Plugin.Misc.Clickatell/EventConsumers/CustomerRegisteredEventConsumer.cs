using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Clickatell.EventConsumers
{
    public class CustomerRegisteredEventConsumer : IConsumer<CustomerRegisteredEvent>
    {
        private readonly IPluginService _pluginService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        public CustomerRegisteredEventConsumer(IPluginService pluginService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILogger logger)
        {
            _pluginService = pluginService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
        }
        public async void HandleEvent(CustomerRegisteredEvent eventMessage)
        {
            try
            {
                //check that plugin is installed
                var pluginDescriptor = _pluginService.GetPluginDescriptorBySystemName<ClickatellPlugin>("Misc.Clickatell", LoadPluginsMode.InstalledOnly);
                var plugin = pluginDescriptor?.Instance<ClickatellPlugin>();

                var customer = _customerService.GetCustomerById(eventMessage.Customer.Id);
                var phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
                phone = FormatNumber(phone);
                var success = await plugin?.SendSms($"Hi {customer.Username}, welcome to BuyBloem.com. Your account has been succesfully registered", phone, "sms");
            }
            catch (Exception ex)
            {
                _logger.Error("Clickatell.Error.CustomerRegisterEvent:" + ex.Message);
            }
        }
        /// <summary>
        /// Replaces the first '0' character in a phone number with '27'
        /// Is static so as to be shareable across classes
        /// </summary>
        /// <param name="number">The number to action</param>
        /// <returns></returns>
        public string FormatNumber(string number)
        {
            char[] numArr = number.ToCharArray();
            if (numArr.Length == 10 && numArr[0] == '0')
            {
                number = number.Remove(0, 1);
                number = number.Insert(0, "27");
            }
            return number;
        }
    }
}
