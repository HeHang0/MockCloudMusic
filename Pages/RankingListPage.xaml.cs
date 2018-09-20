using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicCollection.Pages
{
    /// <summary>
    /// RankingListPage.xaml 的交互逻辑
    /// </summary>
    public partial class RankingListPage : Page
    {
        private MainWindow ParentWindow;
        private NetMusicType PageType = NetMusicType.CloudMusic;
        private RankingListType PageListType = RankingListType.HotList;
        private ObservableCollection<NetMusic> NetMusicList = new ObservableCollection<NetMusic>();
        public RankingListPage(MainWindow mainWindow)
        {
            ParentWindow = mainWindow;
            InitializeComponent();
            NetMusicDataGrid.DataContext = NetMusicList;
            StartGetMusicListThread();
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
                if (!ParentWindow.Play(null, netMusic))
                {
                    MessageBox.Show($"当前音乐不可在线播放！{Environment.NewLine}更换资源或重试！");
                }
            }
        }
        private void SingerCell_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                Label lb = sender as Label;
                string searchStr = lb.Content as string;
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

            if (!(ParentWindow?.IsConnectInternet() ?? false))
            {
                return;
            }
            Thread thread = new Thread(new ThreadStart(() => GetNetMusicList(PageListType, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        public void GetNetMusicList(RankingListType listType, NetMusicType netMusicType)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Visible;
            }));
            var t = NetMusicHelper.GetNetMusicList(listType, netMusicType);
            Dispatcher.Invoke(new Action(() =>
            {
                NetMusicList.Clear();
            }));
            foreach (var item in t)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    NetMusicList.Add(item);
                }));
            }
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Hidden;
            }));
        }
        private void HotListButton_Click(object sender, RoutedEventArgs e)
        {
            HotListButton.Visibility = Visibility.Hidden;
            NewSongListButton.Visibility = Visibility.Visible;
            SoarListButton.Visibility = Visibility.Visible;
            HotListButtonHelper.Visibility = Visibility.Visible;
            NewSongListButtonHelper.Visibility = Visibility.Hidden;
            SoarListButtonHelper.Visibility = Visibility.Hidden;
            PageListType = RankingListType.HotList;
            StartGetMusicListThread();
        }

        private void NewSongListButton_Click(object sender, RoutedEventArgs e)
        {
            HotListButton.Visibility = Visibility.Visible;
            NewSongListButton.Visibility = Visibility.Hidden;
            SoarListButton.Visibility = Visibility.Visible;
            HotListButtonHelper.Visibility = Visibility.Hidden;
            NewSongListButtonHelper.Visibility = Visibility.Visible;
            SoarListButtonHelper.Visibility = Visibility.Hidden;
            PageListType = RankingListType.NewSongList;
            StartGetMusicListThread();
        }

        private void SoarListButton_Click(object sender, RoutedEventArgs e)
        {
            HotListButton.Visibility = Visibility.Visible;
            NewSongListButton.Visibility = Visibility.Visible;
            SoarListButton.Visibility = Visibility.Hidden;
            HotListButtonHelper.Visibility = Visibility.Hidden;
            NewSongListButtonHelper.Visibility = Visibility.Hidden;
            SoarListButtonHelper.Visibility = Visibility.Visible;
            PageListType = RankingListType.SoarList;
            StartGetMusicListThread();
        }

        private void CloudMusicButton_Click(object sender, RoutedEventArgs e)
        {
            CloudMusicButton.Visibility = Visibility.Hidden;
            QQMusicButton.Visibility = Visibility.Visible;
            XiaMiMusicButton.Visibility = Visibility.Visible;
            CloudMusicButtonHelper.Visibility = Visibility.Visible;
            QQMusicButtonHelper.Visibility = Visibility.Hidden;
            XiaMiMusicButtonHelper.Visibility = Visibility.Hidden;
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
            PageType = NetMusicType.XiaMiMusic;
            StartGetMusicListThread();
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddToList(true))
            {
                ParentWindow.Play();
            }
        }

        private void AddToListButton_Click(object sender, RoutedEventArgs e)
        {
            AddToList(false);
        }

        private bool AddToList(bool NeedClear)
        {
            if (NetMusicList.Count > 0)
            {
                if (NeedClear) ParentWindow.CurrentMusicList.Clear();
                foreach (var item in NetMusicList)
                {
                    ParentWindow.CurrentMusicList.Add(new Music(item));
                }
                if (NeedClear) ParentWindow.CurrentIndex = 0;
                return true;
            }
            return false;
        }

        private void AddToMyPlayListButton_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            if (NetMusicList.Count > 0 && ParentWindow.GetStringFromInputStringWindow("添加歌单", "歌单名称", out name))
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    ObservableCollection<Music> list = new ObservableCollection<Music>();
                    foreach (var item in NetMusicList)
                    {
                        list.Add(new Music(item));
                    }
                    ParentWindow.PlayListCollection.Add(new PlayListCollectionModel(name, "", list));
                }
                else
                {
                    MessageBox.Show("名称不能为空");
                }
            }
        }
    }
}
