using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using MusicCollection.MusicManager;
using System.Text.RegularExpressions;

namespace MusicCollection.Pages
{
    /// <summary>
    /// LocalMusicPage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalMusicPage : Page
    {
        private ObservableCollection<string> FolderList = new ObservableCollection<string>();
        public ObservableCollection<Music> LocalMusicList = new ObservableCollection<Music>();
        public MainWindow ParentWindow { get; private set; }
        public LocalMusicPage(MainWindow ParentWindow)
        {
            this.ParentWindow = ParentWindow;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            var configPath = "LocalMusicFolderList.json";

            if (!File.Exists(configPath))
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(new ObservableCollection<string>()));
            }

            var content = "";
            try
            {
                content = File.ReadAllText(configPath);
            }
            catch (System.Exception)
            {                
            }

            FolderList = JsonConvert.DeserializeObject<ObservableCollection<string>>(content);
            if (FolderList.Count == 0)
            {
                FolderList.Add("C:\\Users\\HeHang\\Music\\");
            }
            foreach (var item in FolderList)
            {
                DirectoryInfo TheFolder = new DirectoryInfo(item);
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    var pattern = ".*\\.(mp3|flac|wma|wav|ape)$";
                    if (Regex.IsMatch(NextFile.Name, pattern, RegexOptions.IgnoreCase))
                    {
                        LocalMusicList.Add(new Music(item + NextFile.Name));
                    }
                }
            }


            LocalMusicDataGrid.DataContext = LocalMusicList;
        }

        private void Page_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (Height > 0 && Width > 0)
            {
                LocalMusicDataGrid.Height = Height;
                LocalMusicDataGrid.Width = Width;
                GridSplitter1.Width = Width;
                GridSplitter2.Width = Width;
            }
        }
        
        private void RefreshLocalListButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LocalMusicList.Clear();
            foreach (var item in FolderList)
            {
                DirectoryInfo TheFolder = new DirectoryInfo(item);
                foreach (FileInfo NextFile in TheFolder.GetFiles())
                {
                    var pattern = ".*\\.(mp3|flac|wma|wav|ape)$";
                    if (Regex.IsMatch(NextFile.Name, pattern, RegexOptions.IgnoreCase))
                    {
                        LocalMusicList.Add(new Music(item + NextFile.Name));
                    }
                }
            }
        }

        private void LocalMusicDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }

        private void LocalMusicDataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LocalMusicDataGrid.SelectedIndex == -1)
            {
                return;
            }
            var music = LocalMusicDataGrid.SelectedItem as Music;
            ParentWindow.CurrentMusicList.Add(music);
            ParentWindow.CurrentIndex = ParentWindow.CurrentMusicList.Count - 1;
            ParentWindow.bsp.Stop();
            ParentWindow.Play();
        }

        private void LocalMusicDataGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e)
        {

        }
    }
}
