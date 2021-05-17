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
using XGENO.Framework.Extensions;
using XGENO.Framework.MVVM;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Helpers;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker.ViewModels
{
    public class MissedEpisodesPageViewModel : AppViewModelBase
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

        private List<Episode> _missedEpisodes;
        public List<Episode> MissedEpisodes
        {
            get { return _missedEpisodes; }
            set
            {
                Set(ref _missedEpisodes, value);
            }
        }
        
        private bool _missedListAvailable;
        public bool MissedListAvailable
        {
            get { return _missedListAvailable; }
            set
            {
                Set(ref _missedListAvailable, value);
            }
        }


        DelegateCommand<Episode> _markWatchedCommand = null;
        public DelegateCommand<Episode> MarkWatchedCommand
        {
            get
            {
                if (_markWatchedCommand != null)
                    return _markWatchedCommand;

                _markWatchedCommand = new DelegateCommand<Episode>
                (
                    (args) =>
                    {
                        MarkEpisodeWatched(args);
                    }
                );

                return _markWatchedCommand;
            }
        }


        public MissedEpisodesPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            await LoadMissedEpisodes();
        }

        private async Task LoadMissedEpisodes()
        {
            this.LoadingText = "HOLD ON ..." + Environment.NewLine + "PERPARING THE LIST OF YOUR MISSED EPISODES";
            this.DataLoaded = false;
            this.MissedListAvailable = true;

            try
            {
                //Get my tracked shows
                List<Show> showsList = await AppServiceHelper.GetMyTrackedShows(_appService, false, false, _appSettings.HideShowsWithNoUpcomingInfoEnabled, false);

                //Get Upcoming Episodes
                var missedEpisodesList = new List<Episode>();

                showsList.ForEach(
                                    thisShow =>
                                    {
                                        missedEpisodesList.AddRange(thisShow.AllEpisodes.Where(_e => _e.HasAired && !_e.IsWatched && _e.season > 0));
                                    }
                                 );

                //Order by Name and Air Date
                missedEpisodesList = missedEpisodesList.OrderBy(_e => _e.ShowName).ThenBy(_e => _e.AiredDateToCompare).ThenBy(_e => _e.number).ToList();
                MissedEpisodes = new List<Episode>();
                MissedEpisodes.AddRange(missedEpisodesList);

                //Upcoming Episodes Group List 
                var grouped = from episode in missedEpisodesList
                              group episode by episode.ShowName
                                  into grp
                                  select new EpisodeGroup
                                  {
                                      Key = grp.Key,
                                      Episodes = grp.ToList()
                                  };


                //Set the Episodes List
                MissedListAvailable = (missedEpisodesList.Count > 0);
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

        private async Task MarkEpisodeWatched(Episode theEpisode)
        {
            List<TrackedEpisode> episodesToTrack = new List<TrackedEpisode>();

            var previousEpisodes = MissedEpisodes.Where(_e => _e.ShowTraktID == theEpisode.ShowTraktID && _e.HasAired && !_e.IsWatched && _e.season > 0 && ((_e.season == theEpisode.season && _e.number <= theEpisode.number) || _e.season < theEpisode.season)).ToList();


            if (previousEpisodes.Count > 1)
            {
                //Multiple Episodes - Check with user
                var _returnStatus = true;

                var yesUICommand = new UICommand("Yes", (command) =>
                {
                    _returnStatus = true;
                });
                var noUICommand = new UICommand("No", (command) =>
                {
                    _returnStatus = false;
                });

                await DialogService.ShowAsync("Mark all previous episodes of this show as watched?", theEpisode.ShowName, yesUICommand, noUICommand);

                if (_returnStatus)
                {
                    //Mark all previous
                    foreach (var _episodeToMark in previousEpisodes)
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
                else
                {
                    //Mark only Current
                    episodesToTrack.Add(new TrackedEpisode()
                    {
                        ShowTraktID = theEpisode.ShowTraktID,
                        SeasonTraktID = theEpisode.SeasonTraktID,
                        EpisodeTraktID = theEpisode.ids.trakt,
                        SeasonNo = theEpisode.season,
                        EpisodeNo = theEpisode.number
                    });
                }
            }
            else
            {
                //Add only the current Episode
                episodesToTrack.Add(new TrackedEpisode()
                {
                    ShowTraktID = theEpisode.ShowTraktID,
                    SeasonTraktID = theEpisode.SeasonTraktID,
                    EpisodeTraktID = theEpisode.ids.trakt,
                    SeasonNo = theEpisode.season,
                    EpisodeNo = theEpisode.number
                });
            }

            //Save Marked Episodes to SQLite and Trakt
            await ShowTrackingHelper.MarkEpisodesWatched(episodesToTrack);
            
            //Mark episodes as Watched in this list
            foreach (var _episode in episodesToTrack)
            {
                var episodeToMark = MissedEpisodes.Where(_e => _e.ids.trakt == _episode.EpisodeTraktID).First();

                episodeToMark.IsWatched = true;
            }


            await Task.CompletedTask;
        }

    }
}
