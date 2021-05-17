using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using XGENO.Framework.Exceptions;
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Helpers;
using XGENO.SeriesTracker.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Security.Authentication.Web;
using XGENO.Framework.Helpers;

namespace XGENO.SeriesTracker.ViewModels
{
    public class SettingsPageViewModel : AppViewModelBase
    {
        private bool _liveTilesEnabled;
        public bool LiveTilesEnabled
        {
            get { return _liveTilesEnabled; }
            set
            {
                Set(ref _liveTilesEnabled, value);
                _appSettings.LiveTilesEnabled = _liveTilesEnabled;
            }

        }

        private bool _airedNotificationsEnabled;
        public bool AiredNotificationsEnabled
        {
            get { return _airedNotificationsEnabled; }
            set
            {
                Set(ref _airedNotificationsEnabled, value);
                _appSettings.AiredNotificationsEnabled = _airedNotificationsEnabled;
            }

        }

        private bool _generalNotificationsEnabled;
        public bool GeneralNotificationsEnabled
        {
            get { return _generalNotificationsEnabled; }
            set
            {
                Set(ref _generalNotificationsEnabled, value);
                _appSettings.GeneralNotificationsEnabled = _generalNotificationsEnabled;
            }

        }

        private bool _hideShows;
        public bool HideShows
        {
            get { return _hideShows; }
            set
            {
                Set(ref _hideShows, value);
                _appSettings.HideShowsWithNoUpcomingInfoEnabled = _hideShows;
            }

        }

        private bool _sortAlphabeticallyEnabled;
        public bool SortAlphabeticallyEnabled
        {
            get { return _sortAlphabeticallyEnabled; }
            set
            {
                Set(ref _sortAlphabeticallyEnabled, value);
                _appSettings.SortAlphabeticallyEnabled = _sortAlphabeticallyEnabled;
            }

        }

        private bool _startPageIsWatchList;
        public bool StartPageIsWatchList
        {
            get { return _startPageIsWatchList; }
            set
            {
                Set(ref _startPageIsWatchList, value);
                _appSettings.StartPageIsWatchList = _startPageIsWatchList;
            }

        }



        private bool _canStopSyncing;
        public bool CanStopSyncing
        {
            get { return _canStopSyncing && _appSettings.ApplicationPurchased; }
            set
            {
                Set(ref _canStopSyncing, value);
            }
        }

        DelegateCommand _unlockFullVersionCommand = null;
        public DelegateCommand UnlockFullVersionCommand
        {
            get
            {
                if (_unlockFullVersionCommand != null)
                    return _unlockFullVersionCommand;

                _unlockFullVersionCommand = new DelegateCommand
                (
                    () =>
                    {
                        UnlockFullVersion();
                    }
                );

                return _unlockFullVersionCommand;
            }
        }

        DelegateCommand _clearCacheCommand = null;
        public DelegateCommand ClearCacheCommand
        {
            get
            {
                if (_clearCacheCommand != null)
                    return _clearCacheCommand;

                _clearCacheCommand = new DelegateCommand
                (
                    () =>
                    {
                        ClearCache();
                    }
                );

                return _clearCacheCommand;
            }
        }

        DelegateCommand _traktStopSyncCommand = null;
        public DelegateCommand TraktStopSyncCommand
        {
            get
            {
                if (_traktStopSyncCommand != null)
                    return _traktStopSyncCommand;

                _traktStopSyncCommand = new DelegateCommand
                (
                    () =>
                    {
                        TraktStopSync();
                    }
                );

                return _traktStopSyncCommand;
            }
        }

        DelegateCommand _traktSyncCommand = null;
        public DelegateCommand TraktSyncCommand
        {
            get
            {
                if (_traktSyncCommand != null)
                    return _traktSyncCommand;

                _traktSyncCommand = new DelegateCommand
                (
                    () =>
                    {
                        TraktSync();
                    }
                );

                return _traktSyncCommand;
            }
        }

        DelegateCommand _saveProgressCommand = null;
        public DelegateCommand SaveProgressCommand
        {
            get
            {
                if (_saveProgressCommand != null)
                    return _saveProgressCommand;

                _saveProgressCommand = new DelegateCommand
                (
                    () =>
                    {
                        SaveProgress();
                    }
                );

                return _saveProgressCommand;
            }
        }

        DelegateCommand _restoreProgressCommand = null;
        public DelegateCommand RestoreProgressCommand
        {
            get
            {
                if (_restoreProgressCommand != null)
                    return _restoreProgressCommand;

                _restoreProgressCommand = new DelegateCommand
                (
                    () =>
                    {
                        RestoreProgress();
                    }
                );

                return _restoreProgressCommand;
            }
        }

