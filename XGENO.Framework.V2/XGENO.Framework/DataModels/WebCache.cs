using System;

namespace XGENO.Framework.DataModels
{
    public class WebCache
    {
        public string Content { get; set; }
        public DateTime StartDate { get; set; } = new DateTime(2000, 1, 1);

        public WebCache()
        {
        }
    }
}
