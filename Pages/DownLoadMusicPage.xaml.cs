using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace MusicCollection.Pages
{
    /// <summary>
    /// DownLoadMusicPage.xaml 的交互逻辑
    /// </summary>
    public partial class DownLoadMusicPage : Page
    {
        private MusicObservableCollection<Music> DownLoadedList;
        public NetMusicObservableCollection<NetMusic> DownLoadingList;
        private static string DownLoadDirectory = "DownLoad\\Music\\";
        private DispatcherTimer timer = new DispatcherTimer();
        private MainWindow ParentWindow;
        public DownLoadMusicPage(MainWindow parentWindow)
        {
            ParentWindow = parentWindow;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            if (!File.Exists("Data\\DownLoadedList.json"))
            {
                File.WriteAllText("Data\\DownLoadedList.json", Newtonsoft.Json.JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            var DownLoadedListStr = File.ReadAllText("Data\\DownLoadedList.json");
            DownLoadedList = Newtonsoft.Json.JsonConvert.DeserializeObject<MusicObservableCollection<Music>>(DownLoadedListStr);

            if (!File.Exists("Data\\DownLoadingList.json"))
            {
                File.WriteAllText("Data\\DownLoadingList.json", Newtonsoft.Json.JsonConvert.SerializeObject(new NetMusicObservableCollection<NetMusic>()));
            }
            var DownLoadingListStr = File.ReadAllText("Data\\DownLoadingList.json");
            DownLoadingList = Newtonsoft.Json.JsonConvert.DeserializeObject<NetMusicObservableCollection<NetMusic>>(DownLoadingListStr);
            DownLoadedMusicDataGrid.DataContext = DownLoadedList;
            DownLoadingMusicDataGrid.DataContext = DownLoadingList;
            DownLoadDirectoryLable.Content = DownLoadDirectory;
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += TimerOnTick;
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            if (DownLoadingList.Count > 0)
            {
                PreviewDownLoad(DownLoadingList[0]);
            }
        }

        private void PreviewDownLoad(NetMusic netMusic)
        {
            if (!netMusic.IsDownLoading && !netMusic.IsDownLoaded)
            {
                netMusic.IsDownLoading = true;;
                Thread thread = new Thread(new ThreadStart(() => DownLoadMusic(netMusic)));
                thread.IsBackground = true;
                thread.Start();
            }
        }
        private void DownLoadMusic(NetMusic netMusic)
        {
            var music = NetMusicHelper.GetMusicByNetMusic(netMusic);
            if (music == null)
            {
                return;
            }
            Dispatcher.Invoke(new Action(() =>
            {
                if (!ParentWindow.LocalMusic.FolderList.Contains(Path.GetDirectoryName(music.Path) + "\\"))
                {
                    ParentWindow.LocalMusic.FolderList.Add(Path.GetDirectoryName(music.Path) + "\\");
                }
                ParentWindow.LocalMusic.LocalMusicList.Add(music);
                DownLoadedList.Add(music);
                netMusic.IsDownLoading = false;
                netMusic.IsDownLoaded = true;
                DownLoadingList.Remove(netMusic);
            }));
        }

        private void DownLoadedMusicDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            if ((int)e.Row.Header < 10)
            {
                e.Row.Header = "0" + e.Row.Header;
            }
        }

        private void DownLoadedMusicDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
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

    private void PlayAllDownLoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (DownLoadedList.Count <= 0)
            {
                return;
            }
            ParentWindow.CurrentMusicList.Clear();
            foreach (var item in DownLoadedList)
            {
                ParentWindow.CurrentMusicList.Add(item);
            }
            ParentWindow.bsp.Stop();
            ParentWindow.CurrentIndex = 0;
            ParentWindow.Play();
        }

        private void DownLoadAddToListButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in DownLoadedList)
            {
                ParentWindow.CurrentMusicList.Add(item);
            }
        }
        private void DownLoadDirectoryLable_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (System.IO.Directory.Exists(DownLoadDirectory))
            {
                System.Diagnostics.Process.Start(DownLoadDirectory);
            }
        }

        private void SearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            var st = SearchTextBox.Text;
            if (string.IsNullOrWhiteSpace(st))
            {
                DownLoadedMusicDataGrid.DataContext = DownLoadedList.Where(m => m.Title.Contains(st) || m.Album.Contains(st) || m.Singer.Contains(st));
            }
            else
            {
                DownLoadedMusicDataGrid.DataContext = DownLoadedList;
            }
        }

        private void DownLoadedListButton_Click(object sender, RoutedEventArgs e)
        {
            GridSplitter0.Visibility = Visibility.Visible;
            DownLoadedListButton.Visibility = Visibility.Hidden;
            DownLoadingListButton.Visibility = Visibility.Visible;
            DownLoadedListButtonHelper.Visibility = Visibility.Visible;
            DownLoadingListButtonHelper.Visibility = Visibility.Hidden;
            DownLoadedMusicDataGrid.Visibility = Visibility.Visible;
            DownLoadingMusicDataGrid.Visibility = Visibility.Hidden;
            DownLoadedCanvas.Visibility = Visibility;
        }

        private void DownLoadingListButton_Click(object sender, RoutedEventArgs e)
        {
            GridSplitter0.Visibility = Visibility.Hidden;
            DownLoadedListButton.Visibility = Visibility.Visible;
            DownLoadingListButton.Visibility = Visibility.Hidden;
            DownLoadedListButtonHelper.Visibility = Visibility.Hidden;
            DownLoadingListButtonHelper.Visibility = Visibility.Visible;
            DownLoadedMusicDataGrid.Visibility = Visibility.Hidden;
            DownLoadingMusicDataGrid.Visibility = Visibility.Visible;
            DownLoadedCanvas.Visibility = Visibility.Hidden;
        }

        private void DownLoadingMusicDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            if ((int)e.Row.Header < 10)
            {
                e.Row.Header = "0" + e.Row.Header;
            }
        }
        public void DownLoadMusicPage_Closing()
        {
            File.WriteAllText("Data\\DownLoadedList.json", Newtonsoft.Json.JsonConvert.SerializeObject(DownLoadedList));
            File.WriteAllText("Data\\DownLoadingList.json", Newtonsoft.Json.JsonConvert.SerializeObject(DownLoadingList));
        }
    }
}