        public SettingsPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            await LoadSettings();
        }

        private async Task LoadSettings()
        {
            this.LoadingText = "PLEASE WAIT ..." + Environment.NewLine + "PREPARING THE SETTINGS SCREEN";
            this.DataLoaded = false;

            try
            {
                //Load Settings
                this._liveTilesEnabled = _appSettings.LiveTilesEnabled;
                this._airedNotificationsEnabled = _appSettings.AiredNotificationsEnabled;
                this._generalNotificationsEnabled = _appSettings.GeneralNotificationsEnabled;
                this._sortAlphabeticallyEnabled = _appSettings.SortAlphabeticallyEnabled;
                this._hideShows = _appSettings.HideShowsWithNoUpcomingInfoEnabled;
                this._startPageIsWatchList = _appSettings.StartPageIsWatchList;
                CanStopSyncing = !string.IsNullOrEmpty(_appSettings.TraktAccessToken) && _appSettings.ApplicationPurchased;

            }
            catch (InternetConnectionException iex)
            {
                await DialogService.ShowAsync("You are not connected to Internet at the moment. Please try again once the connection is restored.", "Internet Connectivity");
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync("An error occured. Please try again. If the problem persists, plz contact Support team via WinApps@xgeno.com." + Environment.NewLine + "Error Message: " + ex.Message, "Error");
            }
            finally
            {
                this.LoadingText = "";
            }
        }


        private async Task UnlockFullVersion()
        {
            var _returnStatus = true;

            var yesUICommand = new UICommand("Yes", (command) =>
            {
                _returnStatus = true;
            });

            var noUICommand = new UICommand("No", (command) =>
            {
                _returnStatus = false;
            });

            await DialogService.ShowAsync("Are you sure you want to unlock the full version for $1.99 (or equivalent in your local currency)?", "UNLOCK FULL VERSION", yesUICommand, noUICommand);

            if (_returnStatus)
            {
                this.LoadingText = "PLEASE WAIT ..." + Environment.NewLine + "UNLOCKING THE FULL VERSION";

                try
                {
                    await _appSettings.LoadAppLicense();

                    var isPurchased = await _appSettings.AppLicenseInfo.Purchase();

                    if (isPurchased)
                    {
                        _appSettings.LiveTilesEnabled = true;
                        _appSettings.ApplicationPurchased = true;
                        _appSettings.AiredNotificationsEnabled = true;
                    }
                    else
                    {
                        _appSettings.LiveTilesEnabled = false;
                        _appSettings.ApplicationPurchased = false;
                        _appSettings.AiredNotificationsEnabled = false;
                    }
                }
                catch (Exception ex)
                {
                    await DialogService.ShowAsync("An error occured while completing the purchase. Please try again later. Contact Microsoft Support if the problem persists." + Environment.NewLine + "Error Message: " + ex.Message, "APPLICATION UNLOCK ERROR");
                }
                finally
                {
                    this.LoadingText = "";
                }

            }

            await Task.CompletedTask;
        }

        private async Task ClearCache()
        {
            var _returnStatus = true;

            var yesUICommand = new UICommand("Yes", (command) =>
            {
                _returnStatus = true;
            });

            var noUICommand = new UICommand("No", (command) =>
            {
                _returnStatus = false;
            });

            await DialogService.ShowAsync("Are you sure you want to clear the cache? This might take a while.", "CLEAR CACHE", yesUICommand, noUICommand);

            if (_returnStatus)
            {
                this.LoadingText = "PLEASE WAIT ..." + Environment.NewLine + "CLEARING THE APPLICATION CACHE";

                //Clear Image Cache
                await WebCacheHelper.ClearAsync();
                
                //Clear SQlite Cache
                try
                {
                    await SQLiteService.Instance.ClearDataCache();
                }
                catch
                {
                }

                this.LoadingText = "";

                await DialogService.ShowAsync("Cache has been cleared successfully! Please restart the app for optimal results.", "CACHE CLEARED");
            }

            await Task.CompletedTask;
        }

