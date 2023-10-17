using HtmlAgilityPack;
using MusicCollection.MusicManager;
using MusicCollection.Setting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenCvSharp.ML;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace MusicCollection.MusicAPI
{
    class NetMusicHelper
    {
        public static string NetEasyCsrfToken = "";
        public static string NetEasyMusicU = "";
        public static string QQMusicU = "";
        public static string MiguMusicU = "";

        private static readonly Dictionary<NetMusicType, string> SearchAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "https://interface.music.163.com/weapi/search/get" },
            { NetMusicType.QQMusic, "http://i.y.qq.com/s.music/fcgi-bin/search_for_qq_cp?g_tk=938407465&uin=0&format=jsonp&inCharset=utf-8&outCharset=utf-8&notice=0&platform=h5&needNewCode=1&w={0}&zhidaqu=1&catZhida=1&t=0&flag=1&ie=utf-8&sem=1&aggr=0&perpage=30&n=30&p={1}&remoteplace=txt.mqq.all&_=1459991037831" },
            { NetMusicType.MiguMusic, "https://m.music.migu.cn/migumusic/h5/search/all?text={0}&pageNo={1}&pageSize=30" }
        };

        private static readonly Dictionary<NetMusicType, string> LyricAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/api/song/lyric?os=pc&id={0}&lv=-1&kv=-1&tv=-1" },
            { NetMusicType.QQMusic,"https://i.y.qq.com/lyric/fcgi-bin/fcg_query_lyric.fcg?songmid={0}&loginUin=0&hostUin=0&format=jsonp&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" }
        };

        private static readonly Dictionary<NetMusicType, string> DownloadLinkAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://link.hhtjim.com/163/{0}.mp3" },
            { NetMusicType.MiguMusic, "https://music-api-jwzcyzizya.now.sh/api/search/song/xiami?&limit=1&page=1&key={0}-{1}-{2}/" }
        };

        private static readonly Dictionary<NetMusicType, string> PlayListHotAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/discover/playlist/?order=hot&limit=35&offset={0}" },
            { NetMusicType.QQMusic, "https://c.y.qq.com/splcloud/fcgi-bin/fcg_get_diss_by_tag.fcg?rnd=0.4781484879517406&g_tk=732560869&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&categoryId=10000000&sortId=5&sin={0}&ein={1}" },
            //{ NetMusicType.XiaMiMusic, "http://www.xiami.com/collect/recommend/page/{0}" }
            { NetMusicType.MiguMusic, "https://app.c.nf.migu.cn/MIGUM2.0/v1.0/template/musiclistplaza-listbytag/release?pageNumber={0}&tagId=1003449976" }
        };
        private static readonly Dictionary<NetMusicType, string> PlayListDetailAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "https://music.163.com/weapi/v3/playlist/detail" },
            { NetMusicType.QQMusic, "https://i.y.qq.com/qzone-music/fcg-bin/fcg_ucc_getcdinfo_byids_cp.fcg?type=1&json=1&utf8=1&onlysong=0&nosign=1&disstid={0}&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=GB2312&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" },
            //{ NetMusicType.XiaMiMusic, "http://api.xiami.com/web?v=2.0&app_key=1&id={0}&r=collect/detail" }
            { NetMusicType.MiguMusic, "https://m.music.migu.cn/migumusic/h5/playlist/songsInfo?pageNo=1&pageSize=500&palylistId=" }
        };
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
                    { RankingListType.HotList, $"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&date={DateTime.Now.AddDays(DateTime.Now.Hour < 10 ? -2 : -1).ToString("yyyy-MM-dd")}&topid=26&type=top&song_begin=0&song_num=100&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" },
                    { RankingListType.NewSongList, $"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&date={DateTime.Now.AddDays(DateTime.Now.Hour < 10 ? -2 : -1).ToString("yyyy-MM-dd")}&topid=27&type=top&song_begin=0&song_num=100&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" },
                    { RankingListType.SoarList, $"https://c.y.qq.com/v8/fcg-bin/fcg_v8_toplist_cp.fcg?tpl=3&page=detail&date={DateTime.Now.AddDays(DateTime.Now.Hour < 10 ? -2 : -1).ToString("yyyy-MM-dd")}&topid=4&type=top&song_begin=0&song_num=100&g_tk=5381&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0" }
                }
            },
            {
                NetMusicType.MiguMusic,
                new Dictionary<RankingListType, string>()
                {
                    { RankingListType.HotList, "https://app.c.nf.migu.cn/MIGUM3.0/column/rank/h5/v1.0?columnId=27186466" },
                    { RankingListType.NewSongList, "https://app.c.nf.migu.cn/MIGUM3.0/column/rank/h5/v1.0?columnId=27553319" },
                    { RankingListType.SoarList, "https://app.c.nf.migu.cn/MIGUM3.0/column/rank/h5/v1.0?columnId=27553408" }
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
                case NetMusicType.MiguMusic:
                    Url = GetUrlFromMiguMusic(music);
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
            if (url.EndsWith("/404")) return false;
            HttpWebRequest req = null;
            try
            {
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "HEAD";  //这是关键        
                req.Timeout = 100;
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
                string[] LyricLine = LyricStr.Split('\n');
                for (int i = 0; i < LyricLine.Length; i++)
                {
                    if (LyricLine[i].Contains("[x-trans]"))
                    {
                        var sss = Regex.Match(LyricLine[i-1], "(\\[[\\d|:|.]+\\])").Groups[1].Value;
                        LyricLine[i] = LyricLine[i].Replace("[x-trans]", Regex.Match(LyricLine[i-1], "(\\[[\\d|:|.]+\\])").Groups[1].Value);
                    }
                }
                LyricPath = Path.Combine(EnvironmentSingle.DownloadLyricPath, $"{music.Title} - {music.Singer}.lrc");
                File.WriteAllLines(LyricPath, LyricLine);
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
                LyricPath = Path.Combine(EnvironmentSingle.DownloadLyricPath, $"{music.Title} - {music.Singer}.lrc");
                File.WriteAllText(LyricPath, LyricStr);
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
                    string[] LyricLine = AnalyzeLyric(LyricStr, music.Origin);
                    LyricPath = Path.Combine(EnvironmentSingle.DownloadLyricPath, $"{music.Title} - {music.Singer}.lrc");
                    File.WriteAllLines(LyricPath, LyricLine);
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
                var LyricLine = lyricStr.Split('\n');
                return LyricLine;
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
                var LyricLine = lyricStr.Split('\n');
                return LyricLine;
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
                    var param = AesEncrypt("{\"s\":\"" + SearchStr + "\",\"limit\":30,\"offset\":" + offset * 30 + ",\"type\":1,\"strategy\":5,\"queryCorrect\":true}", "0CoJUm6Qyw8W8jud");
                    //param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
                    param = AesEncrypt(param, "t9Y0m4pdsoMznMlL");
                    param = System.Web.HttpUtility.UrlEncode(param);
                    //var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
                    var encSecKey = "&encSecKey=409afd10f2fa06173df57525287c4a1cdf6fa08bd542c6400da953704eb92dc1ad3c582e82f51a707ebfa0f6a25bcd185139fc1509d40dd97b180ed21641df55e90af4884a0b587bd25256141a9270b1b6f18908c6a626b74167e5a55a796c0f808a2eb12c33e63d34a7c4d358bab1dc661637dd1e888a1268b81a89f6136053";
                    var paramData = "params=" + param + encSecKey;
                    retString = CloudSendDataByPost(SearchAPI[type], paramData, false);
                    break;
                case NetMusicType.QQMusic:
                    retString = SendDataByGET(string.Format(SearchAPI[type], SearchStr, offset + 1));
                    break;
                case NetMusicType.MiguMusic:
                    Dictionary<string, string> headers = new Dictionary<string, string>()
                    {
                        { "by","22210ca73bf1af2ec2eace74a96ee356"},
                        {"Referer", "https://m.music.migu.cn/v4/search" },
                        {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36" }
                    };
                    retString = SendDataByGET(string.Format(SearchAPI[type], SearchStr, offset + 1), headers);
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
                case NetMusicType.MiguMusic:
                    retStr = SendDataByGET(RankinListAPI[type][listType]);
                    list = GetMiguMusicTopPlayListItemsFromRetStr(retStr);
                    break;

            }
            return list;
        }


        private static List<NetMusic> GetMiguMusicTopPlayListItemsFromRetStr(string retStr)
        {
            var list = new List<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["data"]["columnInfo"]["contents"];
                foreach (var item in jt)
                {
                    var music = new NetMusic();
                    var mjs = item["objectInfo"];
                    music.Title = mjs["songName"].ToString();
                    music.Album = mjs["album"].ToString();
                    try
                    {
                        music.AlbumImageUrl = mjs["albumImgs"][0]["img"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    music.Singer = mjs["artists"][0]["name"].ToString();
                    music.MusicID = mjs["copyrightId"].ToString();
                    music.Remark = mjs["contentId"].ToString();
                    TimeSpan.TryParse(mjs["length"].ToString(), out TimeSpan duration);
                    music.Duration = duration;
                    music.Origin = NetMusicType.MiguMusic;
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

                var path = Path.Combine(EnvironmentSingle.DownloadMusicPath, netMusic.Title + " - " + netMusic.Singer + ".mp3");
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

        private static Thread imageDownloadThread = null;
        private static List<string> imageDownloadList = new List<string>();
        private static object ImageDownloadLock = new object();
        //Run in Main Thread
        public static string GetImgFromRemote(string imgUrl)
        {
            if (File.Exists(imgUrl)) return imgUrl;
            string hashPath = Path.Combine(EnvironmentSingle.AlbumImagePath, Utils.Md5Func(imgUrl));
            lock (ImageDownloadLock)
            {
                if (imageDownloadList.Contains(imgUrl)) return imgUrl;
                else if (File.Exists(hashPath)) return hashPath;
            }
            if (imgUrl.StartsWith("http")) return DownloadImage(imgUrl);
            else return imgUrl;
        }

        private static string DownloadImage(string imgUrl)
        {
            lock (ImageDownloadLock)
            {
                if (!imageDownloadList.Contains(imgUrl)) imageDownloadList.Add(imgUrl);
                if (imageDownloadThread == null || imageDownloadThread.ThreadState == ThreadState.Stopped)
                {
                    imageDownloadThread = new Thread(new ThreadStart(() => DownloadImageRemote())) { IsBackground = true };
                    imageDownloadThread.Start();
                }
            }
            return imgUrl;
        }

        private static void DownloadImageRemote()
        {
            int downloadCount = 0;
            lock (ImageDownloadLock)
            {
                downloadCount = imageDownloadList.Count;
            }
            while (downloadCount > 0)
            {
                string imgUrl = string.Empty;
                try
                {
                    lock (ImageDownloadLock)
                    {
                        imgUrl = imageDownloadList[0];
                    }
                    string hashPath = Path.Combine(EnvironmentSingle.AlbumImagePath, Utils.Md5Func(imgUrl));
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imgUrl);
                    request.Method = "GET";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";
                    request.Timeout = 5000;
                    HttpWebResponse response;
                    response = (HttpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    //创建本地文件写入流
                    Stream stream = new FileStream(hashPath + ".download", FileMode.Create);

                    byte[] bArr = new byte[1024];
                    int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    while (size > 0)
                    {
                        stream.Write(bArr, 0, size);
                        size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    }
                    stream.Close();
                    responseStream.Close();
                    File.Move(hashPath + ".download", hashPath);
                    Utils.CheckImageFileScale(hashPath);
                }
                catch (Exception) { }
                lock (ImageDownloadLock)
                {
                    if(imageDownloadList.Count > 0)
                    {
                        imageDownloadList.RemoveAt(0);
                    }
                    downloadCount = imageDownloadList.Count;
                }
            }
        }

        private static string SendDataByPOST(string Url, byte[] data=null, Dictionary<string, string> headers=null)	//读取string
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

                request.Method = "POST";
                request.ContentType = "text/html;charset=UTF-8";
                request.Timeout = 5000;
                request.ReadWriteTimeout = 5000;
                request.Headers.Add("Cookie", "appver=1.5.0.75771");
                SetHeaders(request, headers);
                if(string.IsNullOrEmpty(request.Referer)) request.Referer = "http://music.163.com/";
                if (data != null)
                {
                    request.ContentLength = data.Length;
                    using (var stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

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

        private static void SetHeaders(HttpWebRequest request, Dictionary<string, string> headers = null)
        {
            if (headers == null) return;
            foreach (var item in headers)
            {
                if (item.Key.ToLower() == "referer")
                {
                    request.Referer = item.Value;
                }
                else if (item.Key.ToLower().Replace("-", "") == "useragent")
                {
                    request.UserAgent = item.Value;
                }
                else if (item.Key.ToLower().Replace("-", "") == "contenttype")
                {
                    request.ContentType = item.Value;
                }
                else if (item.Key.ToLower() == "accept")
                {
                    request.Accept = item.Value;
                }
                else
                {
                    request.Headers.Set(item.Key, item.Value);
                }
            }
        }

        private static string SendDataByGET(string Url, Dictionary<string, string> headers = null)	//读取string
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
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36";
            SetHeaders(request, headers);
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                string retString = string.Empty;
                if (response.ContentEncoding?.ToLower() == "gzip")
                {
                    Stream gzipStream = new GZipStream(myResponseStream, CompressionMode.Decompress);
                    StreamReader myStreamReader = new StreamReader(gzipStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                    gzipStream.Close();
                }
                else
                {
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myStreamReader.Close();
                }
                myResponseStream.Close();
                return retString;
            }
            catch (Exception)
            {
                return "";
            }
        }
        private static ObservableCollection<NetMusic> GetNetMusicListBySources(string Sources, NetMusicType type, out int count)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return GetCloudMusicList(Sources, out count);
                case NetMusicType.QQMusic:
                    return GetQQMusicList(Sources, out count);
                case NetMusicType.MiguMusic:
                    return GetMiguMusicList(Sources, out count);
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

        private static ObservableCollection<NetMusic> GetMiguMusicList(string JsonStr, out int count)
        {
            var list = new ObservableCollection<NetMusic>();
            JObject jo = (JObject)JsonConvert.DeserializeObject(JsonStr);
            var jt = jo["data"]["songsData"]["items"];
            count = int.Parse(jo["data"]["songsData"]["total"].ToString());
            foreach (var item in jt)
            {
                var music = new NetMusic();
                music.Title = item["name"].ToString();
                music.Singer = item["singers"][0]["name"].ToString();
                music.MusicID = item["copyrightId"].ToString();
                music.Remark = item["fullSong"]["productId"].ToString();
                music.Origin = NetMusicType.MiguMusic;
                if(item["album"] != null && item["album"].HasValues)
                {
                    music.Album = item["album"]["name"].ToString();
                }
                if(item["largePic"] != null && item["largePic"].HasValues)
                {
                    music.AlbumImageUrl = item["largePic"].ToString();
                }
                //music.LyricPath = item["lyric"].ToString();
                //music.Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["size128"].ToString()));
                list.Add(music);
            }
            return list;
        }

        private static ObservableCollection<NetMusic> GetQQMusicList(string jsonStr, out int count)
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
                var jt = jo["result"]["songs"] as JArray;
                count = int.Parse(jo["result"]["songCount"].ToString());
                foreach (JObject item in jt)
                {
                    var music = new NetMusic();
                    music.Title = item["name"].ToString();
                    if(item["artists"] == null)
                    {
                        music.Singer = item["ar"][0]["name"].ToString();
                        music.Album = item["al"]["name"].ToString();
                        music.AlbumImageUrl = item["al"]["picUrl"].ToString();//"picUrl": "http://p1.music.126.net/B1ePGczwQUZueJl70TITWQ==/3287539775420245.jpg"
                        music.Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["dt"].ToString()));
                    }
                    else
                    {
                        music.Singer = item["artists"][0]["name"].ToString();
                        music.Album = item["album"]["name"].ToString();
                        music.AlbumImageUrl = item["album"]["picUrl"].ToString();//"picUrl": "http://p1.music.126.net/B1ePGczwQUZueJl70TITWQ==/3287539775420245.jpg"
                        music.Duration = new TimeSpan(0,0,0,0,int.Parse(item["duration"].ToString()));
                    }
                    music.MusicID = item["id"].ToString();
                    music.Origin = NetMusicType.CloudMusic;
                    music.Url = "";// item["mp3Url"].ToString().Replace("m2.music.126.net", "p2.music.126.net");
                    if (jo.ContainsKey("hMusic"))
                    {
                        music.Remark = item["hMusic"]["dfsId"].ToString();
                    }
                    else if (jo.ContainsKey("mMusic"))
                    {
                        music.Remark = item["mMusic"]["dfsId"].ToString();
                    }
                    else if (jo.ContainsKey("lMusic"))
                    {
                        music.Remark = item["lMusic"]["dfsId"].ToString();
                    }
                    else if (jo.ContainsKey("bMusic"))
                    {
                        music.Remark = item["bMusic"]["dfsId"].ToString();
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
                    return GetCloudMusicHtmlPlayList(retStr, out count);
                case NetMusicType.QQMusic:
                    retStr = SendDataByGET(string.Format(PlayListHotAPI[type], offset*35, offset * 35+34));
                    return GetQQMusicPlayList(retStr, out count);
                case NetMusicType.MiguMusic:
                    retStr = SendDataByGET(string.Format(PlayListHotAPI[type], offset +1));
                    return GetMiguMiMusicPlayList(retStr, out count);
            }
            count = 0;
            return new List<Playlist>();
        }

        public static List<Playlist> GetMyFavoritePlayList(NetMusicType type)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    string retStr = GetUserPlaylistFromCloudMusic();
                    return GetCloudMusicJsonPlayList(retStr);
                case NetMusicType.QQMusic:
                    return GetUserPlaylistFromQQMusic();
                case NetMusicType.MiguMusic:
                    return GetUserPlaylistFromMigu();
            }
            return new List<Playlist>();
        }

        private static string GetMD5(string param)
        {
            byte[] result = Encoding.Default.GetBytes(param);    //tbPass为输入密码的文本框
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "");
        }

        private static List<Playlist> GetMiguMiMusicPlayList(string retStr, out int count)
        {
            count = 0;
            List<Playlist> list = new List<Playlist>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["data"]["contentItemList"];
                count = 180;
                var collects = jt["itemList"];
                foreach (var item in collects)
                {
                    var model = new Playlist();
                    model.Name = item["title"].ToString();
                    model.ImgUrl = item["imageUrl"].ToString().Replace("http://", "https://");
                    model.Url = item["logEvent"]["contentId"].ToString();
                    list.Add(model);
                }
            }
            catch (Exception)
            {
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

        private static List<Playlist> GetCloudMusicHtmlPlayList(string retStr, out int count)
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
                case NetMusicType.MiguMusic:
                    return GetMiguMusicPlayListItems(url, out name, out imgurl);
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
                case NetMusicType.MiguMusic:
                    return GetMiguMusicPlayListItems(url, out name, out imgurl);
            }
            name = "";imgurl = "";
            return new List<NetMusic>();
        }

        private static List<NetMusic> GetMiguMusicPlayListItems(string pid, out string name, out string imgurl)
        {
            name = ""; imgurl = "";
            var url = PlayListDetailAPI[NetMusicType.MiguMusic]+pid;
            var headers = new Dictionary<string, string>()
            {
                {"By", "22210ca73bf1af2ec2eace74a96ee356"},
                {"Referer", "https://m.music.migu.cn/v4/music/playlist"},
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"}
            };
            var retStr = SendDataByGET(url, headers);
            var list = new List<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["data"];
                foreach (var item in jt["items"])
                {
                    var music = new NetMusic();
                    music.Title = item["name"].ToString();
                    try
                    {
                        music.Album = item["album"]["albumName"].ToString();
                        music.Singer = item["singers"][0]["name"].ToString();
                    }
                    catch (Exception)
                    {
                    }
                    music.MusicID = item["copyrightId"].ToString();
                    music.Origin = NetMusicType.MiguMusic;
                    try
                    {
                        music.Remark = item["fullSong"]["productId"].ToString();
                        music.AlbumImageUrl = item["mediumPic"].ToString();
                        if (music.AlbumImageUrl.StartsWith("//")) music.AlbumImageUrl = "https:" + music.AlbumImageUrl;
                        music.Duration = TimeSpan.Parse(item["duration"].ToString());
                    }
                    catch (Exception)
                    {
                    }
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
                    music.Remark = item["songid"].ToString();
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

        private static List<Playlist> GetCloudMusicJsonPlayList(string retStr)
        {
            JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
            List<Playlist> list = new List<Playlist>();
            try
            {
                var jt = jo["playlist"];
                foreach (var item in jt)
                {
                    string name = item["name"].ToString();
                    string imgurl = item["coverImgUrl"].ToString();
                    string url = "https://music.163.com/#/playlist?id=" + item["id"].ToString();
                    list.Add(new Playlist(name, imgurl, url));
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        private static List<Playlist> GetUserPlaylistFromQQMusic()
        {
            var url = "https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?cid=205360838&reqfrom=1";
            var headers = new Dictionary<string, string>()
            {
                {"cookie", QQMusicU},
            };
            var resultStr = SendDataByGET(url, headers);
            List<Playlist> list = new List<Playlist>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(resultStr);
                var myMusic = jo["data"]["mymusic"];
                var myDiss = jo["data"]["mydiss"]["list"];
                foreach (var item in myMusic)
                {
                    string name = item["title"].ToString();
                    string imgurl = item["picurl"].ToString();
                    string plUrl = item["id"].ToString();
                    list.Add(new Playlist(name, imgurl, plUrl));
                }
                foreach (var item in myDiss)
                {
                    string name = item["title"].ToString();
                    string imgurl = item["picurl"].ToString();
                    string plUrl = item["dissid"].ToString();
                    list.Add(new Playlist(name, imgurl, plUrl));
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        private static List<Playlist> GetUserPlaylistFromMigu()
        {
            var urlMy = "https://m.music.migu.cn/migumusic/h5/playlist/auth/getUserPlaylist";
            var urlCollection = "https://m.music.migu.cn/migumusic/h5/collection/auth/getCollectedSongLists?pageNo=1&pageSize=30";
            var headers = new Dictionary<string, string>()
            {
                {"by", "22210ca73bf1af2ec2eace74a96ee356"},
                {"Referer", "https://m.music.migu.cn/v4/my/collect"},
                {"Cookie", MiguMusicU},
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"},
            };
            List<string> resultStrs = new List<string>()
            {
                SendDataByGET(urlMy, headers), SendDataByGET(urlCollection, headers)
            };
            List<Playlist> list = new List<Playlist>();
            foreach (var retStr in resultStrs)
            {
                try
                {
                    JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                    var jt = jo["data"] is JArray ? jo["data"] : jo["data"]["items"];
                    foreach (var item in jt)
                    {
                        string name = item["playListName"].ToString();
                        string imgurl = item["image"].ToString();
                        if (imgurl.StartsWith("//")) imgurl = "https:" + imgurl;
                        string plUrl = item["playListId"].ToString();
                        list.Add(new Playlist(name, imgurl, plUrl));
                    }
                }
                catch (Exception)
                {
                }
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

            request.Headers.Add("Cookie", $"os=ios;MUSIC_U={NetEasyMusicU}");
            //request.Headers.Add("Cookie", $"MUSIC_U=f3f3e2c4f16db349c6bce0e815e03298d4cfd233212b6b417f539b0c6faf7bc6993166e004087dd3d78b6050a17a35e705925a4e6992f61dfe3f0151024f9e31;");
            var a = Encoding.UTF8.GetBytes(paramData);
            request.ContentLength = a.Length;

            try
            {
                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(a, 0, a.Length);
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                CheckCloudMusicCookie(response.Headers.GetValues("Set-Cookie"));
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

        private static void CheckCloudMusicCookie(string[] cookies)
        {
            if (cookies == null) return;
            foreach (string cookie in cookies)
            {
                if (cookie.StartsWith("__csrf="))
                {
                    var value = Regex.Match(cookie, "__csrf=([0-9a-zA-Z]+)").Groups[1].Value;
                    if(value != NetEasyCsrfToken)
                    {
                        NetEasyCsrfToken = value;
                        File.WriteAllText(EnvironmentSingle.NetEasyCsrfTokenPath, NetEasyCsrfToken);
                    }
                }
                else if (cookie.StartsWith("MUSIC_U="))
                {
                    var value = Regex.Match(cookie, "MUSIC_U=([0-9a-zA-Z]+)").Groups[1].Value;
                    if (value != NetEasyMusicU)
                    {
                        NetEasyMusicU = value;
                        File.WriteAllText(EnvironmentSingle.NetEasyMusicUPath, NetEasyMusicU);
                    }
                }
            }
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

        public static List<NetMusic> GetMyRecommendSongsFromCloudMusic()
        {
            var param = AesEncrypt($"{{\"offset\":\"0\",\"total\":\"true\",\"csrf_token\":\"{NetEasyCsrfToken}\"}}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = "https://music.163.com/weapi/v2/discovery/recommend/songs?csrf_token=";
            var paramData = "params=" + param + encSecKey;
            var result = CloudSendDataByPost(url, paramData, false);
            return GetCloudMusicMyRecommendList(result);
        }

        public static List<NetMusic> GetMyRecommendSongsFromQQMusic()
        {
            var url = "https://c.y.qq.com/node/musicmac/v6/index.html";
            var headers = new Dictionary<string, string>()
            {
                {"Cookie", QQMusicU},
            };
            var retStr = SendDataByGET(url, headers);
            try
            {
                var recommendId = Regex.Match(retStr, "data-rid=\"([\\d]+)\"[\\s]*[\\S]+[\\s]*今日私享").Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(recommendId))
                {
                    return GetPlayListItems(recommendId, NetMusicType.QQMusic);
                }
            }
            catch (Exception)
            {
            }
            return new List<NetMusic>();
        }

        public static List<NetMusic> GetMyRecommendSongsFromMiguMusic()
        {
            var url = "https://app.c.nf.migu.cn/MIGUM3.0/v1.0/template/todayRecommendList/release?actionId=1&actionSong=&index=&templateVersion=6";
            var headers = new Dictionary<string, string>()
            {
                {"by", "22210ca73bf1af2ec2eace74a96ee356"},
                {"Referer", "https://m.music.migu.cn/v4/my/collect"},
                {"Cookie", MiguMusicU},
                {"IMEI", "000000"},
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"},
            };
            var retStr = SendDataByGET(url, headers);
            var list = new List<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                var jt = jo["data"]["recommendData"]["data"];
                foreach (var item in jt)
                {
                    NetMusic music = new NetMusic
                    {
                        Title = item["songName"].ToString(),
                        Singer = item["singerList"][0]["name"].ToString(),
                        MusicID = item["copyrightId"].ToString(),
                        Album = item["album"].ToString(),
                        AlbumImageUrl = item["img3"].ToString(),
                        Origin = NetMusicType.MiguMusic,
                        Duration = new TimeSpan(0, 0, 0, int.Parse(item["duration"].ToString()), 0),
                        Remark = item["contentId"].ToString(),
                    };
                    list.Add(music);
                }
            }
            catch (Exception)
            {
            }
            return list;
        }

        private static List<NetMusic> GetCloudMusicMyRecommendList(string JsonStr)
        {
            var list = new List<NetMusic>();
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(JsonStr);
                var jt = jo["data"]["dailySongs"];
                foreach (var item in jt)
                {
                    NetMusic music = new NetMusic
                    {
                        Title = item["name"].ToString(),
                        Singer = item["artists"][0]["name"].ToString(),
                        MusicID = item["id"].ToString(),
                        Album = item["album"]["name"].ToString(),
                        AlbumImageUrl = item["album"]["picUrl"].ToString(),//"picUrl": "http://p1.music.126.net/B1ePGczwQUZueJl70TITWQ==/3287539775420245.jpg"
                        Origin = NetMusicType.CloudMusic,
                        Url = "",// item["mp3Url"].ToString().Replace("m2.music.126.net", "p2.music.126.net");
                        Duration = new TimeSpan(0, 0, 0, 0, int.Parse(item["duration"].ToString()))
                    };
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
            }
            return list;
        }

        public static string GetUserPlaylistFromCloudMusic()
        {
            var param = AesEncrypt($"{{\"uid\":\"75679234\",\"wordwrap\":\"7\",\"offset\":\"0\",\"total\":\"true\",\"limit\":\"36\",\"csrf_token\":\"{NetEasyCsrfToken}\"}}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = "https://music.163.com/weapi/user/playlist?csrf_token=";
            var paramData = "params=" + param + encSecKey;
            var result = CloudSendDataByPost(url, paramData, false);
            return result;
        }

        public static string GetUnikeyFromCloudMusic()
        {
            var param = AesEncrypt("{\"type\":\"1\",\"csrf_token\":\"\"}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = "https://music.163.com/weapi/login/qrcode/unikey?csrf_token=";
            var paramData = "params=" + param + encSecKey;
            try
            {
                var result = CloudSendDataByPost(url, paramData, false);
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                jo.TryGetValue("unikey", out JToken jValue);
                return jValue != null ? jValue.ToString() : "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static int GetLoginStateFromCloudMusic(string unikey)
        {
            var param = AesEncrypt("{\"key\":\"" + unikey + "\",\"type\":\"1\",\"csrf_token\":\"\"}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = "https://music.163.com/weapi/login/qrcode/client/login?csrf_token=";
            var paramData = "params=" + param + encSecKey;
            var resultStr = CloudSendDataByPost(url, paramData, false);

            JObject jo = (JObject)JsonConvert.DeserializeObject(resultStr);
            jo.TryGetValue("code", out JToken jValue);
            int code = 0;
            if (jValue != null)
            {
                int.TryParse(jValue.ToString(), out code);
            }
            return code;
        }

        public static User GetAccountFromCloudMusic()
        {
            var param = AesEncrypt($"{{\"csrf_token\":\"{NetEasyCsrfToken}\"}}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = "https://music.163.com/weapi/w/nuser/account/get?csrf_token=";
            var paramData = "params=" + param + encSecKey;
            var resultStr = CloudSendDataByPost(url, paramData, false);
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(resultStr);
                var profile = jo.GetValue("profile");
                return new User(profile["userId"].ToString(), profile["nickname"].ToString(), profile["avatarUrl"].ToString());
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static readonly string MiguPublicKey = "MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgEsxO1eMxBy4Oa+Nls8KNhrqXBCAyjRrzTZjBCYSQygansdaZNNBs/2Uj9fnHs/ff1XyYhCuTtzqAs+vJyvMBvCfTwrPN0F6lbmY/+lwEsuAhlqrBdCnRjshosP89Qm0hSDBti1LtBkcE4yLE/f4BWEDCrCdYzDgOrpmNrz3zabZAgMBAAE=";
        private static readonly string MiguPrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIICWwIBAAKBgEsxO1eMxBy4Oa+Nls8KNhrqXBCAyjRrzTZjBCYSQygansdaZNNB
s/2Uj9fnHs/ff1XyYhCuTtzqAs+vJyvMBvCfTwrPN0F6lbmY/+lwEsuAhlqrBdCn
RjshosP89Qm0hSDBti1LtBkcE4yLE/f4BWEDCrCdYzDgOrpmNrz3zabZAgMBAAEC
gYAQRExUOm3K0MgaBIWVsN3XoM/d+h7EjHXOyEkDe3vv1yJ2ekXJtjMcLuGXkbaG
vhEsJM22Uh9Zh36oM3pD7VWqyKRh9XkN1kbYdB7/eXXB6xOFa7Obxk1gm5MMf5CT
uEUrgzJwlV+IyPO3EXJ34fnH7hIRMHG4PIscrHD+sqpi6QJBAIrHiH374O2+CR6f
A6tm3YXc+ACdlLO2Z8Dx/Zkm74Dp4F6K7zTWazk7z/AbiR02jVZ0JLqsQCiDTy7g
ylB/5n8CQQCKtCtKEDxE9325CxDgx+RJN0w3xKetDMH9kYlZWWZ91lXLm0RN06Md
qR2LvUsPNoHGMRWSkKYkGelBj745gLanAkEAhRDzUBFOR8caSXEg/J0iNPN+HGD8
LyDr9PZTOiE6LnqR9zTyTdB2eSdfpxNP8mHXPZkZiqAU2IOnTgSeGHe6kwJATsVh
bE9qGvS+9q7dJ/r9n8MCyw0o+LMtHHdhnFeUSFTIJriIAvb1ROv9NpYLIZmf+9F2
YeU6JXh9qtkafAeoMwJAK6L1JLDwkm6xhuPxuO97Y7NsMjKwWFW7FWn/it48nXia
eZ74Qmm+6d+nLEccUdpoxyRkEe/ZIc+FswOLu/P71Q==
-----END RSA PRIVATE KEY-----";
        private static readonly RSACryptoServiceProvider MiguRSA = CodecHelper.PemDecodeX509PrivateKey(CodecHelper.PemUnpack(MiguPrivateKey));
        public static User GetAccountFromMigu(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie)) return null;
            var url = "https://m.music.migu.cn/migumusic/h5/user/auth/getUserInfo?publicKey="+WebUtility.UrlEncode(MiguPublicKey);
            var headers = new Dictionary<string, string>()
            {
                {"By", "22210ca73bf1af2ec2eace74a96ee356"},
                {"Referer", "https://m.music.migu.cn/v4/my"},
                {"Cookie", cookie},
                {"User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36"},
            };
            var resultStr = SendDataByGET(url, headers);
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(resultStr);
                string encryptedData = jo.GetValue("aesKey").ToString();
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData);
                byte[] decryptedBytes = MiguRSA.Decrypt(encryptedBytes, false);
                string aesKey = Encoding.UTF8.GetString(decryptedBytes);
                var jsonStr = CodecHelper.AESDecode(jo.GetValue("data").ToString(), aesKey);
                jo = (JObject)JsonConvert.DeserializeObject(jsonStr);
                var profile = jo.GetValue("data");
                var avatar = profile["avatar"]["smallAvatar"].ToString();
                if (avatar.StartsWith("//")) avatar = "https:" + avatar;
                var user = new User(profile["userId"].ToString(), profile["nickName"].ToString(), avatar);
                MiguMusicU = cookie;
                File.WriteAllText(EnvironmentSingle.MiguMusicUPath, cookie);
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static User GetAccountFromQQMusic(string cookie)
        {
            if(string.IsNullOrWhiteSpace(cookie)) return null;
            var url = "https://c.y.qq.com/rsc/fcgi-bin/fcg_get_profile_homepage.fcg?cid=205360838&reqfrom=1";
            var headers = new Dictionary<string, string>()
            {
                {"cookie", cookie},
            };
            var resultStr = SendDataByGET(url, headers);
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(resultStr);
                var creator = jo.GetValue("data")["creator"];
                var user = new User(creator["encrypt_uin"].ToString(), creator["nick"].ToString(), creator["headpic"].ToString());
                QQMusicU = cookie;
                File.WriteAllText(EnvironmentSingle.QQMusicUPath, cookie);
                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string GetUrlFromQQMusic(Music music)
        {
            var MusicUrl = "";
            try
            {
                var t = DateTime.Now.Millisecond;
                var guid = ((int)Math.Abs((int)Math.Round(2147483647 * new Random().NextDouble()) * t % 1e10)).ToString();
                string callback = "getplaysongvkey" + (new Random().NextDouble() + "").Replace("0.", "");
                string data = $"{{\"req\":{{\"module\":\"CDN.SrfCdnDispatchServer\",\"method\":\"GetCdnDispatch\",\"param\":{{\"guid\":\"{guid}\",\"calltype\":0,\"userip\":\"\"}}}},\"req_0\":{{\"module\":\"vkey.GetVkeyServer\",\"method\":\"CgiGetVkey\",\"param\":{{\"guid\":\"{guid}\",\"songmid\":[\"{music.MusicID}\"],\"songtype\":[0],\"uin\":\"0\",\"loginflag\":1,\"platform\":\"20\"}}}},\"comm\":{{\"uin\":0,\"format\":\"json\",\"ct\":20,\"cv\":0}}}}";
                data = System.Web.HttpUtility.UrlEncode(data);
                var remoteUrl = $"https://u.y.qq.com/cgi-bin/musicu.fcg?callback={callback}&g_tk=5381&jsonpCallback={callback}&loginUin=0&hostUin=0&format=jsonp&inCharset=utf8&outCharset=utf-8&notice=0&platform=yqq&needNewCode=0&data={data}";
                
                var retStr = SendDataByGET(remoteUrl).Substring(callback.Length+1);
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr.Substring(0,retStr.Length-1));
                var purl = jo["req_0"]["data"]["midurlinfo"][0]["purl"].ToString();
                if (!string.IsNullOrWhiteSpace(purl))
                {
                    MusicUrl = "http://dl.stream.qqmusic.qq.com/" + purl;
                }
            }
            catch (Exception)
            {
            }
            //if (string.IsNullOrWhiteSpace(MusicUrl))
            //{
            //    var remoteUrl = $"https://u6.y.qq.com/cgi-bin/musics.fcg?sign=zzcc6801c0rxecgkqtqjki1grnwdv0xfxud841cd7059f";
            //    var headers = new Dictionary<string, string>()
            //    {
            //        {"content-type", "application/x-www-form-urlencoded"},
            //        {"Referer", "https://i.y.qq.com/"},
            //        {"User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/13.0.3 Mobile/15E148 Safari/604.1"}
            //    };
            //    var data = Encoding.UTF8.GetBytes($"{{\"req_0\":{{\"module\":\"music.trackInfo.UniformRuleCtrl\",\"method\":\"GetTrackInfo\",\"param\":{{\"ids\":[{music.Remark}],\"types\":[0]}},\"comm\":{{\"g_tk\":5381,\"uin\":0,\"format\":\"json\",\"platform\":\"h5\"}}");
            //    var retStr = SendDataByPOST(remoteUrl, data, headers);
            //    Console.WriteLine(retStr);
            //}
            return MusicUrl;
        }
        private static string GetUrlFromMiguMusic(Music music)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>()
            {
                { "channel", "014000D" }
            };
            var retStr = SendDataByGET($"https://c.musicapp.migu.cn/MIGUM3.0/strategy/listen-url/v2.4?contentId={music.Remark}&copyrightId={music.MusicID}&resourceType=2&netType=01&toneFlag=PQ&scene=&lowerQualityContentId={music.MusicID}", headers);
            try
            {
                JObject jo = (JObject)JsonConvert.DeserializeObject(retStr);
                JToken songPlayInfos = jo["data"];
                music.Path = songPlayInfos["url"].ToString();
                music.Duration = new TimeSpan(0, 0, 0, int.Parse(songPlayInfos["song"]["duration"].ToString()), 0);
                music.LyricPath = songPlayInfos["lrcUrl"].ToString();
                music.Album = songPlayInfos["song"]["album"].ToString();
                music.AlbumImageUrl = songPlayInfos["song"]["img1"].ToString();
                if (music.AlbumImageUrl.StartsWith("/"))
                {
                    music.AlbumImageUrl = "http://d.musicapp.migu.cn" + music.AlbumImageUrl;
                }
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
            try
            {
                using (HttpWebResponse httpRes = (HttpWebResponse)httpReq.GetResponse())
                {
                    url = Regex.Replace(httpRes.Headers["Location"], "^http", "https");
                }
            }
            catch (Exception)
            {
                url = "";
            }

            if (url.EndsWith("404")) return "";

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
        LocalMusic,CloudMusic, QQMusic, MiguMusic
    }
    public enum RankingListType
    {
        SoarList,HotList,NewSongList
    }
}
