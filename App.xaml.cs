using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using MusicCollection.MusicManager;
using System.Collections.ObjectModel;
using MusicCollection.MusicAPI;

namespace MusicCollection
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {/*
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            var executingAssemblyName = executingAssembly.GetName();
            var resName = executingAssemblyName.Name + ".resources";

            AssemblyName assemblyName = new AssemblyName(args.Name); string path = "";
            if (resName == assemblyName.Name)
            {
                path = executingAssemblyName.Name + ".g.resources"; ;
            }
            else
            {
                path = assemblyName.Name + ".dll";
                if (assemblyName.CultureInfo.Equals(CultureInfo.InvariantCulture) == false)
                {
                    path = String.Format(@"{0}\{1}", assemblyName.CultureInfo, path);
                }
            }

            using (Stream stream = executingAssembly.GetManifestResourceStream(path))
            {
                if (stream == null)
                    return null;

                byte[] assemblyRawBytes = new byte[stream.Length];
                stream.Read(assemblyRawBytes, 0, assemblyRawBytes.Length);
                return Assembly.Load(assemblyRawBytes);
            }
        }*/

        protected override void OnStartup(StartupEventArgs e)
        {
            CheckNecessaryFile();
            base.OnStartup(e);
            //AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;
        }

        private void CheckNecessaryFile()
        {
            if (!Directory.Exists("AlbumImage\\"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("AlbumImage\\");
            }
            if (!Directory.Exists("DownLoad\\"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("DownLoad\\");
            }
            if (!Directory.Exists("DownLoad\\Music"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("DownLoad\\Music");
            }
            if (!Directory.Exists("DownLoad\\Lyric"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("DownLoad\\Lyric");
            }
            if (!Directory.Exists("Data\\"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("Data\\");
            }
            if (!File.Exists("Data\\CurrentMusicList.json"))
            {
                File.WriteAllText("Data\\CurrentMusicList.json", JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists("Data\\HistoryMusicList.json"))
            {
                File.WriteAllText("Data\\HistoryMusicList.json", JsonConvert.SerializeObject(new MusicHistoriesCollection<MusicHistory>()));
            }
            if (!File.Exists("Data\\PlayListCollection.json"))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns.Add("PlayList");
                File.WriteAllText("Data\\PlayListCollection.json", JsonConvert.SerializeObject(dt));
            }
            if (!File.Exists("Data\\LocalMusicFolderList.json"))
            {
                File.WriteAllText("Data\\LocalMusicFolderList.json", JsonConvert.SerializeObject(new ObservableCollection<string>()));
            }
            if (!File.Exists("Data\\LocalMusicList.json"))
            {
                File.WriteAllText("Data\\LocalMusicList.json", JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists("Data\\DownLoadedList.json"))
            {
                File.WriteAllText("Data\\DownLoadedList.json", JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists("Data\\DownLoadingList.json"))
            {
                File.WriteAllText("Data\\DownLoadingList.json", JsonConvert.SerializeObject(new NetMusicObservableCollection<NetMusic>()));
            }
        }
    }
}