        private async Task TraktStopSync()
        {
            var _returnStatus = true;

            var yesUICommand = new UICommand("Yes", (command) =>
            {
                _returnStatus = true;
            });

            var noUICommand = new UICommand("No", (command) =>
            {
                _returnStatus = false;
            });

            await DialogService.ShowAsync("Are you sure you want to stop Auto-Sync progress with your Trakt.tv account?" + Environment.NewLine + "You can re-connect at any time in future.", "STOP TRAKT SYNC", yesUICommand, noUICommand);

            if (_returnStatus)
            {
                _appSettings.TraktAccessToken = "";
                _appSettings.TraktRefreshToken = "";
                CanStopSyncing = !string.IsNullOrEmpty(_appSettings.TraktAccessToken) && _appSettings.ApplicationPurchased;

                ToastNotificationService.ShowSimpleToast("You have stopped Auto-Syncing with your Trakt.tv account!", "STOP TRAKT SYNC");
            }

            await Task.CompletedTask;
        }

        private async Task TraktSync()
        {
            //Check if already connected
            if (!string.IsNullOrEmpty(_appSettings.TraktAccessToken))
            {
                var _returnStatus = true;

                var yesUICommand = new UICommand("Yes", (command) =>
                {
                    _returnStatus = true;
                });

                var noUICommand = new UICommand("No", (command) =>
                {
                    _returnStatus = false;
                });

                await DialogService.ShowAsync("You are already syncing your progress with Trakt. Are you sure, you want to re-connect?" + Environment.NewLine + "You will need to enter your Trakt credentials again.", "TRAKT SYNC", yesUICommand, noUICommand);

                if (!_returnStatus)
                {
                    return;
                }
            }

            try
            {
                this.LoadingText = "HOLD ON ..." + Environment.NewLine + "SYNCING YOUR PROGRESS WITH TRAKT";

                var oAuthURI = "https://api-v2launch.trakt.tv/oauth/authorize?response_type=code&client_id=" + AppSettings.TraktSyncClientID + "&redirect_uri=" + Uri.EscapeDataString("https://xgeno.com");

                var startURI = new Uri(oAuthURI);
                var endURI = new Uri("https://xgeno.com");

                WebAuthenticationResult webAuthBroker = await WebAuthenticationBroker.AuthenticateAsync(WebAuthenticationOptions.None, startURI, endURI);

                if (webAuthBroker.ResponseStatus == WebAuthenticationStatus.Success)
                {
                    var oAuthResponse = webAuthBroker.ResponseData.ToString();
                    var oAuthCodeString = oAuthResponse.Substring(oAuthResponse.IndexOf("code"));
                    var oAuthCode = oAuthCodeString.Split(new string[] { "&" }, StringSplitOptions.RemoveEmptyEntries)[0].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries)[1];

                    //Request Token
                    var oAuthPostData = new SyncOAuthPostData();
                    oAuthPostData.client_id = AppSettings.TraktSyncClientID;
                    oAuthPostData.client_secret = AppSettings.TraktSyncClientSecret;
                    oAuthPostData.code = oAuthCode;
                    oAuthPostData.grant_type = "authorization_code";
                    oAuthPostData.redirect_uri = "https://xgeno.com";

                    RestService traktService = new RestService(true, _appSettings);
                    var tokenResponse = await traktService.PostAsync<SyncOAuthPostData>("oauth/token", oAuthPostData);

                    var tokenResponseString = await tokenResponse.Content.ReadAsStringAsync();

                    var tokenData = SerializationHelper.Deserialize<SyncTokenResponse>(tokenResponseString);

                    _appSettings.TraktAccessToken = tokenData.access_token;
                    _appSettings.TraktRefreshToken = tokenData.refresh_token;
                    _appSettings.TraktTokenExpiresOn = tokenData.ExpiresOn;

                    //Sync the progress
                    await AppServiceHelper.SyncTraktProgress(_appSettings);

                    ToastNotificationService.ShowSimpleToast("Your progress is now syncing with Trakt.tv!", "TRAKT SYNC");
                    CanStopSyncing = !string.IsNullOrEmpty(_appSettings.TraktAccessToken) && _appSettings.ApplicationPurchased;
                }
                else
                {
                    await DialogService.ShowAsync("An error occured! Couldn't connect to Trakt service. Please try again later.", "TRAKT SYNC");
                }
            }
            catch (Exception ex)
            {
                await DialogService.ShowAsync("An error occured while syncing progress!" + Environment.NewLine + "Error Message: " + ex.Message, "TRAKT SYNC");
            }
            finally
            {
                this.LoadingText = "";
            }

