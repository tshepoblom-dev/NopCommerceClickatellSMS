using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core.Logging;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.Clickatell.EventConsumers
{
    public class OrderRefundedEventConsumer : IConsumer<OrderRefundedEvent>
    {

        private readonly IPluginService _pluginFinder;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        public OrderRefundedEventConsumer(IPluginService pluginService, ILogger logger)
        {
            _pluginFinder = pluginService;
            _logger = logger;
        }
        public async void HandleEvent(OrderRefundedEvent eventMessage)
        {
            try
            {
                //check that plugin is installed
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName<ClickatellPlugin>("Misc.Clickatell", LoadPluginsMode.InstalledOnly);

                var plugin = pluginDescriptor?.Instance<ClickatellPlugin>();
                var order = eventMessage.Order;
                //order note
                if (order != null)
                {
                    var customer = _customerService.GetCustomerById(order.CustomerId);
                    var phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
                    phone = OrderPlacedEventConsumer.FormatNumber(phone);
                    var success = await plugin?.SendSms($"Dear valued customer, your refund request for order #{order.Id} of R{order.OrderTotal} been processed - buybloem.com", phone, "sms");
                    if (success)
                    {
                        order.OrderNotes.Add(new OrderNote()
                        {
                            Note = $"\"Order Refunded\" SMS alert {customer.Email} has been sent",
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        _orderService.UpdateOrder(order);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Clickatell.Error.OrderRefundedEvent:" + ex.Message);
            }
        }

    }
}
