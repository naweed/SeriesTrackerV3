using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Exceptions;
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Helpers;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker.ViewModels
{
    public class SearchPageViewModel : AppViewModelBase
    {
        private List<Show> _shows;
        public List<Show> Shows
        {
            get { return _shows; }
            set
            {
                Set(ref _shows, value);
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


        DelegateCommand<Show> _startTrackingShowCommand = null;
        public DelegateCommand<Show> StartTrackingShowCommand
        {
            get
            {
                if (_startTrackingShowCommand != null)
                    return _startTrackingShowCommand;

                _startTrackingShowCommand = new DelegateCommand<Show>
                (
                    (args) =>
                    {
                        StartTrackingShow(args);
                    }
                );

                return _startTrackingShowCommand;
            }
        }


        public SearchPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
        }

        public async Task SearchShows(string searchQuery)
        {
            this.LoadingText = "HOLD ON ..." + Environment.NewLine + "SEARCH IN PROGRESS";
            this.DataLoaded = false;


            try
            {
                List<Show> showsList = new List<Show>();

                //Load Tracked Lists
                await SQLiteService.Instance.LoadTrackingShowsList();

                ////Search for Shows

                //Specific Search
                if (searchQuery.ToLower().StartsWith("tt"))
                {
                    var searchResultsSpecific = await _appService.GetAsync<List<SearchResult>>("search", "type=show&id_type=imdb&id=" + searchQuery, 24);

                    if (searchResultsSpecific != null)
                    {
                        foreach (var result in searchResultsSpecific)
                        {
                            showsList.Add(result.show);
                        }
                    }
                }


                //General Search
                var searchResultsGeneral = await _appService.GetAsync<List<SearchResult>>("search", "type=show&limit=120&query=" + searchQuery, 24);

                foreach (var result in searchResultsGeneral)
                {
                    showsList.Add(result.show);
                }

                
                //IsTracking Info
                foreach (var _show in showsList)
                {
                    _show.IsTracking = AppSettings.TrackingList.Where(s => s.ShowTraktID == _show.ids.trakt).Any();
                }

                
                //Set the Shows
                Shows = showsList;

                showsList = null;

                //If no search results found
                if (Shows.Count == 0)
                {
                    await DialogService.ShowAsync("Oops! Could not find any show matching your search criteria. Please try with different show name.", "SEARCH RESULTS");
                }

                this.DataLoaded = true;
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

        private async Task StartTrackingShow(Show theShow)
        {
            if (!theShow.IsTracking)
            {
                await ShowTrackingHelper.StartTrackingShow(theShow);
            }
        }

    }
}