            await Task.CompletedTask;
        }

        private async Task SaveProgress()
        {
            FileSavePicker savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("Series Tracker Backup", new List<string>() { ".bkp" });
            savePicker.SuggestedFileName = "SeriesTracker_" + DateTime.Now.ToString("yyyyMMdd") + ".bkp";

            StorageFile file = await savePicker.PickSaveFileAsync();

            if (file != null)
            {
                try
                {
                    this.LoadingText = "PLEASE WAIT ..." + Environment.NewLine + "SAVING YOUR PROGRESS TO A FILE";


                    //Load Tracked Lists
                    await SQLiteService.Instance.LoadTrackingShowsList();
                    await SQLiteService.Instance.LoadTrackedEpisodesList();
                    await SQLiteService.Instance.LoadNotifiedEpisodesList();


                    // Prevent updates to the remote version of the file until we finish making changes and call CompleteUpdatesAsync. 
                    CachedFileManager.DeferUpdates(file);

                    var backupSet = new BackupSet();
                    backupSet.TrackingList.AddRange(AppSettings.TrackingList);
                    backupSet.NotifiedEpisodeList.AddRange(AppSettings.NotifiedEpisodeList);
                    backupSet.TrackedEpisodeList.AddRange(AppSettings.TrackedEpisodeList);

                    var backupData = SerializationHelper.Serialize<BackupSet>(backupSet);

                    // write to file 
                    await FileIO.WriteTextAsync(file, backupData);

                    // Let Windows know that we're finished changing the file so the other app can update the remote version of the file. 
                    // Completing updates may require Windows to ask for user input. 
                    FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == FileUpdateStatus.Complete)
                    {
                        ToastNotificationService.ShowSimpleToast("Progress has been saved successfully!", "BACKUP COMPLETE");
                    }
                    else
                    {
                        await DialogService.ShowAsync("An error occured while saving your progress!", "BACKUP ERROR");
                    }
                }
                catch (Exception ex)
                {
                    await DialogService.ShowAsync("An error occured while saving your progress!" + Environment.NewLine + "Error Message: " + ex.Message, "BACKUP ERROR");
                                    }
                finally
                {
                    this.LoadingText = "";
                }
            }
            else
            {
                //Operation cancelled
            }

            await Task.CompletedTask;
        }

        private async Task RestoreProgress()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            openPicker.FileTypeFilter.Add(".bkp");

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file != null)
            {
                try
                {
                    this.LoadingText = "PLEASE WAIT ..." + Environment.NewLine + "RESTORING YOUR PROGRESS";

                    //Load Tracked Lists
                    await SQLiteService.Instance.LoadTrackingShowsList();
                    await SQLiteService.Instance.LoadTrackedEpisodesList();
                    await SQLiteService.Instance.LoadNotifiedEpisodesList();


                    bool showListUpdated = false;
                    bool episodeListUpdated = false;
                    bool notifiedListUpdated = false;


                    // read from file 
                    var restoreData = await FileIO.ReadTextAsync(file);

                    var restoreSet = SerializationHelper.Deserialize<BackupSet>(restoreData);

                    //Tracked Shows List
                    foreach (var _show in restoreSet.TrackingList)
                    {
                        if (!AppSettings.TrackingList.Where(_x => _x.ShowTraktID == _show.ShowTraktID).Any())
                        {
                            AppSettings.TrackingList.Add(_show);
                            showListUpdated = true;
                        }
                    }

                    //Tracked Episdes List
                    foreach (var _episode in restoreSet.TrackedEpisodeList)
                    {
                        if (!AppSettings.TrackedEpisodeList.Where(_x => _x.EpisodeTraktID == _episode.EpisodeTraktID).Any())
                        {
                            AppSettings.TrackedEpisodeList.Add(_episode);
                            episodeListUpdated = true;
                        }
                    }

                    //Notified Episodes List
                    foreach (var _episode in restoreSet.NotifiedEpisodeList)
                    {
                        if (!AppSettings.NotifiedEpisodeList.Where(_x => _x.EpisodeTraktID == _episode.EpisodeTraktID).Any())
                        {
                            AppSettings.NotifiedEpisodeList.Add(_episode);
                            notifiedListUpdated = true;
                        }
                    }

                    await SQLiteService.Instance.RestoreDataSet(showListUpdated, episodeListUpdated, notifiedListUpdated);

                    ToastNotificationService.ShowSimpleToast("Progress has been restored successfully!", "RESTORE COMPLETED");

                }
                catch (Exception ex)
                {
                    await DialogService.ShowAsync("An error occured while restoring your progress!" + Environment.NewLine + "Error Message: " + ex.Message, "RESTORE ERROR");
                }
                finally
                {
                    this.LoadingText = "";
                }
            }
            else
            {
                //Operation cancelled
            }

            await Task.CompletedTask;
        }

    }
}
