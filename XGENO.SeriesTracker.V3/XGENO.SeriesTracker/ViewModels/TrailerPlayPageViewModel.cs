using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Exceptions;
using XGENO.Framework.Helpers;
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Helpers;
using XGENO.SeriesTracker.Services;


namespace XGENO.SeriesTracker.ViewModels
{
    public class TrailerPlayPageViewModel : AppViewModelBase
    {
        private string _videoPlayURL;
        public string VideoPlayURL
        {
            get { return _videoPlayURL; }
            set { Set(ref _videoPlayURL, value); }
        }

        public TrailerPlayPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            this.VideoPlayURL = parameter.ToString();
        }
    }
}
