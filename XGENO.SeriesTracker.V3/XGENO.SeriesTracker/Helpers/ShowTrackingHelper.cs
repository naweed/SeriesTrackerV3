using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGENO.Framework.Helpers;
using XGENO.Framework.Services;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Services;

namespace XGENO.SeriesTracker.Helpers
{
    public static class ShowTrackingHelper
    {
        public static async Task StartTrackingShow(Show theShow)
        {
            var dialogService = new DialogService();
            var toastService = new ToastNotificationService();

            var appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;


            if (!appSettings.ApplicationPurchased && AppSettings.TrackingList.Count >= 25)
            {
                await dialogService.ShowAsync("You need to UNLOCK the application to track more than 25 shows.", "FULL VERSION");
                return;
            }

            //Update the status as Tracked
            theShow.IsTracking = true;

            await SQLiteService.Instance.StartTrackingShow(theShow);

            if (appSettings.GeneralNotificationsEnabled)
            {
                string cachedImageSource = "";

                try
                {
                    cachedImageSource = await WebCacheHelper.GetWebImage(theShow.PosterImage, null);
                }
                catch (Exception ex)
                {
                    cachedImageSource = theShow.PosterImage;
                }

                toastService.ShowToastWithImage("Added to the watchlist.", theShow.ShowTitle, cachedImageSource);
            }

            //Add the show to Trakt (if syncing)
            if (!string.IsNullOrEmpty(appSettings.TraktAccessToken))
            {
                try
                {
                    var showsToUpload = new SyncShows();
                    showsToUpload.shows = new List<SyncShow>();

                    var _traktShow = new SyncShow();
                    _traktShow.ids = new SyncShowIds();
                    _traktShow.ids.trakt = theShow.ids.trakt;

                    showsToUpload.shows.Add(_traktShow);

                    var traktService = await AppServiceHelper.GetTraktService(appSettings);
                    /*await*/
                    traktService.PostAsync<SyncShows>("sync/watchlist", showsToUpload);
                }
                catch (Exception ex)
                {
                }
            }

            await Task.CompletedTask;
        }

        public static async Task StopTrackingShow(Show theShow)
        {
            var dialogService = new DialogService();
            var toastService = new ToastNotificationService();

            var appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;


            if (!appSettings.ApplicationPurchased)
            {
                await dialogService.ShowAsync("You need to UNLOCK the application to remove shows from your watchlist.", "FULL VERSION");
                return;
            }

            //Update the status as Removed
            theShow.IsTracking = false;

            //Stop Tracking the Show
            await SQLiteService.Instance.StopTrackingShow(theShow);

            //Remove all watched episodes
            var episodesToRemove = AppSettings.TrackedEpisodeList.Where(_e => _e.ShowTraktID == theShow.ids.trakt).ToList();
            await SQLiteService.Instance.MarkEpisodesUnWatched(episodesToRemove);

            if (appSettings.GeneralNotificationsEnabled)
            {
                string cachedImageSource = "";

                try
                {
                    cachedImageSource = await WebCacheHelper.GetWebImage(theShow.PosterImage, null);
                }
                catch (Exception ex)
                {
                    cachedImageSource = theShow.PosterImage;
                }

                toastService.ShowToastWithImage("You have successfully removed this show from your watchlist!", theShow.ShowTitle, cachedImageSource);
            }

            //Remove the show and episodes from Trakt (if syncing)
            if (!string.IsNullOrEmpty(appSettings.TraktAccessToken))
            {
                try
                {
                    var traktService = await AppServiceHelper.GetTraktService(appSettings);

                    //Remove the show
                    var showsToUpload = new SyncShows();
                    showsToUpload.shows = new List<SyncShow>();

                    var _traktShow = new SyncShow();
                    _traktShow.ids = new SyncShowIds();
                    _traktShow.ids.trakt = theShow.ids.trakt;

                    showsToUpload.shows.Add(_traktShow);

                    await traktService.PostAsync<SyncShows>("sync/watchlist/remove", showsToUpload);


                    //Remove the Episodes
                    var episodesToUpload = new SyncWatchedEpisodes();
                    episodesToUpload.episodes = new List<SyncWatchedEpisode>();

                    foreach (var _episode in episodesToRemove)
                    {
                        var theEpisode = new SyncWatchedEpisode();
                        theEpisode.ids = new SyncIds();
                        theEpisode.ids.trakt = _episode.EpisodeTraktID;

                        episodesToUpload.episodes.Add(theEpisode);
                    }

                    if (episodesToUpload.episodes.Count > 0)
                    {
                        /*await*/
                        traktService.PostAsync<SyncWatchedEpisodes>("sync/history/remove", episodesToUpload);
                    }
                }
                catch(Exception ex)
                {
                }
            }


            await Task.CompletedTask;
        }

