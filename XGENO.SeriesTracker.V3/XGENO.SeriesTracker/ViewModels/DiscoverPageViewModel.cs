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
    public class DiscoverPageViewModel : AppViewModelBase
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

        private List<Show> _showsMobile;
        public List<Show> ShowsMobile
        {
            get { return _showsMobile; }
            set
            {
                Set(ref _showsMobile, value);
            }
        }

        private List<Show> _heroShows;
        public List<Show> HeroShows
        {
            get { return _heroShows; }
            set
            {
                Set(ref _heroShows, value);
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

        DelegateCommand<Show> _navigateToShowPage2Command = null;
        public DelegateCommand<Show> NavigateToShowPage2Command
        {
            get
            {
                if (_navigateToShowPage2Command != null)
                    return _navigateToShowPage2Command;

                _navigateToShowPage2Command = new DelegateCommand<Show>
                (
                    (args) =>
                    {
                        NavigateToShowsPage(args.ids.trakt);
                    }
                );

                return _navigateToShowPage2Command;
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


        public DiscoverPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            await LoadShows();
        }

        private async Task LoadShows()
        {
            this.LoadingText = "HOLD ON ..." + Environment.NewLine + "PREPARING LIST OF AMAZING SHOWS FOR YOU";
            this.DataLoaded = false;


            try
            {
                List<Show> showsList = new List<Show>();

                //Load Tracked Lists
                await SQLiteService.Instance.LoadTrackingShowsList();

                
                //Get Popular Shows
                var popularList = await _appService.GetAsync<List<Show>>("shows/popular", "extended=full&limit=120", 200);

                showsList.AddRange(popularList);

                //Get Trending Shows
                var trendingList = await _appService.GetAsync<List<TrendingShow>>("shows/trending", "extended=full&limit=120", 200);

                foreach (var show in trendingList)
                {
                    if (!showsList.Where(_s => _s.ids.trakt == show.show.ids.trakt).Any())
                    {
                        show.show.IsPopular = false;
                        show.show.IsTrending = true;
                        showsList.Add(show.show);
                    }
                    else
                    {
                        showsList.Where(_s => _s.ids.trakt == show.show.ids.trakt).First().IsTrending = true;
                    }
                }

                //IsTracking Info
                foreach (var _show in showsList)
                {
                    _show.IsTracking = AppSettings.TrackingList.Where(s => s.ShowTraktID == _show.ids.trakt).Any();
                }

                showsList = showsList.Where(_s => !_s.IsTracking).ToList();


                //Set the Shows
                HeroShows = showsList.Take(5).ToList();
                Shows = showsList.Skip(5).Take(60).ToList();
                ShowsMobile = showsList.Take(40).ToList();

                showsList = null;

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
