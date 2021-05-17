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
    public class ShowDetailsPageViewModel : AppViewModelBase
    {
        private Show _theShow;
        public Show TheShow
        {
            get { return _theShow; }
            set
            {
                Set(ref _theShow, value);
            }
        }

        private bool _isTracking;
        public bool IsTracking
        {
            get { return _isTracking; }
            set
            {
                Set(ref _isTracking, value);
            }
        }

        private List<Season> _seasons;
        public List<Season> Seasons
        {
            get { return _seasons; }
            set
            {
                Set(ref _seasons, value);
            }
        }

        private bool _isSeasonsAvailable;
        public bool IsSeasonsAvailable
        {
            get { return _isSeasonsAvailable; }
            set 
            { 
                Set(ref _isSeasonsAvailable, value); 
            }
        }

        private List<SeasonEpisodeGroup> _episodesGroup;
        public List<SeasonEpisodeGroup> EpisodesGroup
        {
            get { return _episodesGroup; }
            set
            {
                Set(ref _episodesGroup, value);
            }
        }

        private List<Cast> _casts;
        public List<Cast> Casts
        {
            get { return _casts; }
            set
            {
                Set(ref _casts, value);
            }
        }

        private bool _isCastAvailable;
        public bool IsCastAvailable
        {
            get { return _isCastAvailable; }
            set { Set(ref _isCastAvailable, value); }
        }

        private List<Comment> _comments;
        public List<Comment> Comments
        {
            get { return _comments; }
            set
            {
                Set(ref _comments, value);
            }
        }

        private bool _isCommentsAvailable;
        public bool IsCommentsAvailable
        {
            get { return _isCommentsAvailable; }
            set { Set(ref _isCommentsAvailable, value); }
        }

        private List<Show> _similarShows;
        public List<Show> SimilarShows
        {
            get { return _similarShows; }
            set
            {
                Set(ref _similarShows, value);
            }
        }

        private bool _isSimilarShowsAvailable;
        public bool IsSimilarShowsAvailable
        {
            get { return _isSimilarShowsAvailable; }
            set { Set(ref _isSimilarShowsAvailable, value); }
        }

        //// Youtube Related
        private bool _canPlay;
        public bool CanPlay
        {
            get { return _canPlay; }
            set { Set(ref _canPlay, value); }
        }

        private string _videoPlayURL;
        public string VideoPlayURL
        {
            get { return _videoPlayURL; }
            set { Set(ref _videoPlayURL, value); }
        }


        DelegateCommand _startPlayingCommand = null;
        public DelegateCommand StartPlayingCommand
        {
            get
            {
                if (_startPlayingCommand != null)
                    return _startPlayingCommand;

                _startPlayingCommand = new DelegateCommand
                (
                    () =>
                    {
                        StartPlaying();
                    }
                );

                return _startPlayingCommand;
            }
        }

        DelegateCommand _startTrackingShow2Command = null;
        public DelegateCommand StartTrackingShow2Command
        {
            get
            {
                if (_startTrackingShow2Command != null)
                    return _startTrackingShow2Command;

                _startTrackingShow2Command = new DelegateCommand
                (
                    () =>
                    {
                        StartTrackingShow(this.TheShow);
                        this.IsTracking = true;
                    }
                );

                return _startTrackingShow2Command;
            }
        }

        DelegateCommand _visitIMDBPage = null;
        public DelegateCommand VisitIMDBPage
        {
            get
            {
                if (_visitIMDBPage != null)
                    return _visitIMDBPage;

                _visitIMDBPage = new DelegateCommand
                (
                    () =>
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.imdb.com/title/" + this.TheShow.ids.imdb));
                    }
                );

                return _visitIMDBPage;
            }
        }

        DelegateCommand _visitTraktPage = null;
        public DelegateCommand VisitTraktPage
        {
            get
            {
                if (_visitTraktPage != null)
                    return _visitTraktPage;

                _visitTraktPage = new DelegateCommand
                (
                    () =>
                    {
                        Windows.System.Launcher.LaunchUriAsync(new Uri("http://trakt.tv/shows/" + this.TheShow.ids.slug));
                    }
                );

                return _visitTraktPage;
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

        DelegateCommand<Episode> _markEpisodeUnwatchedCommand = null;
        public DelegateCommand<Episode> MarkEpisodeUnwatchedCommand
        {
            get
            {
                if (_markEpisodeUnwatchedCommand != null)
                    return _markEpisodeUnwatchedCommand;

                _markEpisodeUnwatchedCommand = new DelegateCommand<Episode>
                (
                    (args) =>
                    {
                        MarkEpisodeUnWatched(args);
                    }
                );

                return _markEpisodeUnwatchedCommand;
            }
        }


        DelegateCommand<Episode> _markEpisodeWatchedCommand = null;
        public DelegateCommand<Episode> MarkEpisodeWatchedCommand
        {
            get
            {
                if (_markEpisodeWatchedCommand != null)
                    return _markEpisodeWatchedCommand;

                _markEpisodeWatchedCommand = new DelegateCommand<Episode>
                (
                    (args) =>
                    {
                        MarkEpisodeWatched(args);
                    }
                );

                return _markEpisodeWatchedCommand;
            }
        }



        public ShowDetailsPageViewModel()
        {
        }

        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode)
        {
            await LoadShowDetails(parameter.ToString());
        }

        private async Task LoadShowDetails(string traktID)
        {
            this.LoadingText = "HOLD ON ..." + Environment.NewLine + "GRABBING SHOW INFORMATION";
            this.DataLoaded = false;
            this.VideoPlayURL = "";
            this.CanPlay = false;
            this.IsCastAvailable = false;
            this.IsSeasonsAvailable = false;
            this.IsCommentsAvailable = false;
            this.IsSimilarShowsAvailable = false;

            try
            {
                //Load Tracked Lists
                await SQLiteService.Instance.LoadTrackingShowsList();
                await SQLiteService.Instance.LoadTrackedEpisodesList();


                //Load Show Details
                TheShow = await AppServiceHelper.GetShowDetails(_appService, Convert.ToInt32(traktID), false);
                this.IsTracking = TheShow.IsTracking;

                //Load Seasons and Episodes
                Seasons = TheShow.AllSeasons.Where(_s => _s.number > 0).OrderByDescending(_s => _s.number).ToList();
                IsSeasonsAvailable = (Seasons.Count > 0);

                var grouped = from season in Seasons
                              group season by season.SeasonName
                                  into grp
                              select new SeasonEpisodeGroup
                              {
                                  Key = grp.Key,
                                  Season = grp.First(),
                                  Episodes = grp.First().AllEpisodes.Where(_e => _e.HasAired).ToList()
                              };

                EpisodesGroup = grouped.ToList().Where(_s => _s.Episodes.Count > 0).ToList();

                this.DataLoaded = true;


                //Get Related Shows
                try
                {
                    SimilarShows = await _appService.GetAsync<List<Show>>("shows/" + traktID + "/related", "limit=60", 1000);
                }
                catch
                {
                    SimilarShows = new List<Show>();
                }

                //IsTracking Info
                foreach (var _show in SimilarShows)
                {
                    _show.IsTracking = AppSettings.TrackingList.Where(s => s.ShowTraktID == _show.ids.trakt).Any();
                }


                IsSimilarShowsAvailable = (SimilarShows.Count > 0);

                //Load Comments
                try
                {
                    Comments = await _appService.GetAsync<List<Comment>>("shows/" + traktID + "/comments", "extended=full,images&limit=160", 96);
                }
                catch
                {
                    Comments = new List<Comment>();
                }

                IsCommentsAvailable = (Comments.Count > 0);

                //Load Cast
                var castResult = await _appService.GetAsync<CastResult>("shows/" + traktID + "/people", "extended=full", 480);
                Casts = castResult.cast;

                IsCastAvailable = (Casts.Count > 0);

                //Get Cast Images
                if (IsCastAvailable)
                {
                    var tmdbService = new TMDBRestService();

                    foreach (var _cast in Casts)
                    {
                        try
                        {
                            var castImages = await tmdbService.GetAsync<CastImage>("person/" + _cast.person.ids.tmdb + "/images");
                            if (castImages != null)
                            {
                                if (castImages.profiles.Count > 0)
                                    _cast.CastImage = "http://image.tmdb.org/t/p/w185" + castImages.profiles.First().file_path;
                            }
                        }
                        catch
                        {
                        }
                    }
                }

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

            //Load Video URL for Trailer
            try
            {
                LoadVideoPlayURL();
            }
            catch
            {
            }

            await Task.CompletedTask;
        }

        private async Task LoadVideoPlayURL()
        {
            //Get the Video Download Links
            if (!string.IsNullOrEmpty(TheShow.trailer))
            {
                var tubeHelper = new YoutubeHelper();
                var downloadLinks = await tubeHelper.GetDownloadLinks(TheShow.trailer);

                if (downloadLinks.Count() != 0)
                {
                    //Get VideoPlayURL
                    var _playableQualities = downloadLinks.Where(_vq => _vq.VideoAudioQuality == "MP4 720P (HD)").OrderBy(_t => _t.Itag).ToList();

                    if (_playableQualities.Count() > 0)
                    {
                        this.VideoPlayURL = _playableQualities.First().DownloadURL;
                    }
                    else
                    {
                        _playableQualities = downloadLinks.Where(_vq => _vq.VideoAudioQuality == "MP4 360P").OrderBy(_t => _t.Itag).ToList();

                        if (_playableQualities.Count() > 0)
                        {
                            this.VideoPlayURL = _playableQualities.First().DownloadURL;
                        }
                        else
                        {
                            _playableQualities = downloadLinks.Where(_vq => _vq.VideoAudioQuality.Contains("MP4")).OrderBy(_t => _t.Itag).ToList();

                            if (_playableQualities.Count() > 0)
                            {
                                this.VideoPlayURL = _playableQualities.First().DownloadURL;
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.VideoPlayURL))
                this.CanPlay = true;

            await Task.CompletedTask;
        }

        private async Task StartPlaying()
        {
            if (!string.IsNullOrEmpty(this.VideoPlayURL))
            {
                await NavigationService.NavigateAsync(typeof(Views.TrailerPlayPage), this.VideoPlayURL);
            }

            await Task.CompletedTask;
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

        private async Task MarkEpisodeUnWatched(Episode theEpisode)
        {
            //Check to confirm first
            var _retStatus = true;

            var yesUICommand2 = new UICommand("Yes", (command) =>
            {
                _retStatus = true;
            });
            var noUICommand2 = new UICommand("No", (command) =>
            {
                _retStatus = false;
            });

            await DialogService.ShowAsync("Are you sure you want to mark this episode as Un-Watched?", theEpisode.ShowName, yesUICommand2, noUICommand2);

            if (_retStatus)
            {
                List<TrackedEpisode> episodesToTrack = new List<TrackedEpisode>();

                var allEpisodes = new List<Episode>();

                Seasons.ForEach(_s => { allEpisodes.AddRange(_s.AllEpisodes); });

                var futureEpisodes = allEpisodes.Where(_e => _e.IsWatched && ((_e.season == theEpisode.season && _e.number >= theEpisode.number) || _e.season > theEpisode.season)).ToList();

                if (futureEpisodes.Count > 1)
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

                    await DialogService.ShowAsync("Mark all Future Episodes of this show as Un-Watched as well?", theEpisode.ShowName, yesUICommand, noUICommand);

                    if (_returnStatus)
                    {
                        //Mark all future
                        foreach (var _episodeToMark in futureEpisodes)
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
                await ShowTrackingHelper.MarkEpisodesUnWatched(episodesToTrack);


                foreach (var _episode in episodesToTrack)
                {
                    allEpisodes.Where(_e => _e.ids.trakt == _episode.EpisodeTraktID).First().IsWatched = false;
                }

            }


            await Task.CompletedTask;
        }

        private async Task MarkEpisodeWatched(Episode theEpisode)
        {
            //Check if tracking this show
            if (this.TheShow.IsTracking)
            {
                List<TrackedEpisode> episodesToTrack = new List<TrackedEpisode>();

                var allEpisodes = new List<Episode>();

                Seasons.ForEach(_s => { allEpisodes.AddRange(_s.AllEpisodes); });

                var previousEpisodes = allEpisodes.Where(_e => _e.HasAired && !_e.IsWatched && _e.season > 0 && ((_e.season == theEpisode.season && _e.number <= theEpisode.number) || _e.season < theEpisode.season)).ToList();

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

                    await DialogService.ShowAsync("Mark all Previous Episodes of this show as Watched?", theEpisode.ShowName, yesUICommand, noUICommand);

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

                foreach (var _episode in episodesToTrack)
                {
                    allEpisodes.Where(_e => _e.ids.trakt == _episode.EpisodeTraktID).First().IsWatched = true;
                }
            }
            else
            {
                await DialogService.ShowAsync("You must add this show to your watchlist before marking episodes as watched.", "ERROR");
            }

            await Task.CompletedTask;
        }




    }
}
