using HtmlAgilityPack;
using MusicCollection.MusicManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private static Dictionary<NetMusicType, string> SearchAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/api/search/pc?s={0}&offset={1}&limit=30&type=1" }
        };

        private static Dictionary<NetMusicType, string> LyricAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/api/song/lyric?os=pc&id={0}&lv=-1&kv=-1&tv=-1" }
        };

        private static Dictionary<NetMusicType, string> DownloadLinkAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://link.hhtjim.com/163/{0}.mp3" },
            { NetMusicType.XiaMiMusic, "https://music-api-jwzcyzizya.now.sh/api/search/song/xiami?&limit=1&page=1&key={0}-{1}-{2}/" }
        };

        private static Dictionary<NetMusicType, string> PlayListHotAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/discover/playlist/?order=hot&limit=35&offset={0}" }
        };
        private static Dictionary<NetMusicType, string> PlayListDetailAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/weapi/v3/playlist/detail" }
        };


        public static string GetUrlByNetMusic(Music music = null, NetMusic netMusic = null)
        {
            if (netMusic != null)
            {
                music = new Music(netMusic);
            }
            switch (music.Origin)
            {
                case NetMusicType.CloudMusic:
                    var CloudMusicUrl = GetUrlFromCloudMusic(music);
                    if (!string.IsNullOrWhiteSpace(CloudMusicUrl) && CheckLink(CloudMusicUrl))
                    {
                        return CloudMusicUrl;
                    }
                    else if (CheckLink(music.Path))
                    {
                        return music.Path;
                    }
                    break;
            }
            return "";
        }

        public static bool CheckLink(string url)
        {
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
                if (string.IsNullOrWhiteSpace(music.LyricPath) && CheckLink(music.LyricPath))
                {
                    LyricStr = SendDataByGET(music.LyricPath);
                }
                else
                {
                    LyricStr = SendDataByGET(string.Format(LyricAPI[music.Origin], music.MusicID));
                }
                if (!string.IsNullOrWhiteSpace(LyricStr))
                {
                    JObject jo = (JObject)JsonConvert.DeserializeObject(LyricStr);
                    LyricStr = jo["lrc"]["lyric"].ToString();
                    if (!Directory.Exists("DownLoad\\Lyric\\"))//如果不存在就创建文件夹
                    {
                        Directory.CreateDirectory("DownLoad\\Lyric\\");
                    }
                    var LiricLine = LyricStr.Split('\n');
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

        public static ObservableCollection<NetMusic> GetNetMusicList(string SearchStr, int offset, NetMusicType type, out int count)
        {
            var retString = SendDataByPOST(string.Format(SearchAPI[type], SearchStr, offset));
            if (retString.Length < 50)
            {
                count = 0;
                return new ObservableCollection<NetMusic>();
            }
            return GetNetMusicListBySources(retString, type, out count);
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

        private static string SendDataByGET(string Url)	//读取string
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            //request.Referer = "http://music.163.com/";
            request.Method = "GET";
            //request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";

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
                    music.Url = item["mp3Url"].ToString().Replace("m2.music.126.net", "p2.music.126.net");
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
                return list;
            }
            catch (Exception)
            {
                count = 0;
                return list;
            }
        }

        public static List<Playlist> GetPlayList(int offset, NetMusicType type, out int count)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    var retStr = SendDataByGET(string.Format(PlayListHotAPI[type], offset));
                    return GetCloudMusicPlayList(retStr, out count);
            }
            count = 0;
            return new List<Playlist>();
        }
        private static List<Playlist> GetCloudMusicPlayList(string retStr, out int count)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(retStr);
            var collection = htmlDocument.DocumentNode.SelectNodes("//ul[@id='m-pl-container']/li");
            int.TryParse(htmlDocument.DocumentNode.SelectNodes("//a[@class='zpgi']")?.LastOrDefault()?.InnerText, out count);
            List<Playlist> list = new List<Playlist>();
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
            return list;
        }

        public static List<NetMusic> GetPlayListItems(string url, NetMusicType type)
        {
            string name = string.Empty, imgurl = string.Empty;
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return GetCloudMusicPlayListItems(url, out name,out imgurl);
            }
            return new List<NetMusic>();
        }

        public static List<NetMusic> GetPlayListItems(string url, NetMusicType type, out string name, out string imgurl)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return GetCloudMusicPlayListItems(url, out name, out imgurl);
            }
            name = "";imgurl = "";
            return new List<NetMusic>();
        }

        private static List<NetMusic> GetCloudMusicPlayListItems(string playlisyUrl, out string name,out string imgurl)
        {
            string id = playlisyUrl.Replace("http://", "https://").Replace("/#","").Replace("https://music.163.com/playlist?id=", "");
            var param = AesEncrypt("{\"id\":\"" + id + "\",\"offset\":0,\"total\":true,\"limit\":1000,\"n\":1000,\"csrf_token\":\"\"}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = PlayListDetailAPI[NetMusicType.CloudMusic];
            var paramData = "params=" + param + encSecKey;
            var retStr = CloudSendDataByPost(url, paramData, false);
            name = ""; imgurl = "";
            List<NetMusic> list = new List<NetMusic>();
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
            Url = Url.Replace("http://", "https://");
            //byte[] byteArray = dataEncode.GetBytes(paramData); //转化
            request.Referer = "https://music.163.com/";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.Timeout = 5000;
            //request.ReadWriteTimeout = 5000;
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";

            var a = Encoding.ASCII.GetBytes(paramData);
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
            var param = AesEncrypt("{\"ids\":\"[" + music.MusicID + "]\",\"br\":320000,\"csrf_token\":\"\"}", "0CoJUm6Qyw8W8jud");
            param = AesEncrypt(param, "a8LWv2uAtXjzSfkQ");
            param = System.Web.HttpUtility.UrlEncode(param);
            var encSecKey = "&encSecKey=2d48fd9fb8e58bc9c1f14a7bda1b8e49a3520a67a2300a1f73766caee29f2411c5350bceb15ed196ca963d6a6d0b61f3734f0a0f4a172ad853f16dd06018bc5ca8fb640eaa8decd1cd41f66e166cea7a3023bd63960e656ec97751cfc7ce08d943928e9db9b35400ff3d138bda1ab511a06fbee75585191cabe0e6e63f7350d6";
            var url = "http://music.163.com/weapi/song/enhance/player/url?csrf_token=";
            var paramData = "params=" + param + encSecKey;
            var Url = CloudSendDataByPost(url, paramData);
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
    }
    public enum NetMusicType
    {
        LocalMusic,CloudMusic, QQMusic, XiaMiMusic, BaiduMusic, KuwoMusic, KuGouMusic
    }
}
