using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MusicCollection.Pages
{
    /// <summary>
    /// NetMusicSearchPage.xaml 的交互逻辑
    /// </summary>
    public partial class NetMusicSearchPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private MainWindow ParentWindow;
        public int Offset { get; set; } = 0;
        public int MusicCount { get; set; } = 0;
        private string searchStr = string.Empty;
        public string SearchStr
        {
            get {
                return searchStr;
            }
            set
            {
                searchStr = value;
                OnPropertyChanged("SearchStr");
            }
        }
        public NetMusicType PageType { get; set; } = NetMusicType.CloudMusic;
        public NetMusicSearchPage(MainWindow parentWindow)
        {
            InitializeComponent();
            ParentWindow = parentWindow;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NetMusicDataGrid.DataContext = ParentWindow.NetMusicList;
        }

        private void NetMusicDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            if ((int)e.Row.Header < 10)
            {
                e.Row.Header = "0" + e.Row.Header;
            }
        }
        
        private void NetMusicDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var netMusic = dgr.Item as NetMusic;
                //if (netMusic != null)
                //{
                //    ParentWindow.DownLoadMusic.DownLoadingList.Add(netMusic);
                //}
                if (!ParentWindow.Play(null, netMusic))
                {
                    MessageBox.Show("当前音乐不可在线播放！");
                }
            }
        }
        private void SingerCell_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                Label lb = sender as Label;
                string searchStr = lb.Content as string;
                MusicCount = 0;
                Offset = 0;
                SearchStr = searchStr;
                Thread thread = new Thread(new ThreadStart(() => GetNetMusicList(searchStr, Offset, PageType)));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void NetMusicDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var netMusic = btn.Tag as NetMusic;
            ParentWindow.DownLoadMusic.DownLoadingList.Add(netMusic);
            btn.Visibility = Visibility.Hidden;
        }

        private void NetMusicPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var netMusic = btn.Tag as NetMusic;
            //LodingImage.Visibility = Visibility.Visible;

            //CheckAndDownLoad(netMusic, true);
            if (!ParentWindow.Play(null, netMusic))
            {
                MessageBox.Show($"当前音乐不可在线播放！{Environment.NewLine}更换资源或重试！");
            }
        }

        private void NetMusicAddToListButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var netMusic = btn.Tag as NetMusic;
            ParentWindow.CurrentMusicList.Add(new Music(netMusic));
        }

        private void StartGetMusicListThread()
        {
            Thread thread = new Thread(new ThreadStart(() => GetNetMusicList(SearchStr, Offset, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            --Offset;
            StartGetMusicListThread();
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            ++Offset;
            StartGetMusicListThread();
        }

        public void GetNetMusicList(string searchStr, int offset, NetMusicType netMusicType)
        {
            int count = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Visible;
                if (offset == 0)
                {
                    CountLabel.DataContext = $"搜索\"{searchStr}\"";
                }
            }));
            var t = NetMusicHelper.GetNetMusicList(searchStr, offset, netMusicType, out count);
            Dispatcher.Invoke(new Action(() =>
            {
                ParentWindow.NetMusicList.Clear();
            }));
            foreach (var item in t)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ParentWindow.NetMusicList.Add(item);
                }));
            }
            Dispatcher.Invoke(new Action(() =>
            {
                if (offset == 0)
                {
                    CountLabel.DataContext = $"搜索\"{searchStr}\", 找到 {count} 首单曲";
                }
                if (offset == 0 && count <= 30)
                {
                    LastPageButton.IsEnabled = false;
                    NextPageButton.IsEnabled = false;
                }
                else if (offset == 0)
                {
                    LastPageButton.IsEnabled = false;
                    NextPageButton.IsEnabled = true;
                }
                else if (offset/30+1 == count/30 + (count%30 == 0 ? 0 : 1))
                {
                    LastPageButton.IsEnabled = true;
                    NextPageButton.IsEnabled = false;
                }
                else
                {
                    LastPageButton.IsEnabled = true;
                    NextPageButton.IsEnabled = true;
                }
                MusicCount = count;
                LodingImage.Visibility = Visibility.Hidden;
            }));
        }

        private void CloudMusicButton_Click(object sender, RoutedEventArgs e)
        {
            CloudMusicButton.Visibility = Visibility.Hidden;
            QQMusicButton.Visibility = Visibility.Visible;
            XiaMiMusicButton.Visibility = Visibility.Visible;
            CloudMusicButtonHelper.Visibility = Visibility.Visible;
            QQMusicButtonHelper.Visibility = Visibility.Hidden;
            XiaMiMusicButtonHelper.Visibility = Visibility.Hidden;
            Offset = 0; MusicCount = 0;
            PageType = NetMusicType.CloudMusic;
            StartGetMusicListThread();
        }

        private void QQMusicButton_Click(object sender, RoutedEventArgs e)
        {
            CloudMusicButton.Visibility = Visibility.Visible;
            QQMusicButton.Visibility = Visibility.Hidden;
            XiaMiMusicButton.Visibility = Visibility.Visible;
            CloudMusicButtonHelper.Visibility = Visibility.Hidden;
            QQMusicButtonHelper.Visibility = Visibility.Visible;
            XiaMiMusicButtonHelper.Visibility = Visibility.Hidden;
            Offset = 0; MusicCount = 0;
            PageType = NetMusicType.QQMusic;
            StartGetMusicListThread();
        }

        private void XiaMiMusicButton_Click(object sender, RoutedEventArgs e)
        {
            CloudMusicButton.Visibility = Visibility.Visible;
            QQMusicButton.Visibility = Visibility.Visible;
            XiaMiMusicButton.Visibility = Visibility.Hidden;
            CloudMusicButtonHelper.Visibility = Visibility.Hidden;
            QQMusicButtonHelper.Visibility = Visibility.Hidden;
            XiaMiMusicButtonHelper.Visibility = Visibility.Visible;
            Offset = 0; MusicCount = 0;
            PageType = NetMusicType.XiaMiMusic;
            StartGetMusicListThread();
        }
    }
}
