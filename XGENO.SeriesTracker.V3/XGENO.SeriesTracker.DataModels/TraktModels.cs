using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using XGENO.Framework.Extensions;
using XGENO.Framework.MVVM;

namespace XGENO.SeriesTracker.DataModels
{
    //SHOWS

    public class SearchResult
    {
        public string type { get; set; }
        public Show show { get; set; }
    }

    public class TrendingShow
    {
        public Show show { get; set; }
    }

    public class Show : BindableBase
    {
        public Ids ids { get; set; }
        public string title { get; set; }
        public string overview { get; set; }
        public double? rating { get; set; }
        public string status { get; set; }
        public int? aired_episodes { get; set; }
        public int? runtime { get; set; }
        public string certification { get; set; }
        public string network { get; set; }
        public string country { get; set; }
        public string trailer { get; set; }
        public string first_aired { get; set; }
        public int? year { get; set; }
        public Airs airs { get; set; }
        public List<string> genres { get; set; }

        public bool IsArchived { get; set; } = false;

        public bool IsPopular { get; set; } = true;

        public bool IsTrending { get; set; } = false;

        public List<Episode> AllEpisodes { get; set; } = new List<Episode>();
        public List<Season> AllSeasons { get; set; } = new List<Season>();

        private CollectionViewSource missedEpisodeList;
        public CollectionViewSource MissedEpisodesList
        {
            get { return missedEpisodeList; }
            set
            {
                Set(ref missedEpisodeList, value);
            }
        }


        private bool isTracking;
        public bool IsTracking
        {
            get { return isTracking; }
            set
            {
                Set(ref isTracking, value);
            }
        }

        public string ShowTitle
        {
            get
            {
                return title.ToUpper();
            }
        }

        public string ShowPopularity
        {
            get
            {
                if (IsPopular && IsTrending)
                    return "POPULAR & TRENDING";
                else if (IsPopular)
                    return "POPULAR";
                else if (IsTrending)
                    return "TRNDING";
                else
                    return "N/A";
            }
        }

        public string ShowPopularityMobile
        {
            get
            {
                if (IsPopular && IsTrending)
                    return "POP & TRNDG";
                else if (IsPopular)
                    return "POPULAR";
                else if (IsTrending)
                    return "TRNDING";
                else
                    return "N/A";
            }
        }

        public string ShowStatus
        {
            get
            {
                return status?.ToUpper();
            }
        }

        public double ShowRating05
        {
            get
            {
                if (rating.HasValue)
                    return rating.Value / 2.0d;
                else
                    return 0d;
            }
        }

        public double ShowRating10
        {
            get
            {
                if (rating.HasValue)
                    return rating.Value;
                else
                    return 0d;
            }
        }

        public string Rating10Display
        {
            get
            {
                return ShowRating10.ToString("0.0");
            }
        }

        public string RatingDisplay
        {
            get
            {
                if (rating.HasValue)
                    return rating.Value.ToString("0.0") + " / 10";
                else
                    return "";
            }
        }
        
        public string SinceYear
        {
            get
            {
                if (year.HasValue)
                    return year.Value.ToString();
                else
                {
                    if (!string.IsNullOrEmpty(first_aired))
                        return Convert.ToDateTime(first_aired).Year.ToString();
                }

                return "";
            }
        }

        public string NetworkInfo
        {
            get
            {
                if (!string.IsNullOrEmpty(network))
                    return (network + ((string.IsNullOrEmpty(country) || network.IndexOf("(") > 1) ? "" : " (" + country.ToUpper() + ")")).ToUpper();

                return "N/A";

            }
        }

        public string ShowCertification
        {
            get
            {
                return (string.IsNullOrEmpty(certification) ? "N/A" : certification.ToUpper());
            }
        }

        public string NetworkAndStatusDisplay
        {
            get
            {
                return ShowStatus + " | " + NetworkInfo;

            }
        }

        public string AirsOn
        {
            get
            {
                if (airs != null)
                {
                    if (!string.IsNullOrEmpty(airs.day) && !string.IsNullOrEmpty(airs.time))
                    {
                        var retVal = airs.day.ToUpper() + "S @" + airs.time + " " + airs.timezone.ToUpper();

                        if (ShowStatus == "ENDED" || ShowStatus == "CANCELED")
                            retVal += " (ENDED)";

                        return retVal;
                    }
                }

                return "NOT AVAILABLE";
            }
        }


        public string PosterImage
        {
            get
            {
                return "https://thetvdb.com/banners/posters/" + ids.tvdb + "-1.jpg";
            }
        }
        
        public string BackgroundImage
        {
            get
            {
                return "http://www.thetvdb.com/banners/fanart/original/" + ids.tvdb + "-1.jpg";
            }
        }

        public int EpisodeLength
        {
            get
            {
                if (runtime.HasValue)
                    return runtime.Value;
                else
                    return 0;
            }
        }

        public string EpisodeLengthInMins
        {
            get
            {
                if (runtime.HasValue)
                    return runtime.Value.ToString() + " MINS";
                else
                    return "";
            }
        }
        

        public int AiredEpisodesCount
        {
            get
            {
                try
                {
                    if (AllEpisodes != null && AllEpisodes.Count > 0)
                    {
                        var computedAiredEpisodeCount = AllEpisodes.Where(_e => _e.HasAired && _e.season > 0).Count();

                        return computedAiredEpisodeCount;
                    }
                }
                catch
                {
                }

                if (aired_episodes.HasValue)
                    return aired_episodes.Value;

                return 0;
            }
        }
        
        public int WatchedEpisodesCount
        {
            get
            {
                int toExclude = 0;
                var watchedEpisodes = AppSettings.TrackedEpisodeList.Where(s => (s.ShowTraktID == ids.trakt) && (s.SeasonNo > 0)).ToList();
                var airedEpisodes = AllEpisodes.Where(_e => _e.HasAired && _e.season > 0);

                foreach (var ep in watchedEpisodes)
                {
                    if (!airedEpisodes.Where(_e => _e.ids.trakt == ep.EpisodeTraktID).Any())
                        toExclude++;
                }

                return (watchedEpisodes.Count - toExclude);
            }
        }

        private int missedEpisodeCount;
        public int MissedEpisodeCount
        {
            get
            {
                return missedEpisodeCount;
            }
            set
            {
                Set(ref missedEpisodeCount, value);
            }
        }
        
        private double watchedPercentage;
        public double WatchedPercentage
        {
            get
            {
                if (AiredEpisodesCount == 0)
                    return 100.0d;

                if (WatchedEpisodesCount >= AiredEpisodesCount)
                    return 100d;

                return WatchedEpisodesCount * 100.0d / AiredEpisodesCount;
            }
            set
            {
                Set(ref watchedPercentage, value);
            }

        }

        private string timeSpentWatching;
        public string TimeSpentWatching
        {
            get
            {
                return (EpisodeLength * WatchedEpisodesCount).FormatMinsToPeriod();
            }
            set
            {
                Set(ref timeSpentWatching, value);
            }
        }

        private string timeRemainingToWatch;
        public string TimeRemainingToWatch
        {
            get
            {
                return (EpisodeLength * MissedEpisodeCount).FormatMinsToPeriod();
            }
            set
            {
                Set(ref timeRemainingToWatch, value);
            }
        }


              
        public List<string> ShowGenres
        {
            get
            {
                if (genres == null)
                    return new List<string>();

                var theGenres = new List<string>();

                foreach(var _genre in genres)
                {
                    theGenres.Add(_genre.ToUpper());
                }

                return theGenres;
            }
        }

        public string GenresText
        {
            get
            {
                if (ShowGenres.Count > 0)
                    return string.Join(", ", ShowGenres.ToArray());

                return "";
            }
        }

        private string nextEpisodeInfo;
        public string NextEpisodeInfo
        {
            get
            {
                if (!string.IsNullOrEmpty(nextEpisodeInfo))
                    return nextEpisodeInfo;

                nextEpisodeInfo = "NEXT EPISODE TBA";
                nextEpisodeInfoMobile = "NEXT EPISODE TBA";

                var _futureEpisodes = AllEpisodes.Where(_e => !_e.HasAired && _e.season > 0).ToList();

                if (_futureEpisodes.Count == 0)
                {
                    if (ShowStatus == "ENDED" || ShowStatus == "CANCELED")
                    {
                        nextEpisodeInfo = "ENDED";
                        nextEpisodeInfoMobile = "ENDED";
                    }
                }
                else
                {
                    try
                    {
                        var _futureEpisode = _futureEpisodes.OrderBy(_e => _e.season).ThenBy(_e => _e.number).Where(_e => _e.LocalAiredDate.HasValue).First();

                        if (_futureEpisode?.AiredDate != "TBA")
                        {
                            try
                            {
                                var _daysToNextEpisde = (new DateTime(_futureEpisode.LocalAiredDate.Value.Year, _futureEpisode.LocalAiredDate.Value.Month, _futureEpisode.LocalAiredDate.Value.Day) - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).Days;

                                if (_daysToNextEpisde == 0)
                                {
                                    nextEpisodeInfo = "AIRS TODAY: " + _futureEpisode.EpisodeNo;
                                    nextEpisodeInfoMobile = "AIRS TODAY";
                                }

                                if (_daysToNextEpisde == 1)
                                {
                                    nextEpisodeInfo = "AIRS TOMORROW: " + _futureEpisode.EpisodeNo;
                                    nextEpisodeInfoMobile = "AIRS TOMORROW";
                                }

                                if (_daysToNextEpisde > 1 && _daysToNextEpisde <= 30)
                                {
                                    nextEpisodeInfo = "IN " + _daysToNextEpisde.ToString() + " DAYS: " + _futureEpisode.EpisodeNo;
                                    nextEpisodeInfoMobile = "IN " + _daysToNextEpisde.ToString() + " DAYS";
                                }

                                if (_daysToNextEpisde > 30)
                                {
                                    if (!HideShowsWithNoUpcomingInfoEnabled)
                                    {
                                        nextEpisodeInfo = "IN " + _daysToNextEpisde.ToString() + " DAYS: " + _futureEpisode.EpisodeNo;
                                        nextEpisodeInfoMobile = "IN " + _daysToNextEpisde.ToString() + " DAYS";
                                    }
                                    else
                                    {

                                    }
                                }


                            }
                            catch
                            {
                                //nextEpisodeInfo = _futureEpisode.EpisodeNo + " (" + _futureEpisode.EpisodeName + ")" + " DATE YET TO BE ANNOUNCED";
                            }
                        }
                    }
                    catch
                    {
                    }
                }

                return nextEpisodeInfo;

            }
        }

        private string nextEpisodeInfoMobile;
        public string NextEpisodeInfoMobile
        {
            get
            {
                if (string.IsNullOrEmpty(nextEpisodeInfoMobile))
                {
                    var fullStatus = NextEpisodeInfo;
                }

                return nextEpisodeInfoMobile;
            }
        }


        private string showTrackingStatus;
        public string ShowTrackingStatus
        {
            get
            {
                string retVal = (MissedEpisodeCount == 0 ? "ALL CAUGHT UP" : MissedEpisodeCount.ToString() + " MISSED") + " - " + NextEpisodeInfo;

                return retVal;
            }
            set
            {
                Set(ref showTrackingStatus, value);
            }

        }

        private string showTrackingStatusMobile;
        public string ShowTrackingStatusMobile
        {
            get
            {
                string retVal = (MissedEpisodeCount == 0 ? "ALL CAUGHT UP" : MissedEpisodeCount.ToString() + " MISSED") + " - " + NextEpisodeInfoMobile;

                if (!retVal.StartsWith("ALL CAUGHT UP"))
                    retVal = retVal.Replace("NEXT EPISODE TBA", "NEXT TBA");

                return retVal.Replace("ALL CAUGHT UP - ", "");
            }
            set
            {
                Set(ref showTrackingStatusMobile, value);
            }
        }

        private bool sortOrderSet = false;
        private int sortOrder;
        public int SortOrder
        {
            get
            {
                if(sortOrderSet)
                    return sortOrder;

                if (MissedEpisodeCount == 0)
                {
                    var _nextInfo = NextEpisodeInfo;
                    
                    if(_nextInfo == "NEXT EPISODE TBA")
                        sortOrder = 900;
                    else if(_nextInfo == "ENDED")
                        sortOrder = 1000;
                    else if(_nextInfo.StartsWith("AIRS TODAY"))
                        sortOrder = 1;
                    else if(_nextInfo.StartsWith("AIRS TOMORROW"))
                        sortOrder = 2;
                    else if(_nextInfo.StartsWith("IN "))
                    {
                        try
                        {
                            _nextInfo = _nextInfo.Replace("IN ", "");
                            var _days = Convert.ToInt32(_nextInfo.Split(new string[] { " " }, StringSplitOptions.None)[0]);
                            sortOrder = 500 + _days;
                        }
                        catch
                        {
                            sortOrder = 850;
                        }

                    }
                    else
                        sortOrder = 850;
                }
                else
                {
                    sortOrder = MissedEpisodeCount + (MissedEpisodeCount<=2? 5:10);
                }

                sortOrderSet = true;
                return sortOrder;
            }

        }

        public void CalculateMissedEpisodesCount()
        {
            MissedEpisodeCount = AiredEpisodesCount - WatchedEpisodesCount;
        }

        public void SetMissedEpisodesList()
        {
            if (AllEpisodes != null && AllEpisodes.Count > 0)
            {
                var grouped = from episode in AllEpisodes.Where(_e => _e.HasAired && _e.season > 0 && !_e.IsWatched)
                              group episode by "SEASON " + episode.season.ToString()
                              into grp
                              select new SeasonEpisodeGroup
                              {
                                  Key = grp.Key,
                                  Episodes = grp.ToList()
                              };

                CollectionViewSource _cvs = new CollectionViewSource();
                _cvs.IsSourceGrouped = true;
                _cvs.ItemsPath = new Windows.UI.Xaml.PropertyPath("Episodes");
                _cvs.Source = grouped.ToList();

                MissedEpisodesList = _cvs;
            }
            else
                MissedEpisodesList = new CollectionViewSource();

        }

        private bool HideShowsWithNoUpcomingInfoEnabled = false;
        public void SetHideShowsWithFutureInformation(bool value)
        {
            HideShowsWithNoUpcomingInfoEnabled = value;
        }
        
    }

    public class Airs
    {
        public string day { get; set; }
        public string time { get; set; }
        public string timezone { get; set; }
    }


    //SEASONS

    public class Season : BindableBase
    {
        public int number { get; set; }
        public Ids ids { get; set; }
        public int? episode_count { get; set; }
        public int? aired_episodes { get; set; }
        public string overview { get; set; }

        public List<Episode> episodes { get; set; }


        public int AiredEpisodesCount { get; set; }
        public string SeasonYear { get; set; }

        public string SeasonNo
        {
            get
            {
                return number.ToString("00");
            }
        }

        public string SeasonName
        {
            get
            {
                return "SEASON " + number.ToString();
            }
        }

        public string SeasonStatus
        {
            get
            {
                if (EpisodeCount == AiredEpisodesCount && EpisodeCount > 0)
                    return "ENDED";

                if (EpisodeCount == 0)
                    return "YET TO AIR";

                return "ON AIR";
            }
        }

        public string SeasonNameWithStatus
        {
            get
            {
                return SeasonName + "  ӏ  " + SeasonYear + " - " + SeasonStatus;
            }
        }

        public int EpisodeCount
        {
            get
            {
                try
                {
                    if (episodes != null && episodes.Count > 0)
                        return episodes.Count;
                }
                catch
                {
                }

                if (episode_count.HasValue)
                    return episode_count.Value;

                return 0;
            }
        }

        public string EpisodesStatus
        {
            get
            {
                if (EpisodeCount == AiredEpisodesCount)
                    return EpisodeCount.ToString() + " EPISODES";


                return EpisodeCount.ToString() + " EPISODES  ӏ  " + AiredEpisodesCount.ToString() + " AIRED";
            }
        }

        public List<Episode> AllEpisodes
        {
            get
            {
                if(EpisodeCount > 0)
                    return episodes.OrderByDescending(_e => _e.number).ToList();

                return new List<Episode>();
            }
        }

        public void SetSeasonInfo()
        {
            //Aired Episodes Count
            if (episodes == null)
                AiredEpisodesCount = 0;
            else
                AiredEpisodesCount = episodes.Where(_e => _e.HasAired).Count();

            //Season Year
            if (episodes == null)
                SeasonYear = "TBA";
            else
            {
                try
                {
                    var theFirstEpisodeDate = episodes.OrderBy(_s => _s.number).First().LocalAiredDate;

                    if (theFirstEpisodeDate.HasValue)
                        SeasonYear = theFirstEpisodeDate.Value.Year.ToString();
                    else
                        SeasonYear = "TBA";
                }
                catch
                {
                    SeasonYear = "TBA";
                }
            }
        }

    }


    //EPISODES

    public class Episode : BindableBase
    {
        public int season { get; set; }
        public int number { get; set; }
        public string title { get; set; }
        public Ids ids { get; set; }
        public string overview { get; set; }
        public string first_aired { get; set; }
        public double? rating { get; set; }

        
        public Airs airs { get; set; }
        public string ShowName { get; set; }
        public string EpisodeLength { get; set; }
        public int ShowTraktID { get; set; }
        public string ShowTVDBID { get; set; }
        public int SeasonTraktID { get; set; }
        public string ShowPosterImage { get; set; }
        public string ShowBackgroundImage { get; set; }

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
            }
        }

        private bool watchedSet = false;
        private bool isWatched = false;
        public bool IsWatched
        {
            get
            {
                if (!watchedSet)
                {
                    watchedSet = true;
                    isWatched = AppSettings.TrackedEpisodeList.Where(s => s.EpisodeTraktID == ids.trakt).Any() ? true : false;
                }

                return isWatched;
            }
            set
            {
                Set(ref isWatched, value);
                watchedSet = true;
            }
        }

        public bool IsNotified
        {
            get
            {
                return AppSettings.NotifiedEpisodeList.Where(s => s.EpisodeTraktID == ids.trakt).Any() ? true : false;
            }
        }

        public string EpisodeNo
        {
            get
            {
                return "S" + season.ToString("00") + "E" + number.ToString("00");
            }
        }

        public string EpisodeName
        {
            get
            {
                return title?.ToUpper() ?? "TBA";
            }
        }

        public string EpisodeFullName
        {
            get
            {
                return EpisodeNo + ": " + EpisodeName;
            }
        }

        public DateTime? LocalAiredDate { get; set; } = null;
        public bool HasAired
        {
            get
            {
                try
                {
                    if (LocalAiredDate.HasValue)
                    {
                        if (DateTime.Now > LocalAiredDate.Value)
                            return true;
                    }
                }
                catch
                {
                }

                return false;
            }
        }

        public string AiredDate
        {
            get
            {
                try
                {
                    if (!LocalAiredDate.HasValue)
                        return "TBA";

                    return LocalAiredDate.Value.ToString("MMM dd, yyyy @ hh:mm tt");
                }
                catch
                {
                }

                return "TBA";
            }
        }
        
        public string AiredDateToCompare
        {
            get
            {
                try
                {
                    if (!LocalAiredDate.HasValue)
                        return "TBA";

                    return LocalAiredDate.Value.ToString("yyyy-MM-dd");
                }
                catch
                {
                }

                return "TBA";
            }
        }

        public string AiredDateGroup
        {
            get
            {

                var datToday = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                var datAir = new DateTime(LocalAiredDate.Value.Year, LocalAiredDate.Value.Month, LocalAiredDate.Value.Day);

                var _daysToNextEpisde = (datAir - datToday).Days;

                if (_daysToNextEpisde == 0)
                    return "TODAY";

                if (_daysToNextEpisde == 1)
                    return "TOMORROW";

                if(datAir <= datToday.ThisWeekEnd())
                    return "THIS WEEK";

                if(datAir <= datToday.NextWeekEnd())
                    return "NEXT WEEK";

                if(datAir <= datToday.LastDayOfTheMonth())
                    return "THIS MONTH";

                if(datAir <= datToday.LastDayOfNextMonth())
                    return "NEXT MONTH";

                return "MORE THAN A MONTH";
            }
        }

        public double EpisodeRating05
        {
            get
            {
                if (rating.HasValue)
                    return rating.Value / 2.0d;
                else
                    return 0d;
            }
        }

        public double EpisodeRating10
        {
            get
            {
                if (rating.HasValue)
                    return rating.Value;
                else
                    return 0d;
            }
        }

        public string Rating10Display
        {
            get
            {
                return EpisodeRating10.ToString("0.0");
            }
        }

        public string RatingDisplay
        {
            get
            {
                if (rating.HasValue)
                    return rating.Value.ToString("0.0") + " / 10";
                else
                    return "";
            }
        }

        public string BackgroundImage
        {
            get
            {
                return "http://thetvdb.com/banners/episodes/" + ShowTVDBID + "/" + ids.tvdb + ".jpg";
            }
        }

        public void SetEpisodeInfo(Airs _airs, string _showName, int _showTraktID, int _seasonTraktID, string _showTVDBID, string _showBackgroundImage, string _showPosterImage, string _episodeLength)
        {
            airs = _airs;
            ShowName = _showName;
            ShowTraktID = _showTraktID;
            ShowTVDBID = _showTVDBID;
            SeasonTraktID = _seasonTraktID;
            ShowBackgroundImage = _showBackgroundImage;
            ShowPosterImage = _showPosterImage;
            EpisodeLength = _episodeLength;

            LocalAiredDate = GetLocalAiredDate();
        }

        private DateTime? GetLocalAiredDate()
        {

            //Calculate the Local Aired Date
            try
            {
                if (first_aired == null)
                    return null;

                if (Convert.ToDateTime(first_aired).ToString("dd-MMM-yyyy") == "01-Jan-0001")
                    return null;
            }
            catch
            {
            }

            return Convert.ToDateTime(first_aired);
        }
    }


    //PEOPLE

    public class CastResult
    {
        public List<Cast> cast { get; set; }
    }

    public class Cast : BindableBase
    {
        public string character { get; set; }
        public Person person { get; set; }
        public Show show { get; set; }

        private string castImage;
        public string CastImage
        {
            get
            {
                if (string.IsNullOrEmpty(castImage))
                    return "ms-appx:///Assets/HeadShot.png";

                return castImage;
            }
            set
            {
                Set(ref castImage, value);
            }
        }

        public string ActorName
        {
            get
            {
                return person.name.ToUpper();
            }
        }

        public string AsCharacterName
        {
            get
            {
                return (!string.IsNullOrEmpty(character) ? "AS " + character.ToUpper() : "");
            }
        }
    }

    public class Person
    {
        public string name { get; set; }
        public Ids ids { get; set; }
        public string biography { get; set; }
        public string birthday { get; set; }
        public string death { get; set; }
        public string birthplace { get; set; }
        public string homepage { get; set; }

        public string PersonBiography
        {
            get
            {
                if (string.IsNullOrEmpty(biography))
                    return "";

                string bio = biography.Replace("From Wikipedia, the free encyclopedia.", "").Replace("From Wikipedia, the free encyclopedia", "").Trim();

                while(bio.StartsWith("\n") || bio.StartsWith("​") || bio.StartsWith(" "))
                {
                    bio = bio.Substring(1);
                }

                return bio;
            }
        }

        public string Age
        {
            get
            {
                if (string.IsNullOrEmpty(birthday))
                    return "N/A";

                return birthday.Age();
            }
        }

        public string LivingInfo
        {
            get
            {
                string lifeInfo = "";

                if(!string.IsNullOrEmpty(birthday) && !string.IsNullOrEmpty(birthplace))
                    lifeInfo += "Born " + Convert.ToDateTime(birthday).ToString("MMM dd, yyyy") + " in " + birthplace;
                else
                {
                    if(!string.IsNullOrEmpty(birthday))
                        lifeInfo += "Born " + Convert.ToDateTime(birthday).ToString("MMM dd, yyyy");

                    if(!!string.IsNullOrEmpty(birthplace))
                        lifeInfo += "Born in " + birthplace;
                }

                if(!string.IsNullOrEmpty(death))
                    lifeInfo += (string.IsNullOrEmpty(lifeInfo)? "Died " : " ӏ Died ") + Convert.ToDateTime(death).ToString("MMM dd, yyyy");

                return lifeInfo;
            }
        }

    }

    //COMMENTS

    public class Comment
    {
        public string comment { get; set; }
        public string created_at { get; set; }
        public int? likes { get; set; }
        public int? user_rating { get; set; }
        public User user { get; set; }

        public string PostedBy
        {
            get
            {
                return (string.IsNullOrEmpty(user.name) ? user.username : user.name) + (string.IsNullOrEmpty(user.location) ? "" : " (" + user.location + ")");
            }
        }

        public string PostedInformation
        {
            get
            {
                return created_at.TimeAgoFull() + " | " + PostedBy;
            }
        }

        public double Rating05
        {
            get
            {
                if (user_rating.HasValue)
                    return user_rating.Value / 2d;
                else
                    return 0d;
            }
        }

        public string RatingDisplay
        {
            get
            {
                if (user_rating.HasValue)
                    return user_rating.Value.ToString() + " / 10";
                else
                    return "0 / 10";
            }
        }
    }


    //USER

    public class User
    {
        public string username { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public Images images { get; set; }

        public string Avatar
        {
            get
            {

                if (images != null && images.avatar != null && !string.IsNullOrEmpty(images.avatar.thumb))
                    return images.avatar.thumb;

                if (images != null && images.avatar != null && !string.IsNullOrEmpty(images.avatar.medium))
                    return images.avatar.medium;

                if (images != null && images.avatar != null && !string.IsNullOrEmpty(images.avatar.full))
                    return images.avatar.full;

                return "ms-appx:///Assets/HeadShot.png";
            }
        }
    }

    //COMMON

    public class Ids
    {
        public int trakt { get; set; }
        public string slug { get; set; }
        public string imdb { get; set; }
        public string tvdb { get; set; }
        public string tmdb { get; set; }
        public string tvrage { get; set; }
    }

    public class Images
    {
        //Show, Season
        public ImageArt poster { get; set; }

        //Show and Cast
        public ImageArt fanart { get; set; }

        //Show only - not available in Search Results
        public ImageArt banner { get; set; }

        //Cast only
        public ImageArt headshot { get; set; }

        //Episode only
        public ImageArt screenshot { get; set; }

        //User Only
        public ImageArt avatar { get; set; }
    }

    public class ImageArt
    {
        public string full { get; set; }
        public string medium { get; set; }
        public string thumb { get; set; }
    }
}


