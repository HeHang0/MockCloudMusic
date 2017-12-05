using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            
        }

        private void NetMusicDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var netMusic = btn.Tag as NetMusic;
            DownLoadMusic(netMusic);
        }

        private void NetMusicPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var netMusic = btn.Tag as NetMusic;
            var music = DownLoadMusic(netMusic);
            ParentWindow.CurrentIndex = ParentWindow.CurrentMusicList.Add(music);
            ParentWindow.bsp.Stop();
            ParentWindow.Play(music);
        }

        private Music DownLoadMusic(NetMusic netMusic)
        {
            LodingImage.Visibility = Visibility.Visible;
            Task<Music> t = Task.Factory.StartNew(() => NetMusicHelper.GetMusicByMusicID(netMusic));
            t.Wait();
            var music = t.Result;
            if (!ParentWindow.LocalMusic.FolderList.Contains(Path.GetDirectoryName(music.Url) + "\\"))
            {
                ParentWindow.LocalMusic.FolderList.Add(Path.GetDirectoryName(music.Url) + "\\");
            }
            ParentWindow.LocalMusic.LocalMusicList.Add(music);
            LodingImage.Visibility = Visibility.Hidden;
            return music;
        }
    }
}
