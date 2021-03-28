using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.Clickatell
{
    public class ClickatellSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the value indicting whether this SMS provider is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the Clickatell API Key
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the store owner phone number
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Gets or sets the store owner phone number
        /// </summary>
        public string TestMessage { get; set; }
    }
}
