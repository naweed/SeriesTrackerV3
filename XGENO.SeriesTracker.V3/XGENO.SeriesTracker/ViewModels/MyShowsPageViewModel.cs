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
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Helpers;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker.ViewModels
{
    public class MyShowsPageViewModel : AppViewModelBase
    {
        private ObservableCollection<Show> _shows;
        public ObservableCollection<Show> Shows
        {
            get { return _shows; }
            set
            {
                Set(ref _shows, value);
            }
        }

        private string _nothingToWatchMessage;
        public string NothingToWatchMessage
        {
            get { return _nothingToWatchMessage; }
            set
            {
                Set(ref _nothingToWatchMessage, value);
            }
        }


        private bool _trackingListAvailable;
        public bool TrackingListAvailable
        {
            get { return _trackingListAvailable; }
            set
            {
                Set(ref _trackingListAvailable, value);
            }
        }

        private bool _archivedShows = false;
        public bool ArchivedShows
        {
            get { return _archivedShows; }
            set
            {
                Set(ref _archivedShows, value);
                AppSettings.ArchivedShowsInView = _archivedShows;
            }
        }

        private bool _hideShows = false;
        public bool HideShows
        {
            get
            {
                return _appSettings.HideShowsWithNoUpcomingInfoEnabled;
            }
            set
            {
                _appSettings.HideShowsWithNoUpcomingInfoEnabled = value;
                Set(ref _hideShows, value);
            }
        }

        private bool _sortAlphabeticallyEnabled = false;
        public bool SortAlphabeticallyEnabled
        {
            get
            {
                return _appSettings.SortAlphabeticallyEnabled;
            }
            set
            {
                _appSettings.SortAlphabeticallyEnabled = value;
                Set(ref _sortAlphabeticallyEnabled, value);
            }
        }

        DelegateCommand<ItemClickEventArgs> _navigateToShowPageCommand = null;
        public DelegateCommand<ItemClickEventArgs> NavigateToShowPageCommand
        {
            get
            {
                if (_navigateToShowPageCommand != null)
                    return _navigateToShowPageCommand;

                _navigateToShowPageCommand = new DelegateCommand<ItemClickEventArgs>
                (
                    (args) =>
                    {
                        NavigateToShowsPage((args.ClickedItem as Show).ids.trakt);
                    }
                );

                return _navigateToShowPageCommand;
            }
        }

        DelegateCommand<Show> _markAllEpisodesAsWatchedCommand = null;
        public DelegateCommand<Show> MarkAllEpisodesAsWatchedCommand
        {
            get
            {
                if (_markAllEpisodesAsWatchedCommand != null)
                    return _markAllEpisodesAsWatchedCommand;

                _markAllEpisodesAsWatchedCommand = new DelegateCommand<Show>
                (
                    (args) =>
                    {
                        MarkAllEpisodesAsWatched(args);
                    }
                );

                return _markAllEpisodesAsWatchedCommand;
            }
        }

        DelegateCommand<Show> _markSelectedEpisodesAsWatchedCommand = null;
        public DelegateCommand<Show> MarkSelectedEpisodesAsWatchedCommand
        {
            get
            {
                if (_markSelectedEpisodesAsWatchedCommand != null)
                    return _markSelectedEpisodesAsWatchedCommand;

                _markSelectedEpisodesAsWatchedCommand = new DelegateCommand<Show>
                (
                    (args) =>
                    {
                        MarkSelectedEpisodesAsWatched(args);
                    }
                );

                return _markSelectedEpisodesAsWatchedCommand;
            }
        }

        DelegateCommand<Show> _stopTrackingCommand = null;
        public DelegateCommand<Show> StopTrackingCommand
        {
            get
            {
                if (_stopTrackingCommand != null)
                    return _stopTrackingCommand;

                _stopTrackingCommand = new DelegateCommand<Show>
                (
                    (args) =>
                    {
                        StopTracking(args);
                    }
                );

                return _stopTrackingCommand;
            }
        }

        DelegateCommand<Show> _archiveShowCommand = null;
        public DelegateCommand<Show> ArchiveShowCommand
        {
            get
            {
                if (_archiveShowCommand != null)
                    return _archiveShowCommand;

                _archiveShowCommand = new DelegateCommand<Show>
                (
                    (args) =>
                    {
                        ArchiveShow(args);
                    }
                );

                return _archiveShowCommand;
            }
        }

        DelegateCommand<Show> _unarchiveShowCommand = null;
        public DelegateCommand<Show> UnArchiveShowCommand
        {
            get
            {
                if (_unarchiveShowCommand != null)
                    return _unarchiveShowCommand;

                _unarchiveShowCommand = new DelegateCommand<Show>
                (
                    (args) =>
                    {
                        UnArchiveShow(args);
                    }
                );

                return _unarchiveShowCommand;
            }
        }

        DelegateCommand _closeFlyOutCommand = null;
        public DelegateCommand CloseFlyOutCommand
        {
            get
            {
                if (_closeFlyOutCommand != null)
                    return _closeFlyOutCommand;

                _closeFlyOutCommand = new DelegateCommand
                (
                    () =>
                    {
                        this.IsFlyoutClosed = true;
                    }
                );

                return _closeFlyOutCommand;
            }
        }


        DelegateCommand _showOnlyArchivedShows = null;
        public DelegateCommand ShowOnlyArchivedShows
        {
            get
            {
                if (_showOnlyArchivedShows != null)
                    return _showOnlyArchivedShows;

                _showOnlyArchivedShows = new DelegateCommand
                (
                    () =>
                    {
                        if (!_appSettings.ApplicationPurchased)
                        {
                            DialogService.ShowAsync("You need to UNLOCK the application to use this feature.", "FULL VERSION");
                        }
                        else
                        {
                            LoadShows(true);
                        }
                    }
                );

                return _showOnlyArchivedShows;
            }
        }

        DelegateCommand _showOnlyNonArchivedShows = null;
        public DelegateCommand ShowOnlyNonArchivedShows
        {
            get
            {
                if (_showOnlyNonArchivedShows != null)
                    return _showOnlyNonArchivedShows;

                _showOnlyNonArchivedShows = new DelegateCommand
                (
                    () =>
                    {
                        if (!_appSettings.ApplicationPurchased)
                        {
                            DialogService.ShowAsync("You need to UNLOCK the application to use this feature.", "FULL VERSION");
                        }
                        else
                        {
                            LoadShows(false);
                        }
                    }
                );

                return _showOnlyNonArchivedShows;
            }
        }

        DelegateCommand _showInactiveShowsCommand = null;
        public DelegateCommand ShowInactiveShowsCommand
        {
            get
            {
                if (_showInactiveShowsCommand != null)
                    return _showInactiveShowsCommand;

                _showInactiveShowsCommand = new DelegateCommand
                (
                    () =>
                    {
                        if (!_appSettings.ApplicationPurchased)
                        {
                            DialogService.ShowAsync("You need to UNLOCK the application to use this feature.", "FULL VERSION");
                        }
                        else
                        {
                            this.HideShows = false;
                            LoadShows(ArchivedShows);
                        }
                    }
                );

                return _showInactiveShowsCommand;
            }
        }

        DelegateCommand _hideInactiveShowsCommand = null;
        public DelegateCommand HideInactiveShowsCommand
        {
            get
            {
                if (_hideInactiveShowsCommand != null)
                    return _hideInactiveShowsCommand;

                _hideInactiveShowsCommand = new DelegateCommand
                (
                    () =>
                    {
                        if (!_appSettings.ApplicationPurchased)
                        {
                            DialogService.ShowAsync("You need to UNLOCK the application to use this feature.", "FULL VERSION");
                        }
                        else
                        {
                            this.HideShows = true;
                            LoadShows(ArchivedShows);
                        }
                    }
                );

                return _hideInactiveShowsCommand;
            }
        }

        DelegateCommand _sortByStatusCommand = null;
        public DelegateCommand SortByStatusCommand
        {
            get
            {
                if (_sortByStatusCommand != null)
                    return _sortByStatusCommand;

                _sortByStatusCommand = new DelegateCommand
                (
                    () =>
                    {
                        if (!_appSettings.ApplicationPurchased)
                        {
                            DialogService.ShowAsync("You need to UNLOCK the application to use this feature.", "FULL VERSION");
                        }
                        else
                        {
                            this.SortAlphabeticallyEnabled = false;
                            LoadShows(ArchivedShows);
                        }
                    }
                );

                return _sortByStatusCommand;
            }
        }

        DelegateCommand _sortAlphabeticalCommand = null;
        public DelegateCommand SortAlphabeticalCommand
        {
            get
            {
                if (_sortAlphabeticalCommand != null)
                    return _sortAlphabeticalCommand;

                _sortAlphabeticalCommand = new DelegateCommand
                (
                    () =>
                    {
                        if (!_appSettings.ApplicationPurchased)
                        {
                            DialogService.ShowAsync("You need to UNLOCK the application to use this feature.", "FULL VERSION");
                        }
                        else
                        {
                            this.SortAlphabeticallyEnabled = true;
                            LoadShows(ArchivedShows);
                        }
                    }
                );

                return _sortAlphabeticalCommand;
            }
        }

        public MyShowsPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            await LoadShows(AppSettings.ArchivedShowsInView);
        }

        private async Task LoadShows(bool flgArchivedShows)
        {
            this.LoadingText = "PLEASE WAIT ..." + Environment.NewLine + "LOADING LIST OF YOUR SHOWS#|#THIS MAY TAKE A WHILE FOR THE FIRST TIME";
            this.DataLoaded = false;
            this.TrackingListAvailable = false;
            this.NothingToWatchMessage = "";
            this.ArchivedShows = flgArchivedShows;
            this.HideShows = _appSettings.HideShowsWithNoUpcomingInfoEnabled;
            this.SortAlphabeticallyEnabled = _appSettings.SortAlphabeticallyEnabled;
            this.Shows = new ObservableCollection<Show>();

            try
            {
                //Get my tracked shows
                List<Show> showsList = await AppServiceHelper.GetMyTrackedShows(_appService, false, ArchivedShows, HideShows, false);


                //Exclude Ended Shows if needed
                if (HideShows)
                {
                    showsList = showsList.Where(_s => { var _status = _s.ShowTrackingStatus; return !(_status.StartsWith("ALL CAUGHT UP") && (_status.EndsWith("TBA") || _status.EndsWith("ENDED"))); }).ToList();
                }


                //Set Missed Episodes List
                showsList.ForEach(_s => { _s.SetMissedEpisodesList(); });


                if (this.SortAlphabeticallyEnabled)
                {
                    //Sort Alphabetically
                    showsList = showsList.OrderBy(_s => _s.ShowTitle).ToList();
                }
                else
                {
                    //Sort By Status
                    showsList = showsList.OrderBy(_s => _s.SortOrder).ThenBy(_s => _s.ShowTitle).ToList();
                }

                //Set the Show List
                TrackingListAvailable = (showsList.Count > 0);
                Shows = new ObservableCollection<Show>(showsList);

                if (Shows.Count == 0)
                    this.NothingToWatchMessage = "NOTHING TO SHOW OVER HERE";

                showsList = null;

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



        private async Task NavigateToShowsPage(int traktID)
        {
            await NavigationService.NavigateAsync(typeof(Views.ShowDetailsPage), traktID.ToString());
        }

        private async Task MarkAllEpisodesAsWatched(Show theShow)
        {

            //Set all episodes as watched
            (theShow.MissedEpisodesList.Source as List<SeasonEpisodeGroup>).ForEach(_s => _s.Episodes.ForEach(_e => { _e.IsSelected = true; }));

            await MarkSelectedEpisodesAsWatched(theShow);

            await Task.CompletedTask;
        }

        private async Task MarkSelectedEpisodesAsWatched(Show theShow)
        {
            this.IsFlyoutClosed = true;

            //Get List of Episodes to Mark
            List<TrackedEpisode> episodesToTrack = new List<TrackedEpisode>();

            var theSeasonGroup = theShow.MissedEpisodesList.Source as List<SeasonEpisodeGroup>;

            foreach (var _season in theSeasonGroup)
            {
                foreach (var _episodeToMark in _season.Episodes.Where(_e => _e.IsSelected.Value))
                {
                    episodesToTrack.Add(new TrackedEpisode()
                    {
                        ShowTraktID = _episodeToMark.ShowTraktID,
                        SeasonTraktID = _episodeToMark.SeasonTraktID,
                        EpisodeTraktID = _episodeToMark.ids.trakt,
                        SeasonNo = _episodeToMark.season,
                        EpisodeNo = _episodeToMark.number
                    });
                }
            }

            //Save Marked Episodes to SQLite and Trakt
            await ShowTrackingHelper.MarkEpisodesWatched(episodesToTrack);

            //Remove the episodes from the list

            foreach (var _season in theSeasonGroup)
            {
                foreach (var _episode in _season.Episodes.Where(_e => _e.IsSelected.Value))
                {
                    theShow.AllEpisodes.Where(_e => _e.ids.trakt == _episode.ids.trakt).First().IsWatched = true;
                }
            }

            //Recalculate Missed
            theShow.CalculateMissedEpisodesCount();
            theShow.ShowTrackingStatus = DateTime.Now.ToString();
            theShow.ShowTrackingStatusMobile = DateTime.Now.ToString();
            theShow.WatchedPercentage = theShow.MissedEpisodeCount + 100;
            theShow.SetMissedEpisodesList();


            await Task.CompletedTask;
        }

        private async Task StopTracking(Show theShow)
        {
            if (!_appSettings.ApplicationPurchased)
            {
                await DialogService.ShowAsync("You need to UNLOCK the application to remove the show from watchlist.", "FULL VERSION");
                return;
            }

            //Sure to archive - Check with user
            var _returnStatus = true;

            var yesUICommand = new UICommand("Yes", (command) =>
            {
                _returnStatus = true;
            });
            var noUICommand = new UICommand("No", (command) =>
            {
                _returnStatus = false;
            });

            await DialogService.ShowAsync("Are you sure you want to stop tracking this show? You will lose all previous history on this show.", "STOP TRACKING", yesUICommand, noUICommand);

            if (_returnStatus)
            {
                try
                {
                    this.LoadingText = "PLEASE WAIT ..." + Environment.NewLine + "REMOVING THE SHOW HISTORY";

                    await ShowTrackingHelper.StopTrackingShow(theShow);

                    await LoadShows(ArchivedShows);
                    //Shows.Remove(theShow);
                }
                catch
                {

                }
                finally
                {
                    this.LoadingText = "";
                }
            }


            await Task.CompletedTask;
        }

        private async Task ArchiveShow(Show theShow)
        {
            if (!_appSettings.ApplicationPurchased)
            {
                await DialogService.ShowAsync("You need to UNLOCK the application to archive shows and exclude from watchlist.", "FULL VERSION");
                return;
            }

            if (!theShow.IsArchived)
            {
                //Sure to archive - Check with user
                var _returnStatus = true;

                var yesUICommand = new UICommand("Yes", (command) =>
                {
                    _returnStatus = true;
                });
                var noUICommand = new UICommand("No", (command) =>
                {
                    _returnStatus = false;
                });

                await DialogService.ShowAsync("Are you sure you want to archive this show? This will remove the show from your Active Watchlist (without losing any history).", "ARCHIVE SHOW", yesUICommand, noUICommand);


                if (_returnStatus)
                {
                    await ShowTrackingHelper.ArchiveShow(theShow);

                    await LoadShows(ArchivedShows);
                    //Shows.Remove(theShow);
                }
            }

            await Task.CompletedTask;
        }

        private async Task UnArchiveShow(Show theShow)
        {
            if (theShow.IsArchived)
            {
                await ShowTrackingHelper.UnArchiveShow(theShow);

                await LoadShows(ArchivedShows);
                //Shows.Remove(theShow);
            }

            await Task.CompletedTask;
        }

    }
}
