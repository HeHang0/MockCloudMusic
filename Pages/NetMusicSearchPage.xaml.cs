using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.IO;
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
                    CheckAndDownLoad(netMusic, true);
                }
            }
        }

        private void NetMusicDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var netMusic = btn.Tag as NetMusic;
            CheckAndDownLoad(netMusic, false);
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

        private void CheckAndDownLoad(NetMusic netMusic, bool NeedPlay)
        {
            foreach (var item in ParentWindow.LocalMusic.FolderList)
            {
                var path = item + netMusic.Title + " - " + netMusic.Singer + ".mp3";
                if (File.Exists(path))
                {
                    if (NeedPlay)
                    {
                        var indexL = ParentWindow.LocalMusic.LocalMusicList.Add(new Music(path));

                        ParentWindow.Play(ParentWindow.LocalMusic.LocalMusicList[indexL]);
                    }
                    return;
                }
            }
            LodingImage.Visibility = Visibility.Visible;
            netMusic.IsDownLoading = true;
            Thread thread = new Thread(new ThreadStart(() => DownLoadMusic(netMusic, NeedPlay)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void DownLoadMusic(NetMusic netMusic, bool NeedPlay)
        {
            var music = NetMusicHelper.GetMusicByMusicID(netMusic);
            if (music == null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    LodingImage.Visibility = Visibility.Hidden;
                }));
                return;
            }
            Dispatcher.Invoke(new Action(() =>
            {
                if (!ParentWindow.LocalMusic.FolderList.Contains(Path.GetDirectoryName(music.Url) + "\\"))
                {
                    ParentWindow.LocalMusic.FolderList.Add(Path.GetDirectoryName(music.Url) + "\\");
                }
                ParentWindow.LocalMusic.LocalMusicList.Add(music);
                LodingImage.Visibility = Visibility.Hidden;
                netMusic.IsDownLoading = false;
                netMusic.IsDownLoaded = true;
                if (NeedPlay)
                {
                    ParentWindow.Play(music);
                }
            }));            
        }
    }
}
