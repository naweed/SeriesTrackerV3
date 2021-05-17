using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XGENO.Framework.DataModels
{
    public enum YouTubeQuality : short
    {
        // video
        Quality144P,
        Quality240P,
        Quality270P,
        Quality360P,
        Quality480P,
        Quality520P,
        Quality720P,
        Quality1080P,
        Quality2160P,

        // audio
        QualityLow,
        QualityMedium,
        QualityHigh,

        NotAvailable,
        Unknown,
        AudioOnly
    }
    
    public class YouTubeUri
    {
        public int Itag { get; internal set; }

        public Uri Uri { get; internal set; }

        public string Type { get; internal set; }

        public bool HasAudio
        {
            get { return AudioQuality != YouTubeQuality.Unknown; }
        }

        public bool HasVideo
        {
            get { return VideoQuality != YouTubeQuality.Unknown; }
        }

        public bool Is3DVideo
        {
            get
            {
                if (VideoQuality == YouTubeQuality.Unknown)
                    return false;

                return Itag >= 82 && Itag <= 85;
            }
        }

        public YouTubeQuality VideoQuality
        {
            get
            {
                switch (Itag)
                {
                    // video & audio
                    case 18: return YouTubeQuality.Quality360P;
                    case 22: return YouTubeQuality.Quality720P;
                    case 37: return YouTubeQuality.Quality1080P;
                    case 38: return YouTubeQuality.Quality2160P;

                    // 3d video & audio
                    case 82: return YouTubeQuality.Quality360P;
                    case 83: return YouTubeQuality.Quality480P;
                    case 84: return YouTubeQuality.Quality720P;
                    case 85: return YouTubeQuality.Quality520P;

                    // video only (adaptive - aka DASH)
                    //case 134: return YouTubeQuality.Quality360P;
                    //case 135: return YouTubeQuality.Quality480P;
                    //case 136: return YouTubeQuality.Quality720P;
                    case 137: return YouTubeQuality.Quality1080P;
                    case 138: return YouTubeQuality.Quality2160P;

                    // audio only (adaptive - aka DASH)
                    case 139: return YouTubeQuality.AudioOnly;
                    case 140: return YouTubeQuality.AudioOnly;
                    case 141: return YouTubeQuality.AudioOnly;
                }

                return YouTubeQuality.Unknown;
            }
        }

        public YouTubeQuality AudioQuality
        {
            get
            {
                switch (Itag)
                {
                    //// video & audio
                    //case 18: return YouTubeQuality.QualityMedium;
                    //case 22: return YouTubeQuality.QualityHigh;
                    //case 37: return YouTubeQuality.QualityHigh;
                    //case 38: return YouTubeQuality.QualityHigh;

                    //// 3d video & audio
                    //case 82: return YouTubeQuality.QualityMedium;
                    //case 83: return YouTubeQuality.QualityMedium;
                    //case 84: return YouTubeQuality.QualityHigh;
                    //case 85: return YouTubeQuality.QualityHigh;

                    //// video only
                    //case 134: return YouTubeQuality.NotAvailable;
                    //case 135: return YouTubeQuality.NotAvailable;
                    //case 136: return YouTubeQuality.NotAvailable;
                    //case 137: return YouTubeQuality.NotAvailable;
                    //case 138: return YouTubeQuality.NotAvailable;

                    // audio only
                    case 139: return YouTubeQuality.QualityLow;
                    case 140: return YouTubeQuality.QualityMedium;
                    case 141: return YouTubeQuality.QualityHigh;
                }

                return YouTubeQuality.Unknown;
            }
        }

        internal bool IsValid
        {
            get { return Uri != null && Uri.ToString().StartsWith("http") && Itag > 0 && Type != null; }
        }

        public string Extension
        {
            get
            {
                string _extension = "mp4";

                if (HasAudio)
                    _extension = "aac";

                try
                {
                    _extension = Type.Split(new char[] { ';' })[0].Split(new char[] { '/' })[1].Trim();
                }
                catch
                {
                }

                return _extension;
            }
        }

        public string VideoAudioQuality
        {
            get
            {
                switch (Itag)
                {
                    // video & audio
                    case 18: return "MP4 360P";
                    case 22: return "MP4 720P (HD)";
                    case 37: return "MP4 1080P (HD)";
                    case 38: return "MP4 2160P (HD)";

                    // 3d video & audio
                    case 82: return "MP4 360P (3d)";
                    case 83: return "MP4 480P (3d)";
                    case 84: return "MP4 720P (3d + HD)";
                    case 85: return "MP4 520P (3d)";

                    // video only (adaptive - aka DASH)
                    case 134: return "MP4 360P (DASH)";
                    case 135: return "MP4 480P (DASH)";
                    case 136: return "MP4 720P (DASH + HD)";
                    case 137: return "MP4 1080P* (HD)";
                    case 138: return "MP4 2160P* (HD)";

                    // audio only (adaptive - aka DASH)
                    case 139: return "AUDIO (LQ)";
                    case 140: return "AUDIO (MQ)";
                    case 141: return "AUDIO (HQ)";
                }

                return "Unknown";
            }
        }

        public string VideoAudioSize
        {
            get
            {
                switch (Itag)
                {
                    // video & audio
                    case 18: return "480 X 360";
                    case 22: return "1280 X 720";
                    case 37: return "1920 X 1080";
                    case 38: return "3840 X 2160";

                    // 3d video & audio
                    case 82: return "480 X 360";
                    case 83: return "640 X 480";
                    case 84: return "1280 X 720";
                    case 85: return "930 X 520";

                    // video only (adaptive - aka DASH)
                    case 134: return "480 X 360";
                    case 135: return "640 X 480";
                    case 136: return "1280 X 720";
                    case 137: return "1920 X 1080";
                    case 138: return "3840 X 2160";

                    // audio only (adaptive - aka DASH)
                    case 139: return "LOW QUALITY";
                    case 140: return "MEDIUM QUALITY";
                    case 141: return "HIGH QUALITY";
                }

                return "Unknown";
            }
        }

        public string IconURI
        {
            get
            {
                return "/Assets/Icons/" + (HasAudio ? "Audio" : "Video") + ".png";
            }
        }

        public string DownloadURL
        {
            get
            {
                return Uri.OriginalString;
            }
        }
    }


}
