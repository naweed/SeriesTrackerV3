using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XGENO.Framework.Enums;
using XGENO.Framework.Helpers;
using XGENO.Framework.MVVM;
using XGENO.Framework.Services;
using SQLite.Net.Attributes;

namespace XGENO.SeriesTracker.DataModels
{
    //Local Sync Data

    public class TrackingShow
    {
        [PrimaryKey]
        public int ShowTraktID { get; set; }
        public bool IsArchived { get; set; } = false;
        public DateTime AddedOn { get; set; } = new DateTime(2016, 03, 25);
    }

    public class TrackedEpisode
    {
        [PrimaryKey]
        public int EpisodeTraktID { get; set; }
        public int ShowTraktID { get; set; }
        public int SeasonTraktID { get; set; }
        public int SeasonNo { get; set; }
        public int EpisodeNo { get; set; }

        public DateTime WatchedOn { get; set; } = new DateTime(2015, 12, 25);
    }

    public class NotifiedEpisode
    {
        [PrimaryKey]
        public int EpisodeTraktID { get; set; }
    }

    public class BackupSet
    {
        public List<TrackingShow> TrackingList { get; set; }
        public List<TrackedEpisode> TrackedEpisodeList { get; set; }
        public List<NotifiedEpisode> NotifiedEpisodeList { get; set; }

        public BackupSet()
        {
            this.TrackedEpisodeList = new List<TrackedEpisode>();
            this.TrackingList = new List<TrackingShow>();
            this.NotifiedEpisodeList = new List<NotifiedEpisode>();
        }
    }

    //Cache Data
    public class JSONDataCache
    {
        [PrimaryKey]
        public string Key { get; set; }
        public string Content { get; set; }
        public DateTime StartDate { get; set; }

        public JSONDataCache()
        {

        }
    }

    //Trakt Sync Data

    public class SyncShowPlays
    {
        public int? plays { get; set; }

        public string last_watched_at { get; set; }

        public SyncShow show { get; set; }

        public List<SyncSeason> seasons { get; set; }
    }

    public class SyncShow
    {
        public string title { get; set; }

        public int? year { get; set; }

        public SyncShowIds ids { get; set; }
    }

    public class SyncIds
    {
        public int trakt { get; set; }
    }

    public class SyncShowIds : SyncIds
    {
        public string slug { get; set; }
    }
    
    public class SyncSeason
    {
        public int number { get; set; }

        public List<SyncEpisode> episodes { get; set; }
    }

    public class SyncEpisode
    {
        public int number { get; set; }

        public int? plays { get; set; }

        public string last_watched_at { get; set; }
    }

    public class SyncShowWatchlist
    {
        public SyncShow show { get; set; }
    }

    public class SyncShows
    {
        public List<SyncShow> shows { get; set; }
    }

    public class SyncWatchedEpisodes
    {
        public List<SyncWatchedEpisode> episodes { get; set; }
    }

    public class SyncWatchedEpisode
    {
        public string watched_at { get; set; }

        public SyncIds ids { get; set; }

    }

    public class SyncOAuthPostData
    {
        public string code { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }
        public string grant_type { get; set; }

    }

    public class SyncOAuthRefreshTokenPostData
    {
        public string refresh_token { get; set; }
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string redirect_uri { get; set; }
        public string grant_type { get; set; }

    }
    
    public class SyncTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public double expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public double created_at { get; set; }

        public DateTime ExpiresOn
        {
            get
            {
                return DateTime.Now.AddSeconds(expires_in);
            }

        }

    }

    public class SyncDummySeason
    {
        public int ShowTraktID { get; set; }
        public int SeasonTraktID { get; set; }
        public int SeasonNo { get; set; }
    }

    public class SyncDummyEpisode
    {
        public int ShowTraktID { get; set; }
        public int SeasonTraktID { get; set; }
        public int SeasonNo { get; set; }
        public int EpisodeTraktID { get; set; }
        public int EpisodeNo { get; set; }

        public string WatchedOn { get; set; }
    }

    //Grouped Models
    public class EpisodeGroup
    {
        public string Key { get; set; }
        public List<Episode> Episodes { get; set; }
        //public ObservableCollection<Episode> Episodes2 { get; set; }
    }

    public class SeasonEpisodeGroup : BindableBase
    {
        public string Key { get; set; }
        public Season Season { get; set; }
        public List<Episode> Episodes { get; set; }

        private bool? isSelected = false;
        public bool? IsSelected
        {
            get
            {
                return isSelected.Value;
            }
            set
            {
                Set(ref isSelected, value);

                Episodes.ForEach(_e => { _e.IsSelected = isSelected; });
            }
        }

    }

}
