using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using XGENO.Framework.Extensions;
using XGENO.Framework.MVVM;

namespace XGENO.SeriesTracker.DataModels
{
    public class Configuration
    {
        public bool LocalInitialize { get; set; }

        public ConfigurationImages images { get; set; }
        public List<string> change_keys { get; set; }
    }

    public class ConfigurationImages
    {
        public string base_url { get; set; }
        public string secure_base_url { get; set; }
        public List<string> poster_sizes { get; set; }
        public List<string> backdrop_sizes { get; set; }
        public List<string> profile_sizes { get; set; }
        public List<string> logo_sizes { get; set; }
    }


    public class Profile
    {
        public string file_path { get; set; }
    }

    public class CastImage
    {
        public int id { get; set; }
        public List<Profile> profiles { get; set; }
    }

}


