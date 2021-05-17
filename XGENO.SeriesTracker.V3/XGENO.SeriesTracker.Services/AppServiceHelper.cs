using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGENO.Framework.Extensions;
using XGENO.Framework.Helpers;
using XGENO.SeriesTracker.DataModels;

namespace XGENO.SeriesTracker.Services
{
    public static class AppServiceHelper
    {
        public static async Task<List<Show>> GetMyTrackedShows(RestService _appService, bool _isBackgroundServiceCall = false, bool _includeArchived = false, bool _hideShowsWithNoUpcomingInfo = false, bool _allShows = false)
        {
            //Load Tracked Lists
            await SQLiteService.Instance.LoadTrackingShowsList();
            await SQLiteService.Instance.LoadTrackedEpisodesList();

            var showsList = new List<Show>();

            //Get tracked shows details
            foreach (var _show in AppSettings.TrackingList)
            {
                bool includeInList = _allShows || (!_allShows && (_includeArchived == _show.IsArchived));

                if (includeInList)
                {
                    //Get Show Info
                    try
                    {
                        var thisShow = await GetShowDetails(_appService, _show.ShowTraktID, _isBackgroundServiceCall);
                        thisShow.SetHideShowsWithFutureInformation(_hideShowsWithNoUpcomingInfo);
                        thisShow.CalculateMissedEpisodesCount();
                        showsList.Add(thisShow);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            //Return the list
            return showsList;
        }

        public static async Task<Show> GetShowDetails(RestService _appService, int showID, bool _isBackgroundServiceCall = false)
        {
            //Get Show Info
            var thisShow = await _appService.GetAsync<Show>("shows/" + showID.ToString(), "extended=full", (_isBackgroundServiceCall? 96 : 144));

            if (AppSettings.TrackingList.Where(_s => _s.ShowTraktID == showID).Any())
                thisShow.IsTracking = true;

            if (AppSettings.TrackingList.Where(_s => _s.ShowTraktID == showID && _s.IsArchived).Any())
                thisShow.IsArchived = true;

            //Get Seasons List
            if (_isBackgroundServiceCall && (thisShow.ShowStatus == "ENDED" || thisShow.ShowStatus == "CANCELED") && thisShow.AiredEpisodesCount == thisShow.WatchedEpisodesCount)
            {
                //No need to get the Seasons/Episodes for ended or fully watched shows 
            }
            else
            {
                //Add seasons and episodes to the show
                await AddSeasonsAndEpisodesToShow(_appService, thisShow, _isBackgroundServiceCall);
            }

            return thisShow;
        }

        private static async Task AddSeasonsAndEpisodesToShow(RestService _appService, Show theShow, bool _isBackgroundServiceCall)
        {
            var seasonsList = await _appService.GetAsync<List<Season>>("shows/" + theShow.ids.trakt.ToString() + "/seasons", "extended=full,episodes", (_isBackgroundServiceCall ? 120 : 192));

            //Remove all Empty Seasons
            seasonsList.RemoveAll(_s => _s.EpisodeCount == 0 || _s.number == 0);

            //Set Episode Info
            seasonsList.ForEach(
                _s =>
                {
                    _s.episodes.ForEach(
                        _e =>
                        {
                            _e.SetEpisodeInfo(theShow.airs, theShow.ShowTitle, theShow.ids.trakt, _s.ids.trakt, theShow.ids.tvdb, theShow.BackgroundImage, theShow.PosterImage, theShow.EpisodeLengthInMins);
                        }
                        );
                    _s.SetSeasonInfo();
                }
                );


            //Add all seasons && episodes
            seasonsList.ForEach(
                _s =>
                    {
                        theShow.AllSeasons.Add(_s);
                        theShow.AllEpisodes.AddRange(_s.episodes);
                    }
                );


            await Task.CompletedTask;
        }

        public static async Task<bool> SyncTraktProgress(AppSettings _appSettings)
        {
            bool syncComplete = false;

            //No access token, return
            if (string.IsNullOrEmpty(_appSettings.TraktAccessToken))
                return false;

            bool showListUpdated = false;
            bool episodeListUpdated = false;

            //Start the Sync Process
            try
            {
                //Load Tracked Lists
                await SQLiteService.Instance.LoadTrackingShowsList();
                await SQLiteService.Instance.LoadTrackedEpisodesList();

                RestService traktService = await GetTraktService(_appSettings);

                //Get list of shows in Trakt Watchlist
                var traktWatchlistShowsExtract = await traktService.GetAsync<List<SyncShowWatchlist>>("sync/watchlist/shows", "", 0);

                //Get list of Episodes Wacthed in Trakt
                var traktWatchedEpisodesExtract = await traktService.GetAsync<List<SyncShowPlays>>("sync/watched/shows", "", 0);

                //Add Trakt Watchlist Shows to App TrackingList
                if (traktWatchlistShowsExtract != null)
                {
                    foreach (var _showItem in traktWatchlistShowsExtract)
                    {
                        if (_showItem.show.title != null && _showItem.show.ids != null)
                        {
                            if (!AppSettings.TrackingList.Where(_s => _s.ShowTraktID == _showItem.show.ids.trakt).Any())
                            {
                                AppSettings.TrackingList.Add(new TrackingShow() { ShowTraktID = _showItem.show.ids.trakt, AddedOn = DateTime.Now });
                                showListUpdated = true;
                            }
                        }
                    }
                }

                if (traktWatchedEpisodesExtract != null)
                {
                    foreach (var _showItem in traktWatchedEpisodesExtract)
                    {
                        if (_showItem.show.title != null && _showItem.show.ids != null)
                        {
                            if (!AppSettings.TrackingList.Where(_s => _s.ShowTraktID == _showItem.show.ids.trakt).Any())
                            {
                                AppSettings.TrackingList.Add(new TrackingShow() { ShowTraktID = _showItem.show.ids.trakt, AddedOn = DateTime.Now });
                                showListUpdated = true;
                            }
                        }
                    }
                }



                //Add Trakt Watched Episodes to App TrackedEpisodeList
                //First find list of all unique seasons
                var uniqueSeasons = new List<SyncDummySeason>();
                var allEpisodes = new List<SyncDummyEpisode>();

                foreach (var _showItem in traktWatchedEpisodesExtract)
                {
                    if (_showItem.show.title != null && _showItem.show.ids != null)
                    {
                        if (_showItem.seasons != null)
                        {
                            foreach (var _seasonItem in _showItem.seasons)
                            {
                                if (_seasonItem.episodes != null)
                                {
                                    foreach (var _episodeItem in _seasonItem.episodes)
                                    {
                                        if (!uniqueSeasons.Where(_s => _s.ShowTraktID == _showItem.show.ids.trakt && _s.SeasonNo == _seasonItem.number).Any())
                                        {
                                            uniqueSeasons.Add(new SyncDummySeason() { SeasonNo = _seasonItem.number, ShowTraktID = _showItem.show.ids.trakt });
                                        }

                                        allEpisodes.Add(new SyncDummyEpisode() { SeasonNo = _seasonItem.number, ShowTraktID = _showItem.show.ids.trakt, EpisodeNo = _episodeItem.number, WatchedOn = _episodeItem.last_watched_at });
                                    }
                                }
                            }
                        }
                    }
                }

                //Check if the Season Ids exist (if so update)
                foreach (var _season in uniqueSeasons)
                {
                    try
                    {
                        var _tmpEpisodesList = AppSettings.TrackedEpisodeList.Where(_e => _e.SeasonNo == _season.SeasonNo && _e.ShowTraktID == _season.ShowTraktID).ToList();
                        if (_tmpEpisodesList.Count > 0)
                        {
                            _season.SeasonTraktID = _tmpEpisodesList.First().SeasonTraktID;
                        }
                    }
                    catch
                    {
                    }
                }

                //Get the Season Ids not in current list
                foreach (var _seasonItem in uniqueSeasons.Where(_s => _s.SeasonTraktID == 0))
                {
                    try
                    {
                        var theSeasonList = await traktService.GetAsync<List<Season>>("shows/" + _seasonItem.ShowTraktID.ToString() + "/seasons", "", 0);

                        _seasonItem.SeasonTraktID = theSeasonList.Where(_s => _s.number == _seasonItem.SeasonNo).First().ids.trakt;
                    }
                    catch
                    {
                    }
                }

                //Remove Seasons with no ID
                uniqueSeasons.RemoveAll(_s => _s.SeasonTraktID == 0);

                //Check if the Episode Ids exist (if so update)
                foreach (var _episode in allEpisodes)
                {
                    var _tmpEpisodeList = AppSettings.TrackedEpisodeList.Where(_e => _e.SeasonNo == _episode.SeasonNo && _e.EpisodeNo == _episode.EpisodeNo && _e.ShowTraktID == _episode.ShowTraktID).ToList();

                    if (_tmpEpisodeList.Count > 0)
                    {
                        _episode.EpisodeTraktID = _tmpEpisodeList.First().EpisodeTraktID;
                    }

                    _episode.SeasonTraktID = uniqueSeasons.Where(_s => _s.SeasonNo == _episode.SeasonNo && _s.ShowTraktID == _episode.ShowTraktID).First().SeasonTraktID;
                }

                //Remove Seasons with no ID
                allEpisodes.RemoveAll(_s => _s.SeasonTraktID == 0);


                Dictionary<string, List<Episode>> dctEpisodes = new Dictionary<string, List<Episode>>();

                //Get the Episode Ids not in current list
                foreach (var _episode in allEpisodes.Where(_e => _e.EpisodeTraktID == 0))
                {
                    if (!dctEpisodes.ContainsKey(_episode.ShowTraktID.ToString() + "_" + _episode.SeasonTraktID.ToString()))
                    {
                        try
                        {
                            var theEpisodeList = await traktService.GetAsync<List<Episode>>("shows/" + _episode.ShowTraktID.ToString() + "/seasons/" + _episode.SeasonNo.ToString(), "", 0);

                            dctEpisodes.Add(_episode.ShowTraktID.ToString() + "_" + _episode.SeasonTraktID.ToString(), theEpisodeList);
                        }
                        catch
                        {
                        }
                    }

                    try
                    {
                        _episode.EpisodeTraktID = dctEpisodes[_episode.ShowTraktID.ToString() + "_" + _episode.SeasonTraktID.ToString()].Where(_e => _episode.EpisodeNo == _e.number).First().ids.trakt;
                    }
                    catch
                    {
                    }
                }

                //Remove any episodes with no EpisodeTraktID  or SeasonTraktID    
                allEpisodes.RemoveAll(_e => _e.EpisodeTraktID == 0 || _e.SeasonTraktID == 0);

                //Now add Trakt Watched Episodes to App TrackedEpisodeList
                foreach (var _episode in allEpisodes)
                {
                    if (!AppSettings.TrackedEpisodeList.Where(_e => _e.EpisodeTraktID == _episode.EpisodeTraktID).Any())
                    {
                        AppSettings.TrackedEpisodeList.Add(new TrackedEpisode() { EpisodeNo = _episode.EpisodeNo, EpisodeTraktID = _episode.EpisodeTraktID, SeasonNo = _episode.SeasonNo, SeasonTraktID = _episode.SeasonTraktID, ShowTraktID = _episode.ShowTraktID, WatchedOn = Convert.ToDateTime(_episode.WatchedOn) });
                        episodeListUpdated = true;
                    }
                }


                //Save the local tracking lists
                await SQLiteService.Instance.RestoreDataSet(showListUpdated, episodeListUpdated, false);


                //Upload App TrackingList to Trakt
                var showsToUpload = new SyncShows();
                showsToUpload.shows = new List<SyncShow>();

                foreach (var _show in AppSettings.TrackingList)
                {
                    if (!traktWatchlistShowsExtract.Where(_s => _s.show.title != null && _s.show.ids != null).Where(_e => _e.show.ids.trakt == _show.ShowTraktID).Any())
                    {
                        var theShow = new SyncShow();

                        theShow.ids = new SyncShowIds();
                        theShow.ids.trakt = _show.ShowTraktID;

                        if (_show.ShowTraktID != 0)
                            showsToUpload.shows.Add(theShow);
                    }

                }

                if (showsToUpload.shows.Count > 0)
                {
                    await traktService.PostAsync<SyncShows>("sync/watchlist", showsToUpload);
                }

                //Upload App TrackedEpisodeList to Trakt
                var episodesToUpload = new SyncWatchedEpisodes();
                episodesToUpload.episodes = new List<SyncWatchedEpisode>();

                foreach (var _episode in AppSettings.TrackedEpisodeList)
                {
                    if (!allEpisodes.Where(_e => _e.EpisodeTraktID == _episode.EpisodeTraktID).Any())
                    {
                        var theEpisode = new SyncWatchedEpisode();

                        theEpisode.ids = new SyncIds();
                        theEpisode.watched_at = _episode.WatchedOn.ToUniversalTime().ToUtcJSONDateTimeString();
                        theEpisode.ids.trakt = _episode.EpisodeTraktID;

                        if (_episode.EpisodeTraktID != 0)
                            episodesToUpload.episodes.Add(theEpisode);
                    }
                }

                if (episodesToUpload.episodes.Count > 0)
                {
                    await traktService.PostAsync<SyncWatchedEpisodes>("sync/history", episodesToUpload);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


            return syncComplete;
        }

        public static async Task<RestService> GetTraktService(AppSettings _appSettings)
        {
            var traktService = new RestService(false, _appSettings);

            //Check if refresh token is required
            if (DateTime.Now.AddDays(3) > _appSettings.TraktTokenExpiresOn)
            {
                //Get new Tokens

                //Request Token
                var oAuthPostData = new SyncOAuthRefreshTokenPostData();
                oAuthPostData.client_id = AppSettings.TraktSyncClientID;
                oAuthPostData.client_secret = AppSettings.TraktSyncClientSecret;
                oAuthPostData.refresh_token = _appSettings.TraktRefreshToken;
                oAuthPostData.grant_type = "refresh_token";
                oAuthPostData.redirect_uri = "https://xgeno.com";

                RestService refreshService = new RestService(true);
                var tokenResponse = await traktService.PostAsync<SyncOAuthRefreshTokenPostData>("oauth/token", oAuthPostData);

                var tokenResponseString = await tokenResponse.Content.ReadAsStringAsync();

                var tokenData = SerializationHelper.Deserialize<SyncTokenResponse>(tokenResponseString);

                _appSettings.TraktAccessToken = tokenData.access_token;
                _appSettings.TraktRefreshToken = tokenData.refresh_token;
                _appSettings.TraktTokenExpiresOn = tokenData.ExpiresOn;
            }

            return traktService;
        }

    }


}
