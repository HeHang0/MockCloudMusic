using MusicCollection.MusicManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace MusicCollection.MusicAPI
{
    class NetMusicHelper
    {
        private static Dictionary<NetMusicType, string> SearchAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://music.163.com/api/search/pc?s={0}&offset=0&limit=30&type=1" }
        };

        private static Dictionary<NetMusicType, string> DownloadLinkAPI = new Dictionary<NetMusicType, string>()
        {
            { NetMusicType.CloudMusic, "http://link.hhtjim.com/163/{0}.mp3 " }
        };

        public static Music GetMusicByMusicID(NetMusic net_music)
        {
            var path = GetMusicUrlOfLocal(DownloadLinkAPI[net_music.Origin], net_music.MusicID, net_music.Title + " - " + net_music.Singer);
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
            return new Music(list[0], net_music);
        }

        private static void NewMusicSTA(object o)
        {
            object[] args = (object[])o;
            var list = (List<Music>)args[0];
            var path = (string)args[1];
            list.Add(new Music(path));
        }


        public static ObservableCollection<NetMusic> GetNetMusicList(string SearchStr, NetMusicType type)
        {
            var retString = SendDataByGET(string.Format(SearchAPI[type], SearchStr));
            return GetNetMusicListBySources(retString, type);
        }
        
        private static string GetMusicUrlOfLocal(string Url, string MusicID, string MusicName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(string.Format(Url, MusicID));

            //request.Referer = "http://music.163.com/";
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream stream = response.GetResponseStream();
            if (!Directory.Exists("DownLoad\\Music\\"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("DownLoad\\Music\\");
            }
            var path = "DownLoad\\Music\\" + MusicName + ".mp3";
            StreamWriter sw = new StreamWriter(path);
            stream.CopyTo(sw.BaseStream);

            sw.Flush();
            sw.Close();

            stream.Close();
            return Path.GetFullPath(path);
        }


        private static string SendDataByGET(string Url)	//读取string
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            //request.Referer = "http://music.163.com/";
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

        /// <summary>
        /// 执行JS方法
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="para">参数</param>
        /// <returns></returns>
        private static string GetJsMethd(string methodName, object[] para)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("package aa{");
            sb.Append(" public class JScript {");
            sb.Append("     public static function test(str) {");
            sb.Append("         return 'Hello,'+str;");
            sb.Append("     }");
            sb.Append(" }");
            sb.Append("}");

            CompilerParameters parameters = new CompilerParameters();

            parameters.GenerateInMemory = true;

            CodeDomProvider _provider = new Microsoft.JScript.JScriptCodeProvider();

            CompilerResults results = _provider.CompileAssemblyFromSource(parameters, sb.ToString());

            Assembly assembly = results.CompiledAssembly;

            Type _evaluateType = assembly.GetType("aa.JScript");

            object obj = _evaluateType.InvokeMember("test", BindingFlags.InvokeMethod,
            null, null, para);

            return obj.ToString();
        }
    }
    public enum NetMusicType
    {
        CloudMusic, QQMusic, XiaMiMusic, BaiduMusic, KuwoMusic, KuGouMusic
    }
}
