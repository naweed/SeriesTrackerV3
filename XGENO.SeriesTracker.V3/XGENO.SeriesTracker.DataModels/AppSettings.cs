using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGENO.Framework.Enums;
using XGENO.Framework.Helpers;
using XGENO.Framework.MVVM;
using XGENO.Framework.Services;

namespace XGENO.SeriesTracker.DataModels
{
    public class AppSettings : BindableBase
    {
        #region Static Properties

        private static int APICounter = -1;

        //public static string[] APIKeys = new string[] { "2813a03c71413f9a07a4f022c749218df1002bf045bea2fd41fd5fa3ab1f2b98", "7343d6db9d8043e6a6538b2a15bda6c2f6414fd054d8d28551f115261ce67a0e", "8bbaabe8d8015dd5dcaa1311cb5457aec472dce83c199d8d707d317a5dce2c7e", "3abc555c5e93fcb7eeb2ba8d8d964feea15e46484997dd7c5c644719563507c1", "278a17283e08ec37b803ca3637a03fc324f589705f471c8baf0aa7be1c2be80e", "b4c1b77557714bea3164d59aea5189debfe6b0f38a5839d6c542e0cb0e12ee47" };

        //public static string APIKey
        //{
        //    get
        //    {
        //        return APIKeys[++APICounter % APIKeys.Count()];
        //    }
        //}

        public static string TraktSyncClientID { get; set; } = "bc73b98c00484d817846ff73f59c7beec58930e417e0ac67dd6a7fe597d118a4";
        public static string TraktSyncClientSecret { get; set; } = "70340923c72d4293f540155e6bbf9e2ca6c057eb801336713246aa47dad9211f";

        public static uint LiveTilesDuration { get; set; } = 60;
        public static uint TraktUpdateDuration { get; set; } = 91;

        public static bool ArchivedShowsInView;

        public static List<TrackingShow> TrackingList { get; set; }
        public static List<TrackedEpisode> TrackedEpisodeList { get; set; }
        public static List<NotifiedEpisode> NotifiedEpisodeList { get; set; }

        #endregion

        #region Properties

        public AppLicenseService AppLicenseInfo { get; set; }

        private bool _applicationPurchased;
        public bool ApplicationPurchased
        {
            get 
            { 
                return _applicationPurchased; 
            }
            set
            {
                _applicationPurchased = value;
                SettingStorageHelper.SetSetting<bool>("XGENO.SeriesTracker.ApplicationPurchased", _applicationPurchased, StorageStrategy.Roaming);
            }
        }

        private bool _liveTilesEnabled;
        public bool LiveTilesEnabled
        {
            get 
            { 
                return _liveTilesEnabled; 
            }
            set
            {
                SettingStorageHelper.SetSetting<bool>("XGENO.SeriesTracker.LiveTileEnabled", value, StorageStrategy.Roaming);
                _liveTilesEnabled = value;
            }
        }

        private bool _generalNotificationsEnabled;
        public bool GeneralNotificationsEnabled
        {
            get
            {
                return _generalNotificationsEnabled;
            }
            set
            {
                _generalNotificationsEnabled = value;
                SettingStorageHelper.SetSetting<bool>("XGENO.SeriesTracker.GeneralNotificationsEnabled", _generalNotificationsEnabled, StorageStrategy.Roaming);
            }
        }

        private bool _excludeNoUpcomingEpisodeShowsEnabled;
        public bool HideShowsWithNoUpcomingInfoEnabled
        {
            get
            {
                return _excludeNoUpcomingEpisodeShowsEnabled;
            }
            set
            {
                _excludeNoUpcomingEpisodeShowsEnabled = value;
                SettingStorageHelper.SetSetting<bool>("XGENO.SeriesTracker.HideShowsWithNoUpcomingInfoEnabled", _excludeNoUpcomingEpisodeShowsEnabled, StorageStrategy.Roaming);
            }
        }

        private bool _sortAlphabeticallyEnabled;
        public bool SortAlphabeticallyEnabled
        {
            get
            {
                return _sortAlphabeticallyEnabled;
            }
            set
            {
                _sortAlphabeticallyEnabled = value;
                SettingStorageHelper.SetSetting<bool>("XGENO.SeriesTracker.SortAlphabeticallyEnabled", _sortAlphabeticallyEnabled, StorageStrategy.Roaming);
            }
        }

        private bool _airedNotificationsEnabled;
        public bool AiredNotificationsEnabled
        {
            get
            {
                return _airedNotificationsEnabled;
            }
            set
            {
                _airedNotificationsEnabled = value;
                SettingStorageHelper.SetSetting<bool>("XGENO.SeriesTracker.AiredNotificationsEnabled", _airedNotificationsEnabled, StorageStrategy.Roaming);
            }
        }

        private bool _startPageIsWatchList;
        public bool StartPageIsWatchList
        {
            get
            {
                return _startPageIsWatchList;
            }
            set
            {
                _startPageIsWatchList = value;
                SettingStorageHelper.SetSetting<bool>("XGENO.SeriesTracker.StartPageIsWatchList", _startPageIsWatchList, StorageStrategy.Roaming);
            }
        }


        private DateTime _applicationStartDate;
        public DateTime ApplicationStartDate
        {
            get
            {
                return _applicationStartDate;
            }
            set
            {
                _applicationStartDate = value;
                SettingStorageHelper.SetSetting<string>("XGENO.SeriesTracker.ApplicationStartDate", _applicationStartDate.ToString(), StorageStrategy.Roaming);
            }
        }

        private DateTime _mainTaskLastSyncDate;
        public DateTime MainTaskLastSyncDate
        {
            get
            {
                return _mainTaskLastSyncDate;
            }
            set
            {
                _mainTaskLastSyncDate = value;
                SettingStorageHelper.SetSetting<string>("XGENO.SeriesTracker.MainTaskLastSyncDate", _mainTaskLastSyncDate.ToString(), StorageStrategy.Local);
            }
        }

