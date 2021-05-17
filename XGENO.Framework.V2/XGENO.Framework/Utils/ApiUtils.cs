using System;
using Windows.Foundation.Metadata;

namespace XGENO.Framework.Utils
{
    public static class ApiUtils
    {
        static bool? isHardwareButtonsApiPresent;
        public static bool IsHardwareButtonsApiPresent => isHardwareButtonsApiPresent ?? (isHardwareButtonsApiPresent = ApiInformation.IsTypePresent(@"Windows.Phone.UI.Input.HardwareButtons")).Value;
    }
}