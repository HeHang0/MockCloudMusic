using MusicCollection.MusicAPI;
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
using System.Windows.Shapes;

namespace MusicCollection.ChildWindows
{
    /// <summary>
    /// ChangeResourcesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ChangeResourcesWindow : Window
    {
        private MainWindow ParentWindow;
        private NetMusicType PageType = NetMusicType.CloudMusic;
        private string SearchStr = string.Empty;
        public ObservableCollection<NetMusic> MusicList = new ObservableCollection<NetMusic>();
        public ChangeResourcesWindow(MainWindow mainWindow, string searchStr)
        {
            ParentWindow = mainWindow;
            SearchStr = searchStr;
            InitializeComponent();
            StartGetMusicListThread();
        }

        private void StartGetMusicListThread()
        {
            Thread thread = new Thread(new ThreadStart(() => GetNetMusicList(SearchStr, 0, PageType)));
            thread.IsBackground = true;
            thread.Start();
        }

        public void GetNetMusicList(string searchStr, int offset, NetMusicType netMusicType)
        {
            int count = 0;
            Dispatcher.Invoke(new Action(() =>
            {
                LodingImage.Visibility = Visibility.Visible;
                CountLabel.DataContext = $"搜索\"{searchStr}\"";
            }));
            var t = NetMusicHelper.GetNetMusicList(searchStr, offset, netMusicType, out count);
            Dispatcher.Invoke(new Action(() =>
            {
                MusicList.Clear();
                MusicList = t;
                NetMusicDataGrid.DataContext = MusicList;
            }));
            Dispatcher.Invoke(new Action(() =>
            {
                CountLabel.DataContext = $"搜索\"{searchStr}\", 找到 {count} 首单曲";
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
                    MessageBox.Show("当前音乐不可在线播放！");
                }
            }
        }
        private void NetMusicDataGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var netMusic = dgr.Item as NetMusic;
                foreach (var music in MusicList)
                {
                    music.IsDownLoaded = false;
                }
                netMusic.IsDownLoaded = true;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (MusicList.SingleOrDefault(m => m.IsDownLoaded) == null)
            {
                MessageBox.Show("请选择替换资源！！！");
                return;
            }
            DialogResult = true;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
