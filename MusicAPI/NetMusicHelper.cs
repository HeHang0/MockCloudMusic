using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;

namespace MusicCollection.MusicAPI
{
    class NetMusicHelper
    {
        private static Dictionary<NetMusicType, string> NetAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/api/search/pc?s={0}&offset=0&limit=10&type=1" }
        };
        public static ObservableCollection<NetMusic> GetNetMusicList(string SearchStr, NetMusicType type)
        {
            var retString = SendDataByGET(string.Format(NetAPI[type], SearchStr));
            return GetNetMusicListBySources(retString, type);
        }
        private static string SendDataByGET(string Url)	//读取string
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            request.Referer = "http://music.163.com/";
            request.Method = "POST";
            request.ContentType = "text/html;charset=UTF-8";



            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        private static ObservableCollection<NetMusic> GetNetMusicListBySources(string Sources, NetMusicType type)
        {
            switch (type)
            {
                case NetMusicType.CloudMusic:
                    return GetCloudMusicList(Sources);
            }
            return new ObservableCollection<NetMusic>();
        }

        private static ObservableCollection<NetMusic> GetCloudMusicList(string JsonStr)
        {
            var list = new ObservableCollection<NetMusic>();
            JObject jo = (JObject)JsonConvert.DeserializeObject(JsonStr);
            var jt = jo["result"]["songs"];
            foreach (var item in jt)
            {
                var music = new NetMusic();
                music.Title = item["name"].ToString();
                music.Singer = item["artists"][0]["name"].ToString();
                music.MusicID = item["id"].ToString();
                music.Album = item["album"]["name"].ToString();
                music.Origin = NetMusicType.CloudMusic;
                list.Add(music);
            }
            return list;
        }
    }
    public enum NetMusicType
    {
        CloudMusic, QQMusic, XiaMiMusic, BaiduMusic, KuwoMusic, KuGouMusic
    }
}
