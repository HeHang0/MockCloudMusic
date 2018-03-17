using MusicCollection.ChildWindows;
using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private void ChangeResourcesButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Music music = btn.Tag as Music;
            ChangeResourcesWindow changeResources = new ChangeResourcesWindow(ParentWindow, music.Title + " " + music.Singer);
            if (changeResources.ShowDialog() == true)
            {
                var result = changeResources.MusicList.SingleOrDefault(m => m.IsDownLoaded);
                if (result != null)
                {
                    PageModel.PlayList.Remove(music);
                    PageModel.PlayList.Add(new Music(result));
                }
            }
        }

        private void DeleteMusicButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Music music = btn.Tag as Music;
            InputStringWindow inputStringWindow = new InputStringWindow("删除音乐？", "点击确认删除");
            if (inputStringWindow.ShowDialog() == true)
            {
                PageModel.PlayList.Remove(music);
            }
        }

        private void CheckLinkButton_Click(object sender, RoutedEventArgs e)
        {
            CheckLinkLable.Content = "1";
            CheckLinkLable.Visibility = Visibility.Visible;
            CheckLinkLodingImage.Visibility = Visibility.Visible;
            Thread thread = new Thread(new ThreadStart(() => CheckLink()));
            thread.IsBackground = true;
            thread.Start();
        }

        private void CheckLink()
        {
            for (int i = 0; i < PageModel.PlayList.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(PageModel.PlayList[i].Path))
                {
                    string path = NetMusicHelper.GetUrlByNetMusic(PageModel.PlayList[i]);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PageModel.PlayList[i].Path = path;
                    }));
                }
                if ((Regex.IsMatch(PageModel.PlayList[i].Path, "[a-zA-z]+://[^\\s]*") && !NetMusicHelper.CheckLink(PageModel.PlayList[i].Path)) || string.IsNullOrWhiteSpace(PageModel.PlayList[i].Path))
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        PageModel.PlayList[i].Path = "";
                        PageModel.PlayList[i].IsDisable = true;
                    }));
                }
                Dispatcher.Invoke(new Action(() =>
                {
                    CheckLinkLable.Content = i+1;
                    if (i == PageModel.PlayList.Count-1)
                    {
                        CheckLinkLable.Visibility = Visibility.Hidden;
                        CheckLinkLodingImage.Visibility = Visibility.Hidden;
                    }
                }));
            }
        }
    }
}
