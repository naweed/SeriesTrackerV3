using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker.ViewModels
{
    public abstract class AppViewModelBase : ViewModelBase
    {
        public RestService _appService;

        public AppSettings _appSettings;

        private string _loadingText;
        public string LoadingText
        {
            get { return _loadingText; }
            set
            {
                Set(ref _loadingText, value);
                Views.AppShell.SetBusyIndicator(_loadingText);
            }
        }

        private bool _dataLoaded;
        public bool DataLoaded
        {
            get { return _dataLoaded; }
            set
            {
                Set(ref _dataLoaded, value);
            }
        }

        private bool _isFlyoutClosed;
        public bool IsFlyoutClosed
        {
            get { return _isFlyoutClosed; }
            set
            {
                Set(ref _isFlyoutClosed, value);

                if (value)
                    IsFlyoutClosed = false;
            }
        }

        private bool _isAppPurchased;
        public bool IsAppPurchased
        {
            get { return _isAppPurchased; }
            set
            {
                Set(ref _isAppPurchased, value);
            }
        }


        public AppViewModelBase()
        {
            _appService = new RestService();
            _appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;

            IsAppPurchased = _appSettings.ApplicationPurchased;
        }

    }
}
