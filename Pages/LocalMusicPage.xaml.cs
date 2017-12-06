using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MusicCollection.MusicManager;
using System.Text.RegularExpressions;
using System.Threading;
using System;
using System.Windows;
using System.Linq;

namespace MusicCollection.Pages
{
    /// <summary>
    /// LocalMusicPage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalMusicPage : Page
    {
        public ObservableCollection<string> FolderList = new ObservableCollection<string>();
        public MusicObservableCollection<Music> LocalMusicList = new MusicObservableCollection<Music>();
        public MainWindow ParentWindow { get; private set; }
        public LocalMusicPage(MainWindow ParentWindow)
        {
            this.ParentWindow = ParentWindow;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            var LocalMusicFolderListPath = "LocalMusicFolderList.json";
            var LocalMusicListPath = "LocalMusicList.json";

            if (!File.Exists(LocalMusicFolderListPath))
            {
                File.WriteAllText(LocalMusicFolderListPath, JsonConvert.SerializeObject(new ObservableCollection<string>()));
            }
            if (!File.Exists(LocalMusicListPath))
            {
                File.WriteAllText(LocalMusicListPath, JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }

            var LocalMusicFolderListContent = "";
            var LocalMusicListContent = "";
            try
            {
                LocalMusicFolderListContent = File.ReadAllText(LocalMusicFolderListPath);
                LocalMusicListContent = File.ReadAllText(LocalMusicListPath);
            }
            catch (System.Exception)
            {                
            }

            FolderList = JsonConvert.DeserializeObject<ObservableCollection<string>>(LocalMusicFolderListContent);
            if (FolderList.Count == 0)
            {
                FolderList.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\");
            }
            LocalMusicList = JsonConvert.DeserializeObject<MusicObservableCollection<Music>>(LocalMusicListContent);
            if (LocalMusicList.Count == 0)
            {
                RefreshLocalList();
            }            

            LocalMusicDataGrid.DataContext = LocalMusicList;
            LocalMusicCountLable.Content = LocalMusicList.Count + "首音乐";
            LocalMusicList.CollectionChanged += LocalMusicList_OnCountChange; ;
        }

        private void LocalMusicList_OnCountChange(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LocalMusicCountLable.Content = LocalMusicList.Count + "首音乐";
        }
        
        private void RefreshLocalListButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            RefreshLocalList();
        }

        private void RefreshLocalList()
        {
            Thread thread = new Thread(new ThreadStart(RefreshLocalListThread));
            thread.IsBackground = true;
            thread.Start();
        }
        
        private void RefreshLocalListThread()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                RefreshMusicCavas.Visibility = Visibility.Visible;
            }));
            var count = 0;
            List<string> pathList = new List<string>();
            foreach (var item in FolderList)
            {
                DirectoryInfo TheFolder = new DirectoryInfo(item);
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    var pattern = ".+?(\\.mp3|\\.wav|\\.flac|\\.wma|\\.ape|\\.m4a)$";
                    if (Regex.IsMatch(NextFile.Name, pattern, RegexOptions.IgnoreCase))
                    {
                        pathList.Add(item + "\\" + NextFile.Name);
                        count++;
                    }
                }
            }
            List<Music> WillBeRemove = new List<Music>();
            foreach (var item in LocalMusicList)
            {
                if (pathList.Contains(item.Url))
                {
                    pathList.Remove(item.Url);
                }
                else
                {
                    WillBeRemove.Add(item);
                }
            }

            for (int i = 0; i < WillBeRemove.Count; i++)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    LocalMusicList.Remove(WillBeRemove[i]);
                }));
            }

            foreach (var item in pathList)
            {
                if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                {
                    var music = new Music(item);
                    Dispatcher.Invoke(new Action(() => {
                        LocalMusicList.Add(music);
                    }));
                }
                else
                {
                    object[] args = new object[] { LocalMusicList, item };
                    Thread staThread = new Thread(new ParameterizedThreadStart(RefreshLocalListSTA));
                    staThread.SetApartmentState(ApartmentState.STA);
                    staThread.Start(args);
                    staThread.Join();
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    RefreshMusicCountLable.Content = LocalMusicList.Count + "/" + count;
                }));
            }
            Dispatcher.Invoke(new Action(() => {
                RefreshMusicCavas.Visibility = Visibility.Hidden;
            }));
        }
        private void RefreshLocalListSTA(object o)
        {
            object[] args = (object[])o;
            var list = (MusicObservableCollection<Music>)args[0];
            var item = (string)args[1];
            var music = new Music(item);
            Dispatcher.Invoke(new Action(() =>
            {
                list.Add(music);
            }));
        }

        private void LocalMusicDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            if ((int)e.Row.Header < 10)
            {
                e.Row.Header = "0" + e.Row.Header;
            }
        }

        private void LocalMusicDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var music = dgr.Item as Music;
                if (music != null)
                {
                    ParentWindow.CurrentIndex = ParentWindow.CurrentMusicList.Add(music);
                    ParentWindow.bsp.Stop();
                    ParentWindow.Play(music);
                }
            }
        }

        private void LocalMusicDataGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {
            ParentWindow.bsp.Stop();
            ParentWindow.Play();
        }

        private void PlayAllLocalButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ParentWindow.CurrentMusicList.Clear();
            ParentWindow.CurrentIndex = -1;
            AllLocalMusicAddToList();
            ParentWindow.Play();
        }

        private void LocalAddToListButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var item = LocalMusicDataGrid.SelectedItem as Music;
            if (item != null)
            {
                ParentWindow.CurrentMusicList.Add(item);
            }
        }
        private void AllLocalMusicAddToList()
        {
            if (LocalMusicList.Count == 0)
            {
                return;
            }
            foreach (var item in LocalMusicList)
            {
                ParentWindow.CurrentMusicList.Add(item);
            }
            ParentWindow.CurrentIndex = ParentWindow.CurrentMusicList.Count - 1;
        }

        public void ParentWindow_Closing()
        {
            File.WriteAllText("LocalMusicFolderList.json", JsonConvert.SerializeObject(FolderList));
            File.WriteAllText("LocalMusicList.json", JsonConvert.SerializeObject(LocalMusicList));
        }
        private void SelectContent_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var childWin = new ChildWondows.LocalMusicFolderWindow(ParentWindow, this);
            childWin.Owner = ParentWindow;
            childWin.ShowDialog();
        }

        private void SearchTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var search = SearchTextBox.Text.ToLower();
            if (search.Length > 0)
            {
                LocalMusicDataGrid.DataContext = LocalMusicList.Where(m => m.Singer.ToLower().Contains(search) || m.Title.ToLower().Contains(search) || m.Album.ToLower().Contains(search));
            }
            else
            {
                LocalMusicDataGrid.DataContext = LocalMusicList;
            }
        }
    }
}
