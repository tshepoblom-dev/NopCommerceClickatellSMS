using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Newtonsoft.Json;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.Clickatell.EventConsumers
{
    public class OrderPaidEventConsumer : IConsumer<OrderPaidEvent>
    {
        #region Fields

        private readonly ClickatellSettings _clickatellSettings;
        private readonly IPluginService _pluginFinder;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IVendorService _vendorService;
        private readonly IAddressService _addressService;
        private readonly ILogger _logger;
        #endregion

        #region Ctor

        public OrderPaidEventConsumer(IPluginService pluginFinder,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            IVendorService vendorService,
            IAddressService addressService,
            ILogger logger,
            ClickatellSettings clickatellSettings)
        {
            this._pluginFinder = pluginFinder;
            this._customerService = customerService;
            this._genericAttributeService = genericAttributeService;
            this._orderService = orderService;
            this._vendorService = vendorService;
            this._addressService = addressService;
            this._clickatellSettings = clickatellSettings;
            _logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(OrderPaidEvent eventMessage)
        {
            try
            {
                //check that plugin is installed
                var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName<ClickatellPlugin>("Misc.Clickatell", LoadPluginsMode.InstalledOnly);

                var plugin = pluginDescriptor?.Instance<ClickatellPlugin>();
                var order = eventMessage.Order;

                if (order != null)
                {
                    //Send SMS to customer
                    SmsToCustomer(plugin, order);

                    //Send SMS to Vendor
                    SmsToVendors(plugin, order);

                    SmsToAdmin(plugin, order);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Clickatell.Error.OrderPaidEvent:" + ex.Message);
            }
        }

        async void SmsToAdmin(ClickatellPlugin plugin, Order order, ClickatellSettings settings = null)
        {
            var adminNumber = FormatNumber(_clickatellSettings.PhoneNumber);
            var success = await plugin?.SendSms($"Hi Admin, new order #{order.Id} R{order.OrderTotal} has been PAID", adminNumber);
        }

        async void SmsToCustomer(ClickatellPlugin plugin, Order order)
        {
            var customer = _customerService.GetCustomerById(order.CustomerId);
            var phone = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute);
            phone = FormatNumber(phone);
            var success = await plugin?.SendSms($"Dear valued shopper, your order #{order.Id} of R{order.OrderTotal} on BuyBloem.com has been PAID.", phone, "sms");
            if (success)
            {
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = $"\"Order PAID\" SMS alert {customer.Email} has been sent",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
            }
        }
        async void SmsToVendors(ClickatellPlugin plugin, Order order)
        {
            var vendorDict = GetVendorPhoneAndOrderItems(order);
            List<object> messages = new List<object>();

            foreach (var dictItem in vendorDict)
            {
                messages.Add(new { channel = "sms", content = $"Hello vendor, Order #{order.Id} has been PAID. Items: {dictItem.Value}", to = dictItem.Key });
            }
            var jsonBody = JsonConvert.SerializeObject(messages);
            var success = await plugin?.SendSms(jsonBody);
            if (success)
            {
                order.OrderNotes.Add(new OrderNote()
                {
                    Note = $"\"Order PAID\" SMS alert to vendors has been sent",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                _orderService.UpdateOrder(order);
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
        /// <summary>
        /// scans for vendorIds in order
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<int> GetVendorsFromOrder(Order order)
        {
            var orderItems = order.OrderItems.ToList();
            var vendorIds = new List<int>();
            orderItems.ForEach((orderItem) => { vendorIds.Add(orderItem.Product.VendorId); });
            return vendorIds.Distinct().ToList();
        }

        /// <summary>
        /// retrieves vendor phone numbers and list of items bought from them
        /// </summary>
        /// <param name="order">the order processed</param>
        /// <returns></returns>
        Dictionary<string, string> GetVendorPhoneAndOrderItems(Order order)
        {
            var dictionary = new Dictionary<string, string>();
            var vendorIds = GetVendorsFromOrder(order);
            vendorIds.ForEach((vendorId) =>
            {
                var vendor = _vendorService.GetVendorById(vendorId);
                var vendorAddress = _addressService.GetAddressById(vendor.AddressId);
                var vendorPhone = FormatNumber(vendorAddress.PhoneNumber);

                var items = order.OrderItems.ToList();
                var vendorItems = items.Where(x => x.Product.VendorId == vendorId).ToList();
                var itemsTotal = vendorItems.Select(x => x.PriceInclTax).Sum();

                StringBuilder sb = new StringBuilder();
                vendorItems.ForEach(item => { sb.Append(item.Product.Name + ", "); });

                dictionary.Add(vendorPhone, sb.ToString());
            });
            return dictionary;
        }
        #endregion
    }
}
