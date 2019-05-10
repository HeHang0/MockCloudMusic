using HtmlAgilityPack;
using MusicCollection.MusicManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MusicCollection.MusicAPI
{
    class NetMusicHelper
    {
        private static readonly Dictionary<NetMusicType, string> SearchAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/api/search/pc?s={0}&offset={1}&limit=30&type=1" },
            { NetMusicType.QQMusic, "http://i.y.qq.com/s.music/fcgi-bin/search_for_qq_cp?g_tk=938407465&uin=0&format=jsonp&inCharset=utf-8&outCharset=utf-8&notice=0&platform=h5&needNewCode=1&w={0}&zhidaqu=1&catZhida=1&t=0&flag=1&ie=utf-8&sem=1&aggr=0&perpage=30&n=30&p={1}&remoteplace=txt.mqq.all&_=1459991037831" },
            { NetMusicType.XiaMiMusic, "http://api.xiami.com/web?v=2.0&app_key=1&key={0}&page={1}&limit=30&r=search/songs" }
        };

        private static readonly Dictionary<NetMusicType, string> LyricAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/api/song/lyric?os=pc&id={0}&lv=-1&kv=-1&tv=-1" },
            { NetMusicType.QQMusic,"https://i.y.qq.com/lyric/fcgi-bin/fcg_query_lyric.fcg?songmid={0}&loginUin=0&hostUin=0&format=jsonp&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" }
        };

        private static readonly Dictionary<NetMusicType, string> DownloadLinkAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://link.hhtjim.com/163/{0}.mp3" },
            { NetMusicType.XiaMiMusic, "https://music-api-jwzcyzizya.now.sh/api/search/song/xiami?&limit=1&page=1&key={0}-{1}-{2}/" }
        };

        private static readonly Dictionary<NetMusicType, string> PlayListHotAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/discover/playlist/?order=hot&limit=35&offset={0}" },
            { NetMusicType.QQMusic, "https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_by_tag.fcg?rnd=0.4781484879517406&g_tk=732560869&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&categoryId=10000000&sortId=5&sin={0}&ein={1}" },
            //{ NetMusicType.XiaMiMusic, "http://www.xiami.com/collect/recommend/page/{0}" }
            { NetMusicType.XiaMiMusic, "https://www.xiami.com/api/list/collect?_q={{\"pagingVO\":{{\"page\":{0},\"pageSize\":30}},\"dataType\":\"system\"}}&_s={1}" }
        };
        private static readonly Dictionary<NetMusicType, string> PlayListDetailAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "https://music.163.com/weapi/v3/playlist/detail" },
            { NetMusicType.QQMusic, "https://i.y.qq.com/qzone-music/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&nosign=1&disstid={0}&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" },
            //{ NetMusicType.XiaMiMusic, "http://api.xiami.com/web?v=2.0&app_key=1&id={0}&r=collect/detail" }
            { NetMusicType.XiaMiMusic, "https://www.xiami.com/api/collect/initialize?_q=%7B%22listId%22:{0}%7D&_s={1}" }
        };
        private static string XMToken = "";
        private static string XMTokenPre = "";
        private static string XMTokenSig = "";
        private static string ThisWeek = Math.Abs(GetWeekOfYear(DateTime.Now) - 2).ToString().PadLeft(2);
        private static Dictionary<NetMusicType, Dictionary<RankingListType,string>> RankinListAPI = new Dictionary<NetMusicType, Dictionary<RankingListType, string>>()
        {
            {
                NetMusicType.CloudMusic,
                new Dictionary<RankingListType, string>()
                {
                    { RankingListType.HotList, "3778678" },
                    { RankingListType.NewSongList, "3779629" },
                    { RankingListType.SoarList, "19723756" }
                }
            },
            {
                NetMusicType.QQMusic,
                new Dictionary<RankingListType, string>()
                {
                    { RankingListType.HotList, $"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&date={DateTime.Now.Year}_{ThisWeek}&topid=26&type=top&song_begin=0&song_num=100&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" },
                    { RankingListType.NewSongList, $"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&date={DateTime.Now.AddDays(DateTime.Now.Hour < 10 ? -2 : -1).ToString("yyyy-MM-dd")}&topid=27&type=top&song_begin=0&song_num=100&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" },
                    { RankingListType.SoarList, $"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&date={DateTime.Now.AddDays(DateTime.Now.Hour < 10 ? -2 : -1).ToString("yyyy-MM-dd")}&topid=4&type=top&song_begin=0&song_num=100&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" }
                }
            },
            {
                NetMusicType.XiaMiMusic,
                new Dictionary<RankingListType, string>()
                {
                    { RankingListType.HotList, "http://api.xiami.com/web?v=2.0&app_key=1&id=101&page=1&limit=100&r=rank/song-list" },
                    { RankingListType.NewSongList, "http://api.xiami.com/web?v=2.0&app_key=1&id=102&page=1&limit=100&r=rank/song-list" },
                    { RankingListType.SoarList, "http://api.xiami.com/web?v=2.0&app_key=1&id=103&page=1&limit=100&r=rank/song-list" }
                }
            }
        };

        public static string GetUrlByNetMusic(Music music = null, NetMusic netMusic = null)
        {
            if (netMusic != null)
            {
                music = new Music(netMusic);
            }
            string Url = string.Empty;
            switch (music.Origin)
            {
                case NetMusicType.CloudMusic:
                    Url = GetUrlFromCloudMusic(music);
                    break;
                case NetMusicType.QQMusic:
                    Url = GetUrlFromQQMusic(music);
                    break;
                case NetMusicType.XiaMiMusic:
                    Url = GetUrlFromXiaMiMusic(music);
                    break;
            }
            if (!string.IsNullOrWhiteSpace(Url) && CheckLink(Url))
            {
                return Url;
            }
            else if (CheckLink(music.Path))
            {
                return music.Path;
            }
            return "";
        }

        public static bool CheckLink(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            HttpWebRequest req = null;
            try
            {
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "HEAD";  //这是关键        
                req.Timeout = 5000;
                HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (req != null)
                {
                    req.Abort();
                    req = null;
                }
            }
        }

        public static Music GetMusicByNetMusic(NetMusic net_music)
        {
            var path = GetMusicUrlOfLocal(net_music);
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }
            //Music music = new Music();
            var list = new List<Music>();
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                list.Add(new Music(path));
            }
            else
            {
                object[] args = new object[] { list, path };
                Thread staThread = new Thread(new ParameterizedThreadStart(NewMusicSTA));
                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start(args);
                staThread.Join();
            }
            var music = list[0];
            music.AlbumImageUrl = net_music.AlbumImageUrl;
            music.LyricPath = GetLyricByNetMusic(net_music);
            return new Music(music, net_music);
        }

        private static void NewMusicSTA(object o)
        {
            object[] args = (object[])o;
            var list = (List<Music>)args[0];
            var path = (string)args[1];
            list.Add(new Music(path));
        }
        public static string GetLyricByUrl(Music music, string url)
        {
            var LyricPath = "";
            string LyricStr = SendDataByGET(url);
            if (!string.IsNullOrWhiteSpace(LyricStr))
            {
                string[] LiricLine = LyricStr.Split('\n');
                for (int i = 0; i < LiricLine.Length; i++)
                {
                    if (LiricLine[i].Contains("[x-trans]"))
                    {
                        var sss = Regex.Match(LiricLine[i-1], "(\\[[\\d|:|.]+\\])").Groups[1].Value;
                        LiricLine[i] = LiricLine[i].Replace("[x-trans]", Regex.Match(LiricLine[i-1], "(\\[[\\d|:|.]+\\])").Groups[1].Value);
                    }
                }
                if (!Directory.Exists("DownLoad\\Lyric\\"))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory("DownLoad\\Lyric\\");
                }
                File.WriteAllLines($"DownLoad\\Lyric\\{music.Title} - {music.Singer}.lrc", LiricLine);
                LyricPath = "DownLoad\\Lyric\\" + music.Title + " - " + music.Singer + ".lrc";
                if (File.Exists(Path.GetFullPath(LyricPath)))
                {
                    return Path.GetFullPath(LyricPath);
                }
            }
            return LyricPath;
        }
        public static string GetLyricByMusic(Music music)
        {
            var LyricStr = "";
            var LyricPath = GetLyricByNetMusic(music);
            if (!string.IsNullOrWhiteSpace(LyricPath))
            {
                return LyricPath;
            }
            try
            {
                var LyricInfo = "";
                JObject jo;
                var LyricId = "";
                try
                {
                    LyricInfo = SendDataByGET($"http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword={Path.GetFileNameWithoutExtension(music.Path)}&duration={music.Duration.TotalMilliseconds}&hash=");
                    jo = (JObject)JsonConvert.DeserializeObject(LyricInfo);
                    LyricId = jo["candidates"][0]["id"].ToString();
                }
                catch (Exception)
                {
                    LyricInfo = SendDataByGET($"http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword={music.Title}&duration={music.Duration.TotalMilliseconds}&hash=");
                    jo = (JObject)JsonConvert.DeserializeObject(LyricInfo);
                    LyricId = jo["candidates"][0]["id"].ToString();
                }
                //var LyricInfo = SendDataByGET($"http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword={Path.GetFileNameWithoutExtension(music.Path)}{music.Title}{music.Singer}&duration={music.Duration.TotalMilliseconds}&hash=");
                //JObject jo = (JObject)JsonConvert.DeserializeObject(LyricInfo);
                //var LyricId = jo["candidates"][0]["id"].ToString();
                var LyricAccesskey = jo["candidates"][0]["accesskey"].ToString();
                var LyricBase64 = SendDataByGET($"http://lyrics.kugou.com/download?ver=1&client=pc&id={LyricId}&accesskey={LyricAccesskey}&fmt=lrc&charset=utf8");
                jo = (JObject)JsonConvert.DeserializeObject(LyricBase64);
                var LyricBase64Str = jo["content"].ToString();
                var LyricBytes = Convert.FromBase64String(LyricBase64Str);
                LyricStr = Encoding.UTF8.GetString(LyricBytes);
                if (!Directory.Exists("DownLoad\\Lyric\\"))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory("DownLoad\\Lyric\\");
                }
                File.WriteAllText($"DownLoad\\Lyric\\{music.Title} - {music.Singer}.lrc", LyricStr);
                LyricPath = "DownLoad\\Lyric\\" + music.Title + " - " + music.Singer + ".lrc";
                if (File.Exists(Path.GetFullPath(LyricPath)))
                {
                    return Path.GetFullPath(LyricPath);
                }
            }
            catch (Exception)
            {
            }
            return LyricPath;
        }
        public static string GetLyricByNetMusic(NetMusic music)
        {
            return GetLyricByNetMusic(new Music(music));
        }
        public static string GetLyricByNetMusic(Music music)
        {
            var LyricStr = "";
            var LyricPath = "";
            try
            {
                if (!string.IsNullOrWhiteSpace(music.LyricPath) && CheckLink(music.LyricPath))
                {
                    LyricStr = SendDataByGET(music.LyricPath);
                }
                else
                {
                    LyricStr = SendDataByGET(string.Format(LyricAPI[music.Origin], music.MusicID));
                }
                if (!string.IsNullOrWhiteSpace(LyricStr))
                {
                    string[] LiricLine = AnalyzeLyric(LyricStr, music.Origin);
                    if (!Directory.Exists("DownLoad\\Lyric\\"))//如果不存在就创建文件夹
                    {
                        Directory.CreateDirectory("DownLoad\\Lyric\\");
                    }
                    File.WriteAllLines($"DownLoad\\Lyric\\{music.Title} - {music.Singer}.lrc", LiricLine);
                    LyricPath = "DownLoad\\Lyric\\" + music.Title + " - " + music.Singer + ".lrc";
                    if (File.Exists(Path.GetFullPath(LyricPath)))
                    {
                        return Path.GetFullPath(LyricPath);
                    }
                }
            }
            catch (Exception)
            {
                
            }
            return LyricPath;
        }

        private static string[] AnalyzeLyric(string lyricStr, NetMusicType type)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return CloudMusicLyricAnalyze(lyricStr);
                case NetMusicType.QQMusic:
                    return QQMusicLyricAnalyze(lyricStr);
            }
            return new string[0];
        }

        private static string[] QQMusicLyricAnalyze(string lyricStr)
        {
            try
            {
                lyricStr = Regex.Replace(lyricStr, "^[a-zA-Z]{0,10}[C|c]allback\\(", "");
                lyricStr = Regex.Replace(lyricStr, ";$", "");
                lyricStr = Regex.Replace(lyricStr, "\\)$", "");
                JObject jo = (JObject)JsonConvert.DeserializeObject(lyricStr);
                lyricStr = jo["lyric"].ToString();
                lyricStr = Encoding.UTF8.GetString(Convert.FromBase64String(lyricStr));
                var LiricLine = lyricStr.Split('\n');
                return LiricLine;
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        private static string[] CloudMusicLyricAnalyze(string lyricStr)
        {
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(lyricStr);
                lyricStr = jo["lrc"]["lyric"].ToString();
                if (!Directory.Exists("DownLoad\\Lyric\\"))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory("DownLoad\\Lyric\\");
                }
                var LiricLine = lyricStr.Split('\n');
                return LiricLine;
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        public static ObservableCollection<NetMusic> GetNetMusicList(string SearchStr, int offset, NetMusicType type, out int count)
        {
            var retString = string.Empty;
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    retString = SendDataByPOST(string.Format(SearchAPI[type], SearchStr, offset * 30));
                    break;
                case NetMusicType.QQMusic:
                    retString = SendDataByGET(string.Format(SearchAPI[type], SearchStr, offset + 1));
                    break;
                case NetMusicType.XiaMiMusic:
                    retString = SendDataByGET(string.Format(SearchAPI[type], SearchStr, offset + 1));
                    break;

            }
            if (retString.Length < 50)
            {
                count = 0;
                return new ObservableCollection<NetMusic>();
            }
            return GetNetMusicListBySources(retString, type, out count);
        }

        public static List<NetMusic> GetNetMusicList(RankingListType listType, NetMusicType type)
        {
            List<NetMusic> list = new List<NetMusic>();
            string name, imgurl;
            string retStr = string.Empty;
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    list = GetCloudMusicPlayListItems(RankinListAPI[type][listType], out name, out imgurl);
                    if (list.Count == 0) list = GetCloudMusicPlayListItems(RankinListAPI[type][listType], out name, out imgurl);
                    break;
                case NetMusicType.QQMusic:
                    retStr = SendDataByGET(RankinListAPI[type][listType]);
                    list = GetQQMusicPlayListItemsFromRetStr(retStr, out name, out imgurl, true);
                    break;
                case NetMusicType.XiaMiMusic:
                    retStr = SendDataByGET(RankinListAPI[type][listType]);
                    list = GetXiaMiMusicPlayListItemsFromRetStr(retStr);
                    break;

            }
            return list;
        }


        private static List<NetMusic> GetXiaMiMusicPlayListItemsFromRetStr(string retStr)
        {
            var list = new List<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["data"];
                foreach (var item in jt)
                {
                    var music = new NetMusic();
                    music.Title = item["song_name"].ToString();
                    music.Singer = item["singers"].ToString();
                    music.MusicID = item["song_id"].ToString();
                    music.Origin = NetMusicType.XiaMiMusic;
                    list.Add(music);
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        private static string GetMusicUrlOfLocal(NetMusic netMusic)
        {
            try
            {
                var Url = GetUrlByNetMusic(null,netMusic);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(Url, netMusic.MusicID));

                //request.Referer = "http://music.163.com/";
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";
                request.Timeout = 5000;
                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (Exception)
                {
                    var retString = "";// SendDataByGET(string.Format(DownloadLinkAPI[NetMusicType.XiaMiMusic], netMusic.Title, netMusic.Album ,netMusic.Singer));
                    if (string.IsNullOrWhiteSpace(retString))
                    {
                        return "";
                    }
                    var url = GetNetMusicLinkFromXiaMi(retString);
                    request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";
                    response = (HttpWebResponse)request.GetResponse();
                }


                Stream stream = response.GetResponseStream();
                if (!Directory.Exists("DownLoad\\Music\\"))//如果不存在就创建文件夹
                {
                    Directory.CreateDirectory("DownLoad\\Music\\");
                }
                var path = "DownLoad\\Music\\" + netMusic.Title + " - " + netMusic.Singer + ".mp3";
                if (File.Exists(Path.GetFullPath(path)))
                {
                    stream.Close();
                    return Path.GetFullPath(path);
                }
                StreamWriter sw = new StreamWriter(path);
                stream.CopyTo(sw.BaseStream);

                sw.Flush();
                sw.Close();

                stream.Close();
                return Path.GetFullPath(path);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string SendDataByPOST(string Url)	//读取string
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

                request.Referer = "http://music.163.com/";
                request.Method = "POST";
                request.ContentType = "text/html;charset=UTF-8";
                request.Timeout = 5000;
                request.ReadWriteTimeout = 5000;
                request.Headers.Add("Cookie", "appver=1.5.0.75771");


                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
            catch (Exception)
            {
                return "";
            }
            
        }

        private static void GetXMToken()
        {
            HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.xiami.com");
            myHttpWebRequest.CookieContainer = new CookieContainer(); //暂存到新实例</span>  
            myHttpWebRequest.GetResponse().Close();
            var cookies = myHttpWebRequest.CookieContainer.GetCookies(myHttpWebRequest.RequestUri);
            XMToken = cookies["xm_sg_tk"]?.Value ?? "";
            XMTokenSig = cookies["xm_sg_tk.sig"]?.Value ?? "";
            var ss = XMToken.Split('_');
            if(ss.Length > 0)
            {
                XMTokenPre = ss[0];
            }
        }

        private static string SendDataByGET(string Url)	//读取string
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (Url.Contains("i.y.qq.com"))
            {
                request.Referer = "http://y.qq.com";
            }
            else if (Url.Contains(".com"))
            {
                request.Referer = Url.Substring(0, Url.IndexOf(".com") + 4);
            }
            request.Method = "GET";
            //request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36";
            if (Url.Contains("xiami.com/api"))
            {
                request.Headers.Add("Cookie", $"xm_sg_tk={XMToken}; xm_sg_tk.sig={XMTokenSig}; ");
            }
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        private static ObservableCollection<NetMusic> GetNetMusicListBySources(string Sources, NetMusicType type, out int count)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return GetCloudMusicList(Sources, out count);
                case NetMusicType.QQMusic:
                    return GetQQmusicList(Sources, out count);
                case NetMusicType.XiaMiMusic:
                    return GetXiaMimusicList(Sources, out count);
            }
            count = 0;
            return new ObservableCollection<NetMusic>();
        }

        private static string GetNetMusicLinkFromXiaMi(string Sources)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(Sources);
            try
            {
                return jo["songList"][0]["file"].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static ObservableCollection<NetMusic> GetXiaMimusicList(string JsonStr, out int count)
        {
            var list = new ObservableCollection<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(JsonStr);
                var jt = jo["data"]["songs"];
                count = int.Parse(jo["data"]["total"].ToString());
                foreach (var item in jt)
                {
                    var music = new NetMusic();
                    music.Title = item["song_name"].ToString();
                    music.Singer = item["artist_name"].ToString();
                    music.MusicID = item["song_id"].ToString();
                    music.Album = item["album_name"].ToString();
                    music.AlbumImageUrl = item["album_logo"].ToString();
                    music.Origin = NetMusicType.XiaMiMusic;
                    music.Url = item["listen_file"].ToString();
                    music.LyricPath = item["lyric"].ToString();
                    //music.Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["size128"].ToString()));
                    list.Add(music);
                }
            }
            catch (Exception)
            {
                count = 0;
            }
            return list;
        }

        private static ObservableCollection<NetMusic> GetQQmusicList(string jsonStr, out int count)
        {
            var list = new ObservableCollection<NetMusic>();
            jsonStr = Regex.Replace(jsonStr, "^[a-zA-Z]{0,10}[C|c]allback\\(", "");
            jsonStr = Regex.Replace(jsonStr, ";$", "");
            jsonStr = Regex.Replace(jsonStr, "\\)$", "");
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(jsonStr);
                var jt = jo["data"]["song"]["list"];
                count = int.Parse(jo["data"]["song"]["totalnum"].ToString());
                foreach (var item in jt)
                {
                    var music = new NetMusic();
                    music.Title = item["songname"].ToString();
                    music.Singer = item["singer"][0]["name"].ToString();
                    music.MusicID = item["songmid"].ToString();
                    music.Album = item["albumname"].ToString();
                    var AlbumMid = item["albummid"].ToString();
                    var s = AlbumMid[AlbumMid.Length - 2] + "/" + AlbumMid[AlbumMid.Length - 1] + "/" + AlbumMid;
                    music.AlbumImageUrl = "http://imgcache.qq.com/music/photo/mid_album_300/" + s + ".jpg";
                    music.Origin = NetMusicType.QQMusic;
                    music.Url = "";
                    music.Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["size128"].ToString()) / 16);
                    list.Add(music);
                }
            }
            catch (Exception)
            {
                count = 0;
            }
            return list;
        }
        private static ObservableCollection<NetMusic> GetCloudMusicList(string JsonStr, out int count)
        {
            var list = new ObservableCollection<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(JsonStr);
                var jt = jo["result"]["songs"];
                count = int.Parse(jo["result"]["songCount"].ToString());
                foreach (var item in jt)
                {
                    var music = new NetMusic();
                    music.Title = item["name"].ToString();
                    music.Singer = item["artists"][0]["name"].ToString();
                    music.MusicID = item["id"].ToString();
                    music.Album = item["album"]["name"].ToString();
                    music.AlbumImageUrl = item["album"]["picUrl"].ToString();//"picUrl": "http://p1.music.126.net/B1ePGczwQUZueJl70TITWQ==/3287539775420245.jpg"
                    music.Origin = NetMusicType.CloudMusic;
                    music.Url = "";// item["mp3Url"].ToString().Replace("m2.music.126.net", "p2.music.126.net");
                    music.Duration = new TimeSpan(0,0,0,0,int.Parse(item["duration"].ToString()));
                    try
                    {
                        music.Remark = item["hMusic"]["dfsId"].ToString();
                    }
                    catch (Exception)
                    {
                        try
                        {
                            music.Remark = item["mMusic"]["dfsId"].ToString();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                music.Remark = item["lMusic"]["dfsId"].ToString();
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    music.Remark = item["bMusic"]["dfsId"].ToString();
                                }
                                catch (Exception)
                                {
                                    
                                }
                            }
                        }
                    }
                    list.Add(music);
                }
            }
            catch (Exception)
            {
                count = 0;
            }
            return list;
        }

        public static List<Playlist> GetPlayList(int offset, NetMusicType type, out int count)
        {
            var retStr = string.Empty;
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    retStr = SendDataByGET(string.Format(PlayListHotAPI[type], offset*35));
                    return GetCloudMusicPlayList(retStr, out count);
                case NetMusicType.QQMusic:
                    retStr = SendDataByGET(string.Format(PlayListHotAPI[type], offset*35, offset * 35+34));
                    return GetQQMusicPlayList(retStr, out count);
                case NetMusicType.XiaMiMusic:
                    GetXMToken();
                    var _p = XMTokenPre+"_xmMain_/api/list/collect_{\"pagingVO\":{\"page\":" + (offset + 1) + ",\"pageSize\":30},\"dataType\":\"system\"}";
                    retStr = SendDataByGET(string.Format(PlayListHotAPI[type], offset +1, GetMD5(_p).ToLower()));
                    return GetXimaMiMusicPlayListByJson(retStr, out count);
            }
            count = 0;
            return new List<Playlist>();
        }

        private static string GetMD5(string param)
        {
            byte[] result = Encoding.Default.GetBytes(param);    //tbPass为输入密码的文本框
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "");
        }

        private static List<Playlist> GetXimaMiMusicPlayListByJson(string retStr, out int count)
        {
            count = 0;
            List<Playlist> list = new List<Playlist>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["result"]["data"];
                int.TryParse(jt["pagingVO"]["pages"].ToString(), out count);
                var collects = jt["collects"];
                foreach (var item in collects)
                {
                    var model = new Playlist();
                    model.Name = item["collectName"].ToString();
                    model.ImgUrl = item["collectLogo"].ToString().Replace("http://", "https://")+ "@0e_0c_0i_1o_100Q_192w.webp";
                    model.Url = item["listId"].ToString();
                    list.Add(model);
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        private static List<Playlist> GetXimaMiMusicPlayList(string retStr, out int count)
        {
            count = 0;
            List<Playlist> list = new List<Playlist>();
            try
            {
                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(retStr);
                var collection = htmlDocument.DocumentNode.SelectNodes("//div[@class='block_list clearfix']/ul/li");
                var Sum = 0;
                var sumStr = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='all_page']/span").InnerText;
                sumStr = Regex.Match(sumStr, "共([\\d]+)条").Groups[1].Value;
                int.TryParse(sumStr, out Sum);
                count = Sum / 30 + (Sum % 30 == 0 ? 0 : 1);
                foreach (var item in collection)
                {
                    HtmlDocument nodeDocument = new HtmlDocument();
                    nodeDocument.LoadHtml(item.InnerHtml);
                    var a = nodeDocument.DocumentNode.SelectSingleNode("//div[@class='block_cover']/a");
                    string Name = a?.GetAttributeValue("title", "");
                    string ImgUrl = a.SelectSingleNode("//img").GetAttributeValue("src","");
                    if (ImgUrl.StartsWith("//"))
                    {
                        ImgUrl = "https:" + ImgUrl;
                    }
                    string Url = Regex.Match(a?.GetAttributeValue("href", ""), "/collect/([\\d]+)").Groups[1].Value;
                    list.Add(new Playlist(Name, ImgUrl, Url));
                }
            }
            catch (Exception ex)
            {
                var s = ex;
            }
            return list;
        }

        private static List<Playlist> GetQQMusicPlayList(string retStr, out int count)
        {
            count = 0;
            List<Playlist> list = new List<Playlist>();
            retStr = Regex.Replace(retStr, "^[a-zA-Z]{0,10}[C|c]allback\\(", "");
            retStr = Regex.Replace(retStr, ";$", "");
            retStr = Regex.Replace(retStr, "\\)$", "");
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["data"]["list"];
                var Sum = 0;
                int.TryParse(jo["data"]["sum"].ToString(),out Sum);
                count = Sum / 30 + (Sum % 30 == 0 ? 0 : 1);
                foreach (var item in jt)
                {
                    var model = new Playlist();
                    model.Name = item["dissname"].ToString();
                    model.ImgUrl = item["imgurl"].ToString().Replace("600?n=1", "150?n=1").Replace("http://","https://");
                    model.Url = item["dissid"].ToString();
                    list.Add(model);
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        private static List<Playlist> GetCloudMusicPlayList(string retStr, out int count)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(retStr);
            count = 0;
            List<Playlist> list = new List<Playlist>();
            try
            {
                var collection = htmlDocument.DocumentNode.SelectNodes("//ul[@id='m-pl-container']/li");
                int.TryParse(htmlDocument.DocumentNode.SelectNodes("//a[@class='zpgi']")?.LastOrDefault()?.InnerText, out count);
                foreach (var item in collection)
                {
                    HtmlDocument nodeDocument = new HtmlDocument();
                    nodeDocument.LoadHtml(item.InnerHtml);
                    string ImgUrl = nodeDocument.DocumentNode.SelectNodes("//img[@class='j-flag']").FirstOrDefault()?.GetAttributeValue("src", "");
                    var a = nodeDocument.DocumentNode.SelectNodes("//a[@class='msk']").FirstOrDefault();
                    string Name = a?.GetAttributeValue("title", "");
                    string Url = "https://music.163.com" + a?.GetAttributeValue("href", "");
                    list.Add(new Playlist(Name, ImgUrl, Url));
                }
            }
            catch (Exception)
            {                
            }
            return list;
        }

        public static List<NetMusic> GetPlayListItems(string url, NetMusicType type)
        {
            string name = string.Empty, imgurl = string.Empty;
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return GetCloudMusicPlayListItems(url, out name,out imgurl);
                case NetMusicType.QQMusic:
                    return GetQQMusicPlayListItems(url, out name, out imgurl);
                case NetMusicType.XiaMiMusic:
                    return GetXiaMiMusicPlayListItems(url, out name, out imgurl);
            }
            return new List<NetMusic>();
        }

        public static List<NetMusic> GetPlayListItems(string url, NetMusicType type, out string name, out string imgurl)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return GetCloudMusicPlayListItems(url, out name, out imgurl);
                case NetMusicType.QQMusic:
                    return GetQQMusicPlayListItems(url, out name, out imgurl);
                case NetMusicType.XiaMiMusic:
                    return GetXiaMiMusicPlayListItems(url, out name, out imgurl);
            }
            name = "";imgurl = "";
            return new List<NetMusic>();
        }

        private static List<NetMusic> GetXiaMiMusicPlayListItems(string pid, out string name, out string imgurl)
        {
            name = ""; imgurl = "";
            if (Regex.IsMatch(pid, "[a-zA-z]+://[^\\s]*"))
            {
                pid = Regex.Match(pid, "/([\\d]+)").Groups[1].Value;
            }
            var _p = XMTokenPre+"_xmMain_/api/collect/initialize_{\"listId\":" + pid +"}";
            var url = string.Format(PlayListDetailAPI[NetMusicType.XiaMiMusic], pid, GetMD5(_p).ToLower());
            var retStr = SendDataByGET(url);
            var list = new List<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["result"]["data"];
                var collectDetail = jt["collectDetail"];
                name = collectDetail["collectName"].ToString();
                imgurl = collectDetail["collectLogoSmall"].ToString();
                foreach (var item in jt["collectSongs"])
                {
                    var music = new NetMusic();
                    music.Title = item["songName"].ToString();
                    music.Singer = item["singers"].ToString();
                    music.MusicID = item["songId"].ToString();
                    music.Album = item["albumName"].ToString();
                    music.AlbumImageUrl = item["albumLogo"].ToString()+ "@0e_0c_0i_1o_100Q_192w.webp";
                    music.Origin = NetMusicType.XiaMiMusic;
                    music.Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["length"].ToString()));
                    list.Add(music);
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        private static List<NetMusic> GetQQMusicPlayListItems(string ssid, out string name, out string imgurl)
        {
            name = ""; imgurl = "";
            if (Regex.IsMatch(ssid, "[a-zA-z]+://[^\\s]*"))
            {
                ssid = Regex.Match(ssid, "/([\\d]+)").Groups[1].Value;
            }
            var url = string.Format(PlayListDetailAPI[NetMusicType.QQMusic], ssid);
            var retStr = SendDataByGET(url);

            return GetQQMusicPlayListItemsFromRetStr(retStr, out name, out imgurl);
        }
        private static List<NetMusic> GetQQMusicPlayListItemsFromRetStr(string retStr, out string name, out string imgurl, bool isRl = false)
        {
            name = "";imgurl = "";
            var list = new List<NetMusic>();
            retStr = Regex.Replace(retStr, "^[a-zA-Z]{0,10}[C|c]allback\\(", "");
            retStr = Regex.Replace(retStr, ";$", "");
            retStr = Regex.Replace(retStr, "\\)$", "");
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                JToken jt;
                if (isRl)
                {
                    jt = jo["songlist"];
                }
                else
                {
                    jt = jo["cdlist"][0]["songlist"];
                    name = jo["cdlist"][0]["dissname"].ToString();
                    imgurl = jo["cdlist"][0]["logo"].ToString().Replace("300?n=1", "150?n=1");
                }
                
                foreach (var item0 in jt)
                {
                    JToken item = item0;
                    if (isRl) item = item0["data"];
                    var music = new NetMusic();
                    music.Title = item["songname"].ToString();
                    music.Singer = item["singer"][0]["name"].ToString();
                    music.MusicID = item["songmid"].ToString();
                    music.Album = item["albumname"].ToString();
                    var AlbumMid = item["albummid"].ToString();
                    var s = AlbumMid[AlbumMid.Length - 2] + "/" + AlbumMid[AlbumMid.Length - 1] + "/" + AlbumMid;
                    music.AlbumImageUrl = "http://imgcache.qq.com/music/photo/mid_album_300/" + s + ".jpg";
                    music.Origin = NetMusicType.QQMusic;
                    music.Url = "";
                    music.Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["size128"].ToString()) / 16);
                    list.Add(music);
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        private static List<NetMusic> GetCloudMusicPlayListItems(string playlistUrl, out string name,out string imgurl)
        {
            name = ""; imgurl = "";
            List<NetMusic> list = new List<NetMusic>();
            string id = Regex.IsMatch(playlistUrl, "^[\\d]+$") ? playlistUrl : Regex.Match(playlistUrl, "playlist\\?id=([\\d]+)").Groups[1].Value;
            var param = AesEncrypt("{\"id\":\"" + id + "\",\"offset\":0,\"total\":true,\"limit\":1000,\"n\":1000,\"csrf_token\":\"\"}", "0CoJUm6Qyw8W8jud");
            //param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = AesEncrypt(param, "t9Y0m4pdsoMznMlL");
            param = System.Web.HttpUtility.UrlEncode(param);
            //var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var encSecKey = "&encSecKey=409afd10f2fa06173df57525287c4a1cdf6fa08bd542c6400da953704eb92dc1ad3c582e82f51a707ebfa0f6a25bcd185139fc1509d40dd97b180ed21641df55e90af4884a0b587bd25256141a9270b1b6f18908c6a626b74167e5a55a796c0f808a2eb12c33e63d34a7c4d358bab1dc661637dd1e888a1268b81a89f6136053";
            var url = PlayListDetailAPI[NetMusicType.CloudMusic];
            var paramData = "params=" + param + encSecKey;
            var retStr = CloudSendDataByPost(url, paramData, false);
            if (retStr.Length < 50)
            {
                return list;
            }
            JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
            var jt = jo["playlist"]["tracks"];
            name = jo["playlist"]["name"].ToString();
            imgurl = jo["playlist"]["coverImgUrl"].ToString();
            foreach (var item in jt)
            {
                var music = new NetMusic();
                music.Title = item["name"].ToString();
                music.Singer = item["ar"][0]["name"].ToString();
                music.MusicID = item["id"].ToString();
                music.Album = item["al"]["name"].ToString();
                music.AlbumImageUrl = item["al"]["picUrl"].ToString();
                music.Origin = NetMusicType.CloudMusic;
                music.Url = "";
                music.Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["dt"].ToString()));
                list.Add(music);
            }
            return list;
        }

        private static string CloudSendDataByPost(string Url, string paramData, bool IsUrl = true)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            //Url = Url.Replace("http://", "https://");
            //byte[] byteArray = dataEncode.GetBytes(paramData); //转化
            request.Referer = "https://music.163.com";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.Timeout = 5000;
            //request.ReadWriteTimeout = 5000;
            request.UserAgent = "Mozilla/5.0 (iPad; CPU OS 11_0 like Mac OS X) AppleWebKit/604.1.34 (KHTML, like Gecko) Version/11.0 Mobile/15A5341f Safari/604.1";

            request.Headers.Add("Cookie", "os=ios;");
            var a = Encoding.UTF8.GetBytes(paramData);
            request.ContentLength = a.Length;
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(a, 0, a.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                if (IsUrl)
                {
                    var musicUrl = Regex.Match(retString, "\"url\":\"([^ \"]*)\"").Groups[1].Value;
                    return musicUrl;
                    //JObject jo = (JObject)JsonConvert.DeserializeObject(retString);
                    //var jt = jo["data"][0]["url"];
                    //return jt.ToString();
                }
                else
                {
                    return retString;
                }
            }
            catch (Exception)
            {
            }

            return "";
        }
        private static string GetUrlFromCloudMusic(Music music)
        {
            string Url = GetNewUrlFromCloudMusic("http://music.163.com/song/media/outer/url?id=" + music.MusicID);
            if (!string.IsNullOrWhiteSpace(Url)) return Url;
            var param = AesEncrypt("{\"ids\":[" + music.MusicID + "],\"br\":320000,\"csrf_token\":\"\"}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = "http://music.163.com/weapi/song/enhance/player/url?csrf_token=";
            var paramData = "params=" + param + encSecKey;
            Url = CloudSendDataByPost(url, paramData);
            if (string.IsNullOrWhiteSpace(Url))
            {
                Url = CloudSendDataByPost(url, paramData);
            }
            if (false && string.IsNullOrWhiteSpace(Url))
            {
                if (CheckLink(music.Path))
                {
                    Url = music.Path;
                }
                //else
                //{
                //    Url = ""; GetOldUrlFromCloudMusic(music);
                //    if (!CheckLink(Url))
                //    {
                //        var retString = "";// SendDataByGET(string.Format(DownloadLinkAPI[NetMusicType.XiaMiMusic], music.Title, music.Album, music.Singer));
                //        if (string.IsNullOrWhiteSpace(retString))
                //        {
                //            return "";
                //        }
                //        Url = GetNetMusicLinkFromXiaMi(retString);
                //        if (!CheckLink(Url))
                //        {
                //            Url = "";
                //        }
                //    }
                //}
            }
            return Url;
        }
        private static string GetUrlFromQQMusic(Music music)
        {
            var Url = "";
            //var retStr = SendDataByGET("http://base.music.qq.com/fcgi-bin/fcg_musicexpress.fcg?json=3&guid=4935867420&g_tk=938407465&loginUin=0&hostUin=0&format=jsonp&inCharset=GB2312&outCharset=GB2312&notice=0&platform=yqq&needNewCode=0");
            try
            {
                    //Url = $"http://ws.stream.qqmusic.qq.com/C100{music.MusicID}.m4a?fromtag=0&guid=126548448";
                var t = DateTime.Now.Millisecond;
                var guid = ((int)Math.Abs((int)Math.Round(2147483647 * new Random().NextDouble()) * t % 1e10)).ToString();
                string callback = "getplaysongvkey" + (new Random().NextDouble() + "").Replace("0.", "");
                string data = $"{{\"req\":{{\"module\":\"CDN.SrfCdnDispatchServer\",\"method\":\"GetCdnDispatch\",\"param\":{{\"guid\":\"{guid}\",\"calltype\":0,\"userip\":\"\"}}}},\"req_0\":{{\"module\":\"vkey.GetVkeyServer\",\"method\":\"CgiGetVkey\",\"param\":{{\"guid\":\"{guid}\",\"songmid\":[\"{music.MusicID}\"],\"songtype\":[0],\"uin\":\"0\",\"loginflag\":1,\"platform\":\"20\"}}}},\"comm\":{{\"uin\":0,\"format\":\"json\",\"ct\":20,\"cv\":0}}}}";
                data = System.Web.HttpUtility.UrlEncode(data);
                Url = $"https://u.y.qq.com/cgi-bin/musicu.fcg?callback={callback}&g_tk=5381&jsonpCallback={callback}&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&data={data}";
                var retStr = SendDataByGET(Url).Substring(callback.Length+1);
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr.Substring(0,retStr.Length-1));
                Url = "http://dl.stream.qqmusic.qq.com/"+ jo["req_0"]["data"]["midurlinfo"][0]["purl"];
            }
            catch (Exception)
            {
            }
            return Url;
        }
        private static string GetUrlFromXiaMiMusic(Music music)
        {
            //var retStr = SendDataByGET($"http://api.xiami.com/web?v=2.0&app_key=1&id={music.MusicID}&r=song/detail");
            var _p = XMTokenPre + "_xmMain_/api/song/getPlayInfo_{\"songIds\":[" + music.MusicID + "]}";
            var retStr = SendDataByGET($"https://www.xiami.com/api/song/getPlayInfo?_q=%7B%22songIds%22:["+music.MusicID+"]%7D&_s="+ GetMD5(_p).ToLower());
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                JToken songPlayInfos = jo["result"]["data"]["songPlayInfos"][0]["playInfos"];
                foreach (var item in songPlayInfos)
                {
                    var listenFile = item["listenFile"].ToString();
                    if(listenFile.Length > 0)
                    {
                        music.Path = listenFile;
                        break;
                    }
                }
                //music.Album = item["album_name"].ToString();
                //music.AlbumImageUrl = item["logo"].ToString();
                ////music.Origin = NetMusicType.XiaMiMusic;
                //music.Path = item["listen_file"].ToString();
                //music.LyricPath = item["lyric"].ToString();
                //string secondstr = Regex.Match(music.LyricPath, "/([\\d]{2,4})/").Groups[1].Value;
                //music.Duration = new TimeSpan(0, 0, 0, int.Parse(secondstr), 0);
            }
            catch (Exception)
            {
            }
            return music.Path;
        }

        private static string AesEncrypt(string plaintextData, string key, string iv = "0102030405060708")
        {
            try
            {
                if (string.IsNullOrEmpty(plaintextData)) return null;
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(plaintextData);
                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes(key),
                    IV = Encoding.UTF8.GetBytes(iv),
                    Mode = CipherMode.CBC
                };
                ICryptoTransform cTransform = rm.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string GetOldUrlFromCloudMusic(NetMusic music)
        {
            var input = music.Remark;
            string key = "3go8&$8*3*3h0k(2)2";
            byte[] keyBytes = Encoding.Default.GetBytes(key);
            byte[] searchBytes = Encoding.Default.GetBytes(input);

            for (int i = 0; i < searchBytes.Length; ++i)
            {
                searchBytes[i] ^= keyBytes[i % keyBytes.Length];
            }

            var md5 = new MD5CryptoServiceProvider().ComputeHash(searchBytes);
            var result = Convert.ToBase64String(md5);
            result = result.Replace("+", "-");
            result = result.Replace("/", "_");

            return "http://p2.music.126.net/" + result + "/" + input + ".mp3";
        }
        public static string GetNewUrlFromCloudMusic(string url)
        {
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(url);
            httpReq.AllowAutoRedirect = false;
            using(HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse())
            {
                try
                {
                    url = Regex.Replace(httpRes.Headers["Location"], "^http","https");
                }
                catch (Exception)
                {
                    url = "";
                }
            }

            return url;
        }
        /// <summary>
        /// 获取指定期第几周
        /// </summary>
        /// <param name="dt">指定间</param>
        /// <reutrn>返第几周</reutrn>
        private static int GetWeekOfYear(DateTime dt)
        {
            GregorianCalendar gc = new GregorianCalendar();
            int weekOfYear = gc.GetWeekOfYear(dt, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return weekOfYear;
        }
    }
    public enum NetMusicType
    {
        LocalMusic,CloudMusic, QQMusic, XiaMiMusic, BaiduMusic, KuwoMusic, KuGouMusic
    }
    public enum RankingListType
    {
        SoarList,HotList,NewSongList
    }
}