        public static async Task MarkEpisodesWatched(List<TrackedEpisode> episodeList)
        {
            var dialogService = new DialogService();
            var toastService = new ToastNotificationService();

            var appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;


            await SQLiteService.Instance.MarkEpisodesWatched(episodeList);

            if (appSettings.GeneralNotificationsEnabled)
                toastService.ShowSimpleToast(episodeList.Count.ToString() + " episode(s) marked as watched!", "WATCHED");

            //Add the episodes to Trakt (if syncing)
            if (!string.IsNullOrEmpty(appSettings.TraktAccessToken))
            {
                try
                {
                    var episodesToUpload = new SyncWatchedEpisodes();
                    episodesToUpload.episodes = new List<SyncWatchedEpisode>();

                    foreach (var _episode in episodeList)
                    {
                        var theEpisode = new SyncWatchedEpisode();
                        theEpisode.ids = new SyncIds();
                        theEpisode.ids.trakt = _episode.EpisodeTraktID;

                        episodesToUpload.episodes.Add(theEpisode);
                    }

                    if (episodesToUpload.episodes.Count > 0)
                    {
                        var traktService = await AppServiceHelper.GetTraktService(appSettings);
                        /*await*/
                        traktService.PostAsync<SyncWatchedEpisodes>("sync/history", episodesToUpload);
                    }
                }
                catch
                {
                }
            }


            await Task.CompletedTask;
        }

        public static async Task MarkEpisodesUnWatched(List<TrackedEpisode> episodeList)
        {
            var dialogService = new DialogService();
            var toastService = new ToastNotificationService();

            var appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;


            await SQLiteService.Instance.MarkEpisodesUnWatched(episodeList);

            if (appSettings.GeneralNotificationsEnabled)
                toastService.ShowSimpleToast(episodeList.Count.ToString() + " episode(s) marked as Un-Watched!", "UN-WATCHED");


            //Remove the episodes to Trakt (if syncing)
            if (!string.IsNullOrEmpty(appSettings.TraktAccessToken))
            {
                try
                {
                    var episodesToUpload = new SyncWatchedEpisodes();
                    episodesToUpload.episodes = new List<SyncWatchedEpisode>();

                    foreach (var _episode in episodeList)
                    {
                        var theEpisode = new SyncWatchedEpisode();
                        theEpisode.ids = new SyncIds();
                        theEpisode.ids.trakt = _episode.EpisodeTraktID;

                        episodesToUpload.episodes.Add(theEpisode);
                    }

                    if (episodesToUpload.episodes.Count > 0)
                    {
                        var traktService = await AppServiceHelper.GetTraktService(appSettings);
                        /*await*/
                        traktService.PostAsync<SyncWatchedEpisodes>("sync/history/remove", episodesToUpload);
                    }
                }
                catch
                {
                }
            }


            await Task.CompletedTask;
        }


        public static async Task UnArchiveShow(Show theShow)
        {
            var dialogService = new DialogService();
            var toastService = new ToastNotificationService();

            var appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;

            //Update the Archived Status
            theShow.IsArchived = false;

            await SQLiteService.Instance.UnArchiveShow(theShow.ids.trakt);

            if (appSettings.GeneralNotificationsEnabled)
            {
                string cachedImageSource = "";

                try
                {
                    cachedImageSource = await WebCacheHelper.GetWebImage(theShow.PosterImage, null);
                }
                catch (Exception ex)
                {
                    cachedImageSource = theShow.PosterImage;
                }

                toastService.ShowToastWithImage("You have Un-Archived this show! It will now be visible in your Watchlist.", theShow.ShowTitle, cachedImageSource);
            }

            await Task.CompletedTask;
        }

        public static async Task ArchiveShow(Show theShow)
        {
            var dialogService = new DialogService();
            var toastService = new ToastNotificationService();

            var appSettings = (App.Current as XGENO.SeriesTracker.App).Settings;

            //Update the Archived Status
            theShow.IsArchived = true;

            await SQLiteService.Instance.ArchiveShow(theShow.ids.trakt);

            if (appSettings.GeneralNotificationsEnabled)
            {
                string cachedImageSource = "";

                try
                {
                    cachedImageSource = await WebCacheHelper.GetWebImage(theShow.PosterImage, null);
                }
                catch (Exception ex)
                {
                    cachedImageSource = theShow.PosterImage;
                }

                toastService.ShowToastWithImage("This show has been archived! You can recover it any time in the future.", theShow.ShowTitle, cachedImageSource);
            }

            await Task.CompletedTask;
        }

    }
}
