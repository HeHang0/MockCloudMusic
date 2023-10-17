using MusicCollection.ChildWindows;
using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MusicCollection.Pages
{
    /// <summary>
    /// DiscoverMusic.xaml 的交互逻辑
    /// </summary>
    public partial class DiscoverMusicPage : Page
    {
        private MainWindow ParentWindow;
        private NetMusicType PageType = NetMusicType.CloudMusic;
        private int Offset = 0;
        private int SearchPageCount = 0;
        private bool isMyFavorite = true;

        public DiscoverMusicPage(MainWindow mainWindow)
        {
            ParentWindow = mainWindow;
            InitializeComponent();
        }

        public void SetMyFavorite(bool isFav)
        {
            if(isFav != isMyFavorite) StartGetPlayListThread();
            isMyFavorite = isFav;
            LastPageButton.Visibility = isFav ? Visibility.Collapsed : Visibility.Visible;
            NextPageButton.Visibility = isFav ? Visibility.Collapsed : Visibility.Visible;
        }

        private void StartGetPlayListThread()
        {
            if (string.IsNullOrWhiteSpace(NetMusicHelper.NetEasyCsrfToken))
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Owner = ParentWindow;
                loginWindow.ShowDialog();
            }
            Thread thread = new Thread(new ThreadStart(() => GetPlayList(Offset, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (PlayListDisplay.ItemsSource == null)
            {
                StartGetPlayListThread();
            }
        }

        private void CloudMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (CloseDataGridButton.Visibility == Visibility.Visible)
            {
                CloseDataGridButton_Click(new object(), e);
            }
            CloudMusicButton.Visibility = Visibility.Hidden;
            QQMusicButton.Visibility = Visibility.Visible;
            XiaMiMusicButton.Visibility = Visibility.Visible;
            CloudMusicButtonHelper.Visibility = Visibility.Visible;
            QQMusicButtonHelper.Visibility = Visibility.Hidden;
            MiguMusicButtonHelper.Visibility = Visibility.Hidden;
            PageType = NetMusicType.CloudMusic;
            Offset = 0;SearchPageCount = 0;
            StartGetPlayListThread();
        }

        private void QQMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (CloseDataGridButton.Visibility == Visibility.Visible)
            {
                CloseDataGridButton_Click(new object(), e);
            }
            CloudMusicButton.Visibility = Visibility.Visible;
            QQMusicButton.Visibility = Visibility.Hidden;
            XiaMiMusicButton.Visibility = Visibility.Visible;
            CloudMusicButtonHelper.Visibility = Visibility.Hidden;
            QQMusicButtonHelper.Visibility = Visibility.Visible;
            MiguMusicButtonHelper.Visibility = Visibility.Hidden;
            PageType = NetMusicType.QQMusic;
            Offset = 0; SearchPageCount = 0;
            StartGetPlayListThread();
        }

        private void MiguMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (CloseDataGridButton.Visibility == Visibility.Visible)
            {
                CloseDataGridButton_Click(new object(), e);
            }
            CloudMusicButton.Visibility = Visibility.Visible;
            QQMusicButton.Visibility = Visibility.Visible;
            XiaMiMusicButton.Visibility = Visibility.Hidden;
            CloudMusicButtonHelper.Visibility = Visibility.Hidden;
            QQMusicButtonHelper.Visibility = Visibility.Hidden;
            MiguMusicButtonHelper.Visibility = Visibility.Visible;
            PageType = NetMusicType.MiguMusic;
            Offset = 0; SearchPageCount = 0;
            StartGetPlayListThread();
        }

        private void LastPageButton_Click(object sender, RoutedEventArgs e)
        {
            //Offset -= 35;
            Thread thread = new Thread(new ThreadStart(() => GetPlayList(--Offset, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            //Offset += 35;
            Thread thread = new Thread(new ThreadStart(() => GetPlayList(++Offset, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void GetPlayList(int offset, NetMusicType pageType)
        {
            if (!(ParentWindow?.IsConnectInternet() ?? false))
            {
                return;
            }
            int count = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Visible;
            }));
            List<Playlist> list;
            if (isMyFavorite)
            {
                list = NetMusicHelper.GetMyFavoritePlayList(PageType);
                if(list.Count > 0)
                {
                    list.Insert(0, new Playlist(Playlist.MyDailyRecommand, Playlist.MyDailyRecommand, Playlist.MyDailyRecommand));
                }
            }
            else
            {
                list = NetMusicHelper.GetPlayList(Offset, PageType, out count);
            }
            Dispatcher.Invoke(new Action(() =>
            {
                if (count <= 1)
                {
                    LastPageButton.IsEnabled = false;
                    NextPageButton.IsEnabled = false;
                }
                else if (offset == 0)
                {
                    LastPageButton.IsEnabled = false;
                    NextPageButton.IsEnabled = true;
                }
                else if (offset == count)
                {
                    LastPageButton.IsEnabled = true;
                    NextPageButton.IsEnabled = false;
                }
                else
                {
                    LastPageButton.IsEnabled = true;
                    NextPageButton.IsEnabled = true;
                }
                SearchPageCount = count;
                PlayListDisplay.ItemsSource = list;
                LodingImage.Visibility = Visibility.Hidden;
            }));
        }

        private void PlayListButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            var row = btn.Tag as Playlist;
            AddToMyPlayListButton.Tag = btn.DataContext;
            Thread thread = new Thread(new ThreadStart(() => GetPlayListItems(row.Url, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void GetPlayListItems(string url, NetMusicType pageType)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Visible;
            }));
            List<NetMusic> list = null;
            if(pageType == NetMusicType.CloudMusic && url == Playlist.MyDailyRecommand)
            {
                list = NetMusicHelper.GetMyRecommendSongsFromCloudMusic();
            }
            else if(pageType == NetMusicType.MiguMusic && url == Playlist.MyDailyRecommand)
            {
                list = NetMusicHelper.GetMyRecommendSongsFromMiguMusic();
            }
            else if(pageType == NetMusicType.QQMusic && url == Playlist.MyDailyRecommand)
            {
                list = NetMusicHelper.GetMyRecommendSongsFromQQMusic();
            }
            else
            {
                list = NetMusicHelper.GetPlayListItems(url, PageType);
            }
            Dispatcher.Invoke(new Action(() =>
            {
                ButtonGroup.Visibility = Visibility.Hidden;
                LodingImage.Visibility = Visibility.Hidden;
                NetMusicDataGrid.DataContext = list;
                NetMusicDataGrid.Visibility = Visibility.Visible;
                PlayAllLocalButton.Visibility = Visibility.Visible;
                AllAddToCurrentListButton.Visibility = Visibility.Visible;
                AddToMyPlayListButton.Visibility = Visibility.Visible;
                AllDownloadButton.Visibility = Visibility.Visible;
                PlayListDisplay.Visibility = Visibility.Hidden;
                LastPageButton.Visibility = Visibility.Hidden;
                NextPageButton.Visibility = Visibility.Hidden;
                CloseDataGridButton.Visibility = Visibility.Visible;
            }));
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

        private void PlayAllLocalButton_Click(object sender, RoutedEventArgs e)
        {
            var list = NetMusicDataGrid.DataContext as List<NetMusic>;
            if (list != null && list.Count > 0)
            {
                ParentWindow.CurrentMusicList.Clear();
                foreach (var item in list)
                {
                    ParentWindow.CurrentMusicList.Add(new Music(item));
                }
                ParentWindow.CurrentIndex = 0;
                ParentWindow.Play();
            }
        }

        private void AllAddToCurrentListButton_Click(object sender, RoutedEventArgs e)
        {
            var list = NetMusicDataGrid.DataContext as List<NetMusic>;
            if (list != null)
            {
                foreach (var item in list)
                {
                    ParentWindow.CurrentMusicList.Add(new Music(item));
                }
            }
        }

        private void AllDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var nlist = NetMusicDataGrid.DataContext as List<NetMusic>;
            foreach (var item in nlist)
            {
                ParentWindow.DownLoadMusic.DownLoadingList.Add(item);
            }
        }

        private void AddToMyPlayListButton_Click(object sender, RoutedEventArgs e)
        {
            string name = string.Empty;
            string imgurl = string.Empty;
            if(AddToMyPlayListButton.Tag is DataRowView)
            {
                DataRowView row = AddToMyPlayListButton.Tag as DataRowView;
                if (ParentWindow.PlayListCollection.Where(m => m.Name == row.Row.ItemArray[0] as string).Count() > 0)
                {
                    return;
                }
                name = row.Row.ItemArray[0] as string;
                imgurl = row.Row.ItemArray[1] as string;
            }
            else if(AddToMyPlayListButton.Tag is Playlist)
            {
                Playlist row = AddToMyPlayListButton.Tag as Playlist;
                if (ParentWindow.PlayListCollection.Where(m => m.Name == row.Name).Count() > 0)
                {
                    return;
                }
                name = row.Name;
                imgurl = row.ImgUrl;
            }
            else
            {
                return;
            }
            var nlist = NetMusicDataGrid.DataContext as List<NetMusic>;
            var mlist = new ObservableCollection<Music>();
            foreach (var item in nlist)
            {
                mlist.Add(new Music(item));
            }
            ParentWindow.PlayListCollection.Add(new PlayListCollectionModel(name, imgurl, mlist));
        }

        private void CloseDataGridButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDataGridButton.Visibility = Visibility.Hidden;
            ButtonGroup.Visibility = Visibility.Visible;
            NetMusicDataGrid.Visibility = Visibility.Hidden;
            PlayAllLocalButton.Visibility = Visibility.Hidden;
            AllAddToCurrentListButton.Visibility = Visibility.Hidden;
            AddToMyPlayListButton.Visibility = Visibility.Hidden;
            AllDownloadButton.Visibility = Visibility.Hidden;
            PlayListDisplay.Visibility = Visibility.Visible;
            LastPageButton.Visibility = Visibility.Visible;
            NextPageButton.Visibility = Visibility.Visible;
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}
