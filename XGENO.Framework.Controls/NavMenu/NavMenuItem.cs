using System;

namespace XGENO.Framework.Controls
{
    public class NavMenuItem
    {
        public string Label { get; set; }
        public string Symbol { get; set; }

        public string ItemType { get; set; } = "Page";

        public Type DestPage { get; set; }
        public string Arguments { get; set; }
    }
}
