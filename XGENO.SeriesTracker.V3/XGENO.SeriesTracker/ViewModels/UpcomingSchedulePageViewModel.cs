using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using XGENO.Framework.Exceptions;
using XGENO.Framework.Extensions;
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Helpers;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker.ViewModels
{
    public class UpcomingSchedulePageViewModel : AppViewModelBase
    {
        private List<EpisodeGroup> _episodesGroup;
        public List<EpisodeGroup> EpisodesGroup
        {
            get { return _episodesGroup; }
            set
            {
                Set(ref _episodesGroup, value);
            }
        }

        private bool _upcomingListAvailable;
        public bool UpcomingListAvailable
        {
            get { return _upcomingListAvailable; }
            set
            {
                Set(ref _upcomingListAvailable, value);
            }
        }

        
        public UpcomingSchedulePageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            await LoadUpcomingEpisodes();
        }

        private async Task LoadUpcomingEpisodes()
        {
            this.LoadingText = "HOLD ON ..." + Environment.NewLine + "PERPARING YOUR SCHEDULE FOR NEXT ONE MONTH";
            this.DataLoaded = false;
            this.UpcomingListAvailable = true;

            try
            {
                //Get my tracked shows
                List<Show> showsList = await AppServiceHelper.GetMyTrackedShows(_appService, false, false, _appSettings.HideShowsWithNoUpcomingInfoEnabled, false);

                //Get Upcoming Episodes
                var upcomingEpisodesList = new List<Episode>();

                showsList.ForEach(
                                    thisShow =>
                                    {
                                        upcomingEpisodesList.AddRange(thisShow.AllEpisodes.Where(_e => !_e.HasAired && _e.AiredDate != "TBA").Where(_e => _e.LocalAiredDate.Value <= (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).LastDayOfNextMonth()));
                                    }
                                 );

                //Order by Air Date
                upcomingEpisodesList = upcomingEpisodesList.OrderBy(_e => _e.LocalAiredDate.Value).ToList();

                //Upcoming Episodes Group List 
                var grouped = from episode in upcomingEpisodesList
                              group episode by episode.AiredDateGroup
                              into grp
                              select new EpisodeGroup
                              {
                                  Key = grp.Key,
                                  Episodes = grp.ToList()
                              };


                //Set the Episodes List
                UpcomingListAvailable = (upcomingEpisodesList.Count > 0);
                EpisodesGroup = grouped.ToList();

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

    }
}
