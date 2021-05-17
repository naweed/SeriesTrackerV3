using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using XGENO.SeriesTracker.DataModels;
using System.IO;

namespace XGENO.SeriesTracker.Services
{
    public class SQLiteService
    {
        private static SQLiteService _instance;
        public static SQLiteService Instance => _instance ?? (_instance = new SQLiteService());

        private const string DatabaseName = "seriestracker.db.sqlite";
        private readonly string _databasePath;
        private static SQLite.Net.Async.SQLiteAsyncConnection _asynConnection;

        public SQLiteService()
        {
            //determine the path of the sqlite database file
            _databasePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, DatabaseName);

            //populate the database file by creating tables for each model (if needed)
            using (var conn = GetConnection())
            {
                conn.CreateTable<TrackingShow>();
                conn.CreateTable<TrackedEpisode>();
                conn.CreateTable<NotifiedEpisode>();
                conn.CreateTable<JSONDataCache>();
            }
        }

        public SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(new SQLitePlatformWinRT(), _databasePath);
        }

        public SQLite.Net.Async.SQLiteAsyncConnection GetAsyncConnection()
        {
            if (_asynConnection == null)
                _asynConnection = new SQLite.Net.Async.SQLiteAsyncConnection(() => new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(_databasePath, true)));

            return _asynConnection;
        }

        public async Task InsertOrUpdate<T>(List<T> objList)
        {

            var conn = GetAsyncConnection();

            await conn.InsertOrReplaceAllAsync(objList);

            await Task.CompletedTask;
        }

        public async Task Delete<T>(T obj)
        {
            var conn = GetAsyncConnection();

            await conn.DeleteAsync(obj);

            await Task.CompletedTask;
        }

        public async Task DeleteList<T>(List<T> objList)
        {
            var conn = GetAsyncConnection();

            foreach(var _value in objList)
                await conn.DeleteAsync(_value);

            await Task.CompletedTask;
        }


        public async Task<JSONDataCache> GetCacheObject(string cacheKey)
        {
            var conn = GetAsyncConnection();
            var qResult = conn.Table<JSONDataCache>().Where(v => v.Key == cacheKey);
            var cachObjs = await qResult.ToListAsync();

            return cachObjs.FirstOrDefault();
        }

        public async Task<List<JSONDataCache>> GetAllCachedObjects()
        {
            var conn = GetAsyncConnection();
            var qResult = conn.Table<JSONDataCache>();
            var lstObjs = await qResult.ToListAsync();

            return lstObjs;
        }

        public async Task SaveDataCache(JSONDataCache cacheItem)
        {
            var conn = GetAsyncConnection();

            await conn.InsertOrReplaceAsync(cacheItem);

            await Task.CompletedTask;
        }

        public async Task ClearDataCache()
        {
            var conn = GetAsyncConnection();

            await conn.DeleteAllAsync<JSONDataCache>();

            await Task.CompletedTask;
        }

        public async Task LoadTrackingShowsList()
        {
            AppSettings.TrackingList = await GetTrackingShowsList();

            await Task.CompletedTask;
        }

        private async Task<List<TrackingShow>> GetTrackingShowsList()
        {
            var conn = GetAsyncConnection();
            var qResult = conn.Table<TrackingShow>();
            var lstObjs = await qResult.ToListAsync();

            return lstObjs;
        }

        public async Task LoadTrackedEpisodesList()
        {
            AppSettings.TrackedEpisodeList = await GetAllTrackedEpisode();

            await Task.CompletedTask;
        }

        public async Task<List<TrackedEpisode>> GetAllTrackedEpisode()
        {
            var conn = GetAsyncConnection();
            var qResult = conn.Table<TrackedEpisode>();
            var lstObjs = await qResult.ToListAsync();

            return lstObjs;
        }
        
        public async Task LoadNotifiedEpisodesList()
        {
            AppSettings.NotifiedEpisodeList = await GetAllNotifiedEpisode();

            await Task.CompletedTask;
        }
        
        public async Task<List<NotifiedEpisode>> GetAllNotifiedEpisode()
        {
            var conn = GetAsyncConnection();
            var qResult = conn.Table<NotifiedEpisode>();
            var lstObjs = await qResult.ToListAsync();

            return lstObjs;
        }

        public async Task StartTrackingShow(Show thisShow)
        {
            await SQLiteService.Instance.LoadTrackingShowsList();

            if (!AppSettings.TrackingList.Where(s => s.ShowTraktID == thisShow.ids.trakt).Any())
            {
                AppSettings.TrackingList.Add(new TrackingShow() { ShowTraktID = thisShow.ids.trakt, AddedOn = DateTime.Now });

                await InsertOrUpdate<TrackingShow>(AppSettings.TrackingList);
            }

            await Task.CompletedTask;
        }

        public async Task StopTrackingShow(Show thisShow)
        {
            await SQLiteService.Instance.LoadTrackingShowsList();

            if (AppSettings.TrackingList.Where(s => s.ShowTraktID == thisShow.ids.trakt).Any())
            {
                var theShowToDelete = AppSettings.TrackingList.Where(s => s.ShowTraktID == thisShow.ids.trakt).First();

                await Delete<TrackingShow>(theShowToDelete);

                AppSettings.TrackingList.RemoveAll(_s => _s.ShowTraktID == thisShow.ids.trakt);
            }

            await Task.CompletedTask;
        }
        
        public async Task UnArchiveShow(int showID)
        {
            await SQLiteService.Instance.LoadTrackingShowsList();

            AppSettings.TrackingList.Where(_s => _s.ShowTraktID == showID).First().IsArchived = false;

            await InsertOrUpdate<TrackingShow>(AppSettings.TrackingList);

            await Task.CompletedTask;
        }

        public async Task ArchiveShow(int showID)
        {
            await SQLiteService.Instance.LoadTrackingShowsList();

            AppSettings.TrackingList.Where(_s => _s.ShowTraktID == showID).First().IsArchived = true;

            await InsertOrUpdate<TrackingShow>(AppSettings.TrackingList);

            await Task.CompletedTask;
        }
        
        public async Task MarkEpisodesWatched(List<TrackedEpisode> episodeList)
        {
            await SQLiteService.Instance.LoadTrackedEpisodesList();

            bool storageRequired = false;

            foreach (var _episode in episodeList)
            {
                if (!AppSettings.TrackedEpisodeList.Where(s => s.EpisodeTraktID == _episode.EpisodeTraktID).Any())
                {
                    _episode.WatchedOn = DateTime.Now;
                    AppSettings.TrackedEpisodeList.Add(_episode);
                    storageRequired = true;
                }
            }

            if (storageRequired)
            {
                await InsertOrUpdate<TrackedEpisode>(AppSettings.TrackedEpisodeList);
            }

            await Task.CompletedTask;
        }

        public async Task MarkEpisodesUnWatched(List<TrackedEpisode> episodeList)
        {
            await SQLiteService.Instance.LoadTrackedEpisodesList();

            foreach (var _episode in episodeList)
            {
                AppSettings.TrackedEpisodeList.RemoveAll(_e => _e.EpisodeTraktID == _episode.EpisodeTraktID);
            }

            await DeleteList(episodeList);

            await Task.CompletedTask;
        }

        public async Task MarkEpisodesNotified(List<NotifiedEpisode> episodeList)
        {
            await SQLiteService.Instance.LoadNotifiedEpisodesList();

            bool storageRequired = false;

            foreach (var _episode in episodeList)
            {
                if (!AppSettings.NotifiedEpisodeList.Where(s => s.EpisodeTraktID == _episode.EpisodeTraktID).Any())
                {
                    AppSettings.NotifiedEpisodeList.Add(_episode);
                    storageRequired = true;
                }
            }

            if (storageRequired)
            {
                await InsertOrUpdate<NotifiedEpisode>(AppSettings.NotifiedEpisodeList);
            }

            await Task.CompletedTask;
        }

        public async Task RestoreDataSet(bool showListUpdated, bool episodeListUpdated, bool notifiedListUpdated)
        {
            if (showListUpdated)
            {
                await InsertOrUpdate<TrackingShow>(AppSettings.TrackingList);
            }

            if (episodeListUpdated)
            {
                await InsertOrUpdate<TrackedEpisode>(AppSettings.TrackedEpisodeList);
            }

            if (notifiedListUpdated)
            {
                await InsertOrUpdate<NotifiedEpisode>(AppSettings.NotifiedEpisodeList);
            }

            await Task.CompletedTask;
        }

    }
}
