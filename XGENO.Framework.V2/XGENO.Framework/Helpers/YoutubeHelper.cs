using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XGENO.Framework.DataModels;

namespace XGENO.Framework.Helpers
{
    public class YoutubeHelper
    {
        private RestClientHelper RestClient { get; set; }

        public YoutubeHelper()
        {
            this.RestClient = new RestClientHelper(false);
        }

        public async Task<List<YouTubeUri>> GetDownloadLinks(string videoURI)
        {
            videoURI += "&nomobile=1";

            var urls = new List<YouTubeUri>();

            string javaScriptCode = null;

            var response = await RestClient.GetAsync(videoURI);

            var match = Regex.Match(response, "url_encoded_fmt_stream_map\": ?\"(.*?)\"");
            var data = Uri.UnescapeDataString(match.Groups[1].Value);
            match = Regex.Match(response, "adaptive_fmts\": ?\"(.*?)\"");
            var data2 = Uri.UnescapeDataString(match.Groups[1].Value);
            var arr = Regex.Split(data + "," + data2, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"); // split by comma but outside quotes

            foreach (var d in arr)
            {
                var url = "";
                var signature = "";
                var tuple = new YouTubeUri();
                foreach (var p in d.Replace("\\u0026", "\t").Split('\t'))
                {
                    var index = p.IndexOf('=');
                    if (index != -1 && index < p.Length)
                    {
                        try
                        {
                            var key = p.Substring(0, index);
                            var value = Uri.UnescapeDataString(p.Substring(index + 1));
                            if (key == "url")
                                url = value;
                            else if (key == "itag")
                                tuple.Itag = int.Parse(value);
                            else if (key == "type" && (value.Contains("video/mp4") || value.Contains("audio/mp4")))
                                tuple.Type = value;
                            else if (key == "sig")
                                signature = value;
                            else if (key == "s")
                            {
                                if (javaScriptCode == null)
                                {
                                    string javaScriptUri;

                                    var urlMatch = Regex.Match(response, "\"\\\\/\\\\/s.ytimg.com\\\\/yts\\\\/jsbin\\\\/html5player-(.+?)\\.js\"");

                                    if (urlMatch.Success)
                                        javaScriptUri = "http://s.ytimg.com/yts/jsbin/html5player-" + urlMatch.Groups[1] + ".js";
                                    else
                                        javaScriptUri = "https://s.ytimg.com/yts/jsbin/player-" + Regex.Match(response, "\"\\\\/\\\\/s.ytimg.com\\\\/yts\\\\/jsbin\\\\/player-(.+?)\\.js\"").Groups[1] + ".js";


                                    javaScriptCode = await RestClient.GetAsync(javaScriptUri);
                                }

                                signature = YouTubeDecipherer.Decipher(value, javaScriptCode);
                            }

                        }
                        catch (Exception exception)
                        {
                            throw exception;
                        }
                    }
                }

                if (url.Contains("&signature=") || url.Contains("?signature="))
                    tuple.Uri = new Uri(url, UriKind.Absolute);
                else
                    tuple.Uri = new Uri(url + "&signature=" + signature, UriKind.Absolute);

                if (tuple.IsValid)
                    urls.Add(tuple);
            }

            return urls.Where(u => u.VideoQuality != YouTubeQuality.Unknown).OrderBy(v => v.Itag).ToList();
        }
        
    }

    internal static class YouTubeDecipherer
    {
        public static string Decipher(string cipher, string javaScript)
        {
            //Find "C" in this: var A = B.sig||C (B.s)
            string functNamePattern = @"\.sig\s*\|\|([a-zA-Z0-9\$]+)\("; //Regex Formed To Find Word or DollarSign

            var funcName = Regex.Match(javaScript, functNamePattern).Groups[1].Value;

            if (funcName.Contains("$"))
            {
                funcName = "\\" + funcName; //Due To Dollar Sign Introduction, Need To Escape
            }

            string funcPattern = @funcName + @"=function\(\w+\)\{.*?\},"; //Escape funcName string
            var funcBody = Regex.Match(javaScript, funcPattern, RegexOptions.Singleline).Value; //Entire sig function
            var lines = funcBody.Split(';'); //Each line in sig function

            string idReverse = "", idSlice = "", idCharSwap = ""; //Hold name for each cipher method
            string functionIdentifier = "";
            string operations = "";

            foreach (var line in lines.Skip(1).Take(lines.Length - 2)) //Matches the funcBody with each cipher method. Only runs till all three are defined.
            {
                if (!string.IsNullOrEmpty(idReverse) && !string.IsNullOrEmpty(idSlice) &&
                    !string.IsNullOrEmpty(idCharSwap))
                {
                    break; //Break loop if all three cipher methods are defined
                }

                functionIdentifier = GetFunctionFromLine(line);
                string reReverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
                string reSlice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string reSwap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.

                if (Regex.Match(javaScript, reReverse).Success)
                {
                    idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
                }

                if (Regex.Match(javaScript, reSlice).Success)
                {
                    idSlice = functionIdentifier; //If def matched the regex for slice then the current function is defined as the slice.
                }

                if (Regex.Match(javaScript, reSwap).Success)
                {
                    idCharSwap = functionIdentifier; //If def matched the regex for charSwap then the current function is defined as swap.
                }
            }

            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                functionIdentifier = GetFunctionFromLine(line);

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idCharSwap)
                {
                    operations += "w" + m.Groups["index"].Value + " "; //operation is a swap (w)
                }

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idSlice)
                {
                    operations += "s" + m.Groups["index"].Value + " "; //operation is a slice
                }

                if (functionIdentifier == idReverse) //No regex required for reverse (reverse method has no parameters)
                {
                    operations += "r "; //operation is a reverse
                }
            }

            operations = operations.Trim();

            return DecipherWithOperations(cipher, operations);
        }

        private static string ApplyOperation(string cipher, string op)
        {
            switch (op[0])
            {
                case 'r':
                    return new string(cipher.ToCharArray().Reverse().ToArray());

                case 'w':
                    {
                        int index = GetOpIndex(op);
                        return SwapFirstChar(cipher, index);
                    }

                case 's':
                    {
                        int index = GetOpIndex(op);
                        return cipher.Substring(index);
                    }

                default:
                    throw new NotImplementedException("Couldn't find cipher operation.");
            }
        }

        private static string DecipherWithOperations(string cipher, string operations)
        {
            return operations.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(cipher, ApplyOperation);
        }

        private static string GetFunctionFromLine(string currentLine)
        {
            Regex matchFunctionReg = new Regex(@"\w+\.(?<functionID>\w+)\("); //lc.ac(b,c) want the ac part.
            Match rgMatch = matchFunctionReg.Match(currentLine);
            string matchedFunction = rgMatch.Groups["functionID"].Value;
            return matchedFunction; //return 'ac'
        }

        private static int GetOpIndex(string op)
        {
            string parsed = new Regex(@".(\d+)").Match(op).Result("$1");
            int index = Int32.Parse(parsed);

            return index;
        }

        private static string SwapFirstChar(string cipher, int index)
        {
            var builder = new StringBuilder(cipher);
            builder[0] = cipher[index];
            builder[index] = cipher[0];

            return builder.ToString();
        }
    }

}
