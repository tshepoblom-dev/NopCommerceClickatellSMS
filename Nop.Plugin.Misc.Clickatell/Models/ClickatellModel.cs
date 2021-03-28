using System;
using System.Collections.Generic;
using System.Text;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.Clickatell.Models
{
    public class ClickatellModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Clickatell.Fields.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Clickatell.Fields.ApiKey")]
        public string ApiKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Clickatell.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Misc.Clickatell.Fields.TestMessage")]
        public string TestMessage { get; set; }
    }
}