        private DateTime _traktTaskLastSyncDate;
        public DateTime TraktTaskLastSyncDate
        {
            get
            {
                return _traktTaskLastSyncDate;
            }
            set
            {
                _traktTaskLastSyncDate = value;
                SettingStorageHelper.SetSetting<string>("XGENO.SeriesTracker.TraktTaskLastSyncDate", _traktTaskLastSyncDate.ToString(), StorageStrategy.Local);
            }
        }


        //Trakt Sync Data
        private string _traktAccessToken;
        public string TraktAccessToken
        {
            get 
            { 
                return _traktAccessToken; 
            }
            set
            {
                _traktAccessToken = value;
                SettingStorageHelper.SetSetting<string>("XGENO.SeriesTracker.TraktAccessToken", _traktAccessToken, StorageStrategy.Roaming);
            }
        }

        private string _traktRefreshToken;
        public string TraktRefreshToken
        {
            get 
            { 
                return _traktRefreshToken; 
            }
            set
            {
                _traktRefreshToken = value;
                SettingStorageHelper.SetSetting<string>("XGENO.SeriesTracker.TraktRefreshToken", _traktRefreshToken, StorageStrategy.Roaming);
            }
        }

        private DateTime _traktTokenExpiresOn;
        public DateTime TraktTokenExpiresOn
        {
            get 
            { 
                return _traktTokenExpiresOn; 
            }
            set
            {
                _traktTokenExpiresOn = value;
                SettingStorageHelper.SetSetting<string>("XGENO.SeriesTracker.TraktTokenExpiresOn", _traktTokenExpiresOn.ToString(), StorageStrategy.Roaming);
            }
        }

        #endregion

        public AppSettings()
        {
        }

        public async Task LoadSettings()
        {
            //Load Variables
            _liveTilesEnabled = SettingStorageHelper.GetSetting<bool>("XGENO.SeriesTracker.LiveTileEnabled", false, StorageStrategy.Roaming, true);
            _generalNotificationsEnabled = SettingStorageHelper.GetSetting<bool>("XGENO.SeriesTracker.GeneralNotificationsEnabled", true, StorageStrategy.Roaming, true);
            _excludeNoUpcomingEpisodeShowsEnabled = SettingStorageHelper.GetSetting<bool>("XGENO.SeriesTracker.HideShowsWithNoUpcomingInfoEnabled", false, StorageStrategy.Roaming, true);
            _sortAlphabeticallyEnabled = SettingStorageHelper.GetSetting<bool>("XGENO.SeriesTracker.SortAlphabeticallyEnabled", true, StorageStrategy.Roaming, true);
            _airedNotificationsEnabled = SettingStorageHelper.GetSetting<bool>("XGENO.SeriesTracker.AiredNotificationsEnabled", false, StorageStrategy.Roaming, true);
            _startPageIsWatchList = SettingStorageHelper.GetSetting<bool>("XGENO.SeriesTracker.StartPageIsWatchList", true, StorageStrategy.Roaming, true);
            _applicationPurchased = SettingStorageHelper.GetSetting<bool>("XGENO.SeriesTracker.ApplicationPurchased", false, StorageStrategy.Roaming, true);

            try
            {
                _applicationStartDate = Convert.ToDateTime(SettingStorageHelper.GetSetting<string>("XGENO.SeriesTracker.ApplicationStartDate", DateTime.Now.ToString("yyyy-MM-dd"), StorageStrategy.Roaming, true));
            }
            catch
            {
                ApplicationStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }


            try
            {
                _mainTaskLastSyncDate = Convert.ToDateTime(SettingStorageHelper.GetSetting<string>("XGENO.SeriesTracker.MainTaskLastSyncDate", "2000-01-01", StorageStrategy.Local, true));
            }
            catch
            {
                MainTaskLastSyncDate = new DateTime(2000, 1, 1);
            }

            try
            {
                _traktTaskLastSyncDate = Convert.ToDateTime(SettingStorageHelper.GetSetting<string>("XGENO.SeriesTracker.TraktTaskLastSyncDate", "2000-01-01", StorageStrategy.Local, true));
            }
            catch
            {
                TraktTaskLastSyncDate = new DateTime(2000, 1, 1);
            }

            await LoadTraktTokens();

            if(!ApplicationPurchased)
            {
                LiveTilesEnabled = true;
                ApplicationPurchased = true;
                AiredNotificationsEnabled = true;
            }

            await Task.CompletedTask;
        }

        public async Task LoadTraktTokens()
        {
            _traktTokenExpiresOn = Convert.ToDateTime(SettingStorageHelper.GetSetting<string>("XGENO.SeriesTracker.TraktTokenExpiresOn", "2000-01-01", StorageStrategy.Roaming, true));
            _traktAccessToken = SettingStorageHelper.GetSetting<string>("XGENO.SeriesTracker.TraktAccessToken", "", StorageStrategy.Roaming, true);
            _traktRefreshToken = SettingStorageHelper.GetSetting<string>("XGENO.SeriesTracker.TraktRefreshToken", "", StorageStrategy.Roaming, true);

            await Task.CompletedTask;
        }

        public async Task LoadAppLicense()
        {
            //Load AppLicense
            if (!ApplicationPurchased)
            {
                AppLicenseInfo = new XGENO.Framework.Services.AppLicenseService("9NBLGGH3SL90");
                await AppLicenseInfo.Initialize();

                ApplicationPurchased = AppLicenseInfo.IsPurchased;
            }

            await Task.CompletedTask;
        }

    }
}
