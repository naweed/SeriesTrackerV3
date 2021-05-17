using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using XGENO.Framework.Services;
using XGENO.SeriesTracker.DataModels;
using XGENO.SeriesTracker.Services;
using NotificationsExtensions.Tiles;
using XGENO.Framework.Helpers;
using XGENO.Framework.Extensions;


namespace XGENO.SeriesTracker.BackgroundTasks
{
    public sealed class SeriesBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get a deferral, to prevent the task from closing prematurely while asynchronous code is still running
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            try
            {
                //Initialize Settings
                var appSettings = new AppSettings();
                await appSettings.LoadSettings();


                //Load Tracked Lists
                await SQLiteService.Instance.LoadTrackingShowsList();
                await SQLiteService.Instance.LoadTrackedEpisodesList();
                await SQLiteService.Instance.LoadNotifiedEpisodesList();

                //Get the Shows
                RestService _appService = new RestService();

                var shows = await AppServiceHelper.GetMyTrackedShows(_appService, true);

                //Sort by Order
                shows = shows.OrderBy(_s => _s.SortOrder).ThenBy(_s => _s.ShowTitle).ToList();

                // Update the live tile with the feed items.
                if (appSettings.LiveTilesEnabled && AppSettings.TrackingList.Count > 0)
                {
                    await UpdateTile(shows);
                }

                // Update on aired episode date
                if (appSettings.AiredNotificationsEnabled && AppSettings.TrackingList.Count > 0)
                {
                    await NotifyTodaysEpisodes(shows);
                }

            }
            catch
            {
            }

            // Inform the system that the task is finished.
            deferral.Complete();
        }

        private static async Task UpdateTile(List<Show> shows)
        {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.EnableNotificationQueue(true);
            updater.Clear();

            //Get the top 5 shows
            //var showsToDisplay = shows.Where(_s => { var _status = _s.ShowTrackingStatus; return !(_status.StartsWith("ALL CAUGHT UP") && (_status.EndsWith("DATE TBA") || _status.EndsWith("THE SHOW HAS ENDED"))); }).Randomize().Take(5).ToList();
            var showsToDisplay = shows.Take(5).ToList();

            // Create a tile notification for each feed item.
            foreach (var item in showsToDisplay)
            {
                string cachedImageSource = "";

                try
                {
                    cachedImageSource = await WebCacheHelper.GetWebImage(item.BackgroundImage, null);
                }
                catch (Exception ex)
                {
                    cachedImageSource = item.BackgroundImage;
                }

                var theStatus = (item.MissedEpisodeCount == 0 ? item.NextEpisodeInfo : item.MissedEpisodeCount.ToString() + " MISSED");

                TileBindingContentAdaptive bindingContent = new TileBindingContentAdaptive()
                {
                    PeekImage = new TilePeekImage()
                    {
                        Source = cachedImageSource
                    },

                    Children =
                    {
                        new TileText()
                        {
                            Text = item.ShowTitle,
                            Wrap = true,
                            Style = TileTextStyle.Body
                        },

                        new TileText()
                        {
                            Text = theStatus.Replace(" - ", Environment.NewLine).Replace(": ", Environment.NewLine).Replace("NEXT EPISODE", "AIRS ON").Replace("ALL CAUGHT UP", ""),
                            Style = TileTextStyle.CaptionSubtle
                        }
                    }
                };

                TileBinding binding = new TileBinding()
                {
                    Branding = TileBranding.Logo,
                    DisplayName = "Series Tracker",
                    Content = bindingContent
                };


                TileContent content = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        TileMedium = binding,
                        TileWide = binding,
                        TileLarge = binding
                    }
                };

                // Create a new tile notification. 
                updater.Update(new TileNotification(content.GetXml()));
            }

            await Task.CompletedTask;
        }

        private static async Task NotifyTodaysEpisodes(List<Show> shows)
        {
            var todaysEpisodes = new List<Episode>();
            var oldEpisodes = new List<Episode>();
            bool updateListRequired = false;

            foreach (var _show in shows)
            {
                foreach (var _episode in _show.AllEpisodes.Where(_e => _e.HasAired && !_e.IsNotified && !_e.IsWatched))
                {
                    if (_episode.AiredDateToCompare == Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-dd") && _episode.LocalAiredDate.Value > AppSettings.TrackingList.Where(_s => _s.ShowTraktID == _show.ids.trakt).First().AddedOn)
                    {
                        todaysEpisodes.Add(_episode);
                    }
                    else
                    {
                        oldEpisodes.Add(_episode);
                    }
                }
            }

            if (todaysEpisodes.Count > 0)
            {
                var toastService = new ToastNotificationService();

                foreach (var _episode in todaysEpisodes)
                {
                    string cachedImageSource = "";

                    try
                    {
                        cachedImageSource = await WebCacheHelper.GetWebImage(_episode.BackgroundImage, null);
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            cachedImageSource = await WebCacheHelper.GetWebImage(_episode.ShowBackgroundImage, null);
                        }
                        catch
                        {
                            cachedImageSource = _episode.ShowBackgroundImage;
                        }
                    }

                    toastService.ShowToastWithBigImage("AIRED TODAY: " + _episode.EpisodeFullName, _episode.ShowName, _episode.overview, cachedImageSource);

                }
            }

            //Update the Notified List
            var notifiedList = new List<NotifiedEpisode>();

            foreach (var _episode in todaysEpisodes)
            {
                notifiedList.Add(new NotifiedEpisode() { EpisodeTraktID = _episode.ids.trakt });
                updateListRequired = true;
            }

            foreach (var _episode in oldEpisodes)
            {
                notifiedList.Add(new NotifiedEpisode() { EpisodeTraktID = _episode.ids.trakt });
                updateListRequired = true;
            }


            //Save the new Notified list (only if needed)
            if (updateListRequired)
            {
                await SQLiteService.Instance.MarkEpisodesNotified(notifiedList);
            }

            await Task.CompletedTask;
        }

    }

}
