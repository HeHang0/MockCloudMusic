using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
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
    public partial class NetMusicSearchPage : Page
    {
        private MainWindow ParentWindow;
        public NetMusicSearchPage(MainWindow parentWindow)
        {
            InitializeComponent();
            ParentWindow = parentWindow;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            NetMusicDataGrid.DataContext = ParentWindow.NetMusicList;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            
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
                if (netMusic != null)
                {
                    ParentWindow.DownLoadMusic.DownLoadingList.Add(netMusic);
                }
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
                MessageBox.Show("当前音乐不可在线播放！");
            }
        }
    }
}
