using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MusicCollection.Pages
{
    /// <summary>
    /// PlayListPage.xaml 的交互逻辑
    /// </summary>
    public partial class PlayListPage : Page
    {
        private MainWindow ParentWindow;
        private PlayListCollectionModel PageModel;
        public PlayListPage(MainWindow mainWindow)
        {
            InitializeComponent();
            ParentWindow = mainWindow;
        }

        public void Show(PlayListCollectionModel model)
        {
            ParentWindow.PageFrame.Content = this;
            if (string.IsNullOrWhiteSpace(model.ImgUrl))
            {
                foreach (var item in model.PlayList)
                {
                    if (!string.IsNullOrEmpty(item.AlbumImageUrl))
                    {
                        model.ImgUrl = item.AlbumImageUrl;
                        break;
                    }
                }
            }
            PageModel = model;
            DataContext = PageModel;
        }

        private void PlayAllLocalButton_Click(object sender, RoutedEventArgs e)
        {
            if (PageModel != null)
            {
                var list = PageModel.PlayList;
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
        }

        private void AllAddToCurrentListButton_Click(object sender, RoutedEventArgs e)
        {
            if (PageModel != null)
            {
                var list = PageModel.PlayList;
                if (list != null && list.Count > 0)
                {
                    foreach (var item in list)
                    {
                        ParentWindow.CurrentMusicList.Add(new Music(item));
                    }
                }
            }
        }

        private void PlayListMusicDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
            if ((int)e.Row.Header < 10)
            {
                e.Row.Header = "0" + e.Row.Header;
            }
        }

        private void PlayListMusicDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
    }
}
