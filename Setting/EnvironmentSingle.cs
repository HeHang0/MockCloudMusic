using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace MusicCollection.Setting
{
    public static class EnvironmentSingle
    {
        public static string AppDataPath;
        public static string DownloadPath;
        public static string DownloadMusicPath;
        public static string DownloadLyricPath;
        public static string AlbumImagePath;
        public static string ConfigPath;
        public static string NetEasyCsrfTokenPath;
        public static string NetEasyMusicUPath;
        public static string QQMusicUPath;
        public static string MiguMusicUPath;
        public static long MaxAlbumImageCacheSize = 50 * 1024 * 1024;


        public static string ConfigCurrentMusicList;
        public static string ConfigDownLoadingList;
        public static string ConfigLocalMusicFolderList;
        public static string ConfigPlayListCollection;
        public static string ConfigDownLoadedList;
        public static string ConfigHistoryMusicList;
        public static string ConfigLocalMusicList;

        public static void CheckNecessaryFile()
        {
            string roming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name.ToString();
            AppDataPath = Path.Combine(roming,appName);
            DownloadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), appName);
            DownloadMusicPath = Path.Combine(DownloadPath, "Music");
            DownloadLyricPath = Path.Combine(DownloadPath, "Lyric");
            AlbumImagePath = Path.Combine(AppDataPath, "AlbumImage");
            ConfigPath = Path.Combine(AppDataPath, "Data");
            NetEasyCsrfTokenPath = Path.Combine(ConfigPath, "ne-csrf-token");
            NetEasyMusicUPath = Path.Combine(ConfigPath, "ne-music-u");
            QQMusicUPath = Path.Combine(ConfigPath, "qq-cookies");
            MiguMusicUPath = Path.Combine(ConfigPath, "migu-cookies");

            ConfigCurrentMusicList = Path.Combine(ConfigPath, "CurrentMusicList.json");
            ConfigDownLoadingList = Path.Combine(ConfigPath, "DownLoadingList.json");
            ConfigLocalMusicFolderList = Path.Combine(ConfigPath, "LocalMusicFolderList.json");
            ConfigPlayListCollection = Path.Combine(ConfigPath, "PlayListCollection.json");
            ConfigDownLoadedList = Path.Combine(ConfigPath, "DownLoadedList.json");
            ConfigHistoryMusicList = Path.Combine(ConfigPath, "HistoryMusicList.json");
            ConfigLocalMusicList = Path.Combine(ConfigPath, "LocalMusicList.json");

            if (!Directory.Exists(AppDataPath)) Directory.CreateDirectory(AppDataPath);
            if (!Directory.Exists(DownloadPath)) Directory.CreateDirectory(DownloadPath);
            if (!Directory.Exists(DownloadMusicPath)) Directory.CreateDirectory(DownloadMusicPath);
            if (!Directory.Exists(DownloadLyricPath)) Directory.CreateDirectory(DownloadLyricPath);
            if (!Directory.Exists(ConfigPath)) Directory.CreateDirectory(ConfigPath);
            if (!Directory.Exists(AlbumImagePath)) Directory.CreateDirectory(AlbumImagePath);


            if (!File.Exists(ConfigCurrentMusicList))
            {
                File.WriteAllText(ConfigCurrentMusicList, JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists(ConfigHistoryMusicList))
            {
                File.WriteAllText(ConfigHistoryMusicList, JsonConvert.SerializeObject(new MusicHistoriesCollection<MusicHistory>()));
            }
            if (!File.Exists(ConfigPlayListCollection))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("PlayList");
                File.WriteAllText(ConfigPlayListCollection, JsonConvert.SerializeObject(dt));
            }
            if (!File.Exists(ConfigLocalMusicFolderList))
            {
                File.WriteAllText(ConfigLocalMusicFolderList, JsonConvert.SerializeObject(new List<string>()));
            }
            if (!File.Exists(ConfigLocalMusicList))
            {
                File.WriteAllText(ConfigLocalMusicList, JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists(ConfigDownLoadedList))
            {
                File.WriteAllText(ConfigDownLoadedList, JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists(ConfigDownLoadingList))
            {
                File.WriteAllText(ConfigDownLoadingList, JsonConvert.SerializeObject(new NetMusicObservableCollection<NetMusic>()));
            }
            if (!File.Exists(NetEasyCsrfTokenPath))
            {
                File.WriteAllText(NetEasyCsrfTokenPath, "");
            }
            if (!File.Exists(NetEasyMusicUPath))
            {
                File.WriteAllText(NetEasyMusicUPath, "");
            }
            if (!File.Exists(QQMusicUPath))
            {
                File.WriteAllText(QQMusicUPath, "");
            }
            if (!File.Exists(MiguMusicUPath))
            {
                File.WriteAllText(MiguMusicUPath, "");
            }

            ClearAlbumImagePath();
        }

        private static void ClearAlbumImagePath()
        {
            DirectoryInfo di = new DirectoryInfo(AlbumImagePath);

            //通过GetFiles方法,获取di目录中的所有文件的大小
            long len = 0;
            Dictionary<string, long> fileDict = new Dictionary<string, long>();
            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name.EndsWith(".download"))
                {
                    fi.Delete();
                    continue;
                }
                len += fi.Length;
                fileDict.Add(fi.FullName, fi.LastAccessTime.Ticks);
            }
            if (len < MaxAlbumImageCacheSize) return;
            var fileList = fileDict.OrderByDescending(o => o.Value).ToList();
            for (int i = 0; i < fileList.Count(); i++)
            {
                FileInfo fi = new FileInfo(fileList[i].Key);
                if (!fi.Exists) continue;
                File.Delete(fileList[i].Key);
                len -= fi.Length;
                if (len < MaxAlbumImageCacheSize) return;
            }
        }

        public static long GetDirectoryLength(string dirPath)
        {
            //判断给定的路径是否存在,如果不存在则退出
            if (!Directory.Exists(dirPath))
                return 0;
            long len = 0;

            //定义一个DirectoryInfo对象
            DirectoryInfo di = new DirectoryInfo(dirPath);

            //通过GetFiles方法,获取di目录中的所有文件的大小
            foreach (FileInfo fi in di.GetFiles())
            {
                len += fi.Length;
            }

            //获取di中所有的文件夹,并存到一个新的对象数组中,以进行递归
            DirectoryInfo[] dis = di.GetDirectories();
            if (dis.Length > 0)
            {
                for (int i = 0; i < dis.Length; i++)
                {
                    len += GetDirectoryLength(dis[i].FullName);
                }
            }
            return len;
        }
    }
}
