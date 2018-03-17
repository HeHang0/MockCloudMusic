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

        public DiscoverMusicPage(MainWindow mainWindow)
        {
            ParentWindow = mainWindow;
            InitializeComponent();
            LastPageButton.Content = "<";
        }

        private void StartGetPlayListThread()
        {
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
            XiaMiMusicButtonHelper.Visibility = Visibility.Hidden;
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
            XiaMiMusicButtonHelper.Visibility = Visibility.Hidden;
            PageType = NetMusicType.QQMusic;
            Offset = 0; SearchPageCount = 0;
            StartGetPlayListThread();
        }

        private void XiaMiMusicButton_Click(object sender, RoutedEventArgs e)
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
            XiaMiMusicButtonHelper.Visibility = Visibility.Visible;
            PageType = NetMusicType.XiaMiMusic;
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
            int count = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Visible;
            }));
            var list = NetMusicHelper.GetPlayList(Offset, PageType, out count);
            DataTable pic = new DataTable();
            pic.Columns.Add("Name");
            pic.Columns.Add("ImgUrl");
            pic.Columns.Add("Url");
            foreach (var item in list)
            {
                pic.Rows.Add(item.Name, item.ImgUrl, item.Url);
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
                PlayListDisplay.ItemsSource = pic.DefaultView;
                LodingImage.Visibility = Visibility.Hidden;
            }));
        }

        private void PlayListButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DataRowView row = btn.Tag as DataRowView;
            AddToMyPlayListButton.Tag = btn.DataContext;
            Thread thread = new Thread(new ThreadStart(() => GetPlayListItems(row.Row.ItemArray[2] as string, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void GetPlayListItems(string url, NetMusicType pageType)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Visible;
            }));
            var list = NetMusicHelper.GetPlayListItems(url, PageType);
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Hidden;
                NetMusicDataGrid.DataContext = list;
                NetMusicDataGrid.Visibility = Visibility.Visible;
                PlayAllLocalButton.Visibility = Visibility.Visible;
                AllAddToCurrentListButton.Visibility = Visibility.Visible;
                AddToMyPlayListButton.Visibility = Visibility.Visible;
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

        private void AddToMyPlayListButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = AddToMyPlayListButton.Tag as DataRowView;
            if (ParentWindow.PlayListCollection.Where(m => m.Name == row.Row.ItemArray[0] as string).Count() > 0)
            {
                return;
            }
            var nlist = NetMusicDataGrid.DataContext as List<NetMusic>;
            var mlist = new ObservableCollection<Music>();
            foreach (var item in nlist)
            {
                mlist.Add(new Music(item));
            }
            ParentWindow.PlayListCollection.Add(new PlayListCollectionModel(row.Row.ItemArray[0] as string, row.Row.ItemArray[1] as string, mlist));
        }

        private void CloseDataGridButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDataGridButton.Visibility = Visibility.Hidden;
            NetMusicDataGrid.Visibility = Visibility.Hidden;
            PlayAllLocalButton.Visibility = Visibility.Hidden;
            AllAddToCurrentListButton.Visibility = Visibility.Hidden;
            AddToMyPlayListButton.Visibility = Visibility.Hidden;
            PlayListDisplay.Visibility = Visibility.Visible;
            LastPageButton.Visibility = Visibility.Visible;
            NextPageButton.Visibility = Visibility.Visible;
        }
    }
}
