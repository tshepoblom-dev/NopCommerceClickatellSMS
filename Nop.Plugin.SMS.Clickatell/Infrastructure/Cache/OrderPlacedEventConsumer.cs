using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;
using Nop.Services.Plugins;

namespace Nop.Plugin.SMS.Clickatell.Infrastructure.Cache
{
    public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        #region Fields

        private readonly IPluginService _pluginFinder;

        #endregion

        #region Ctor

        public OrderPlacedEventConsumer(IPluginService pluginFinder)
        {
            this._pluginFinder = pluginFinder;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="eventMessage">The event message.</param>
        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            //check that plugin is installed
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName<ClickatellSmsProvider>("Mobile.SMS.Clickatell", LoadPluginsMode.InstalledOnly);

            var plugin = pluginDescriptor?.Instance<ClickatellSmsProvider>();

            plugin?.SendSms(string.Empty, eventMessage.Order.Id);
        }

        #endregion
    }
}
