using MusicCollection.SoundPlayer;
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
using System.Windows.Shapes;
using System.Timers;
using MusicCollection.Pages;
using MusicCollection.MusicManager;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using MusicCollection.MusicAPI;
using System.Threading;

namespace MusicCollection
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public BSoundPlayer bsp = new BSoundPlayer();
        public MusicObservableCollection<Music> CurrentMusicList = new MusicObservableCollection<Music>();
        public MusicHistoriesCollection<MusicHistory> HistoryMusicList = new MusicHistoriesCollection<MusicHistory>();
        public ObservableCollection<NetMusic> NetMusicList = new ObservableCollection<NetMusic>();
        
        public int CurrentIndex = -1;

        public LocalMusicPage LocalMusic;
        public DownLoadMusicPage DownLoadMusic;
        private MusicDetailPage MusicDetail;
        private NetMusicSearchPage NetMusicSearch;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayBar.DataContext = bsp;
            InitMusic();
            InitPages();
        }

        private void InitPages()
        {
            LocalMusic = new LocalMusicPage(this);
            DownLoadMusic = new DownLoadMusicPage(this);
            MusicDetail = new MusicDetailPage(this);
            NetMusicSearch = new NetMusicSearchPage(this);
            PageFrame.Content = LocalMusic;
            CurrentMusicListFrame.Content = new CurrentMusicListAndHistoriesPages(this);
            MusicDetailFrame.Content = new MusicDetailPage(this);
        }

        private void InitMusic()
        {
            CurrentMusicList.CollectionChanged += CurrentMusicList_OnCountChange;
            var content = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\";
            if (!Directory.Exists("Data\\"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("Data\\");
            }
            if (!File.Exists("Data\\CurrentMusicList.json"))
            {
                File.WriteAllText("Data\\CurrentMusicList.json", JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists("Data\\HistoryMusicList.json"))
            {
                File.WriteAllText("Data\\HistoryMusicList.json", JsonConvert.SerializeObject(new MusicHistoriesCollection<MusicHistory>()));
            }


            var CurrentMusicListStr = File.ReadAllText("Data\\CurrentMusicList.json");
            CurrentMusicList = JsonConvert.DeserializeObject<MusicObservableCollection<Music>>(CurrentMusicListStr);

            var HistoryMusicListStr = File.ReadAllText("Data\\HistoryMusicList.json");
            HistoryMusicList = JsonConvert.DeserializeObject<MusicHistoriesCollection<MusicHistory>>(HistoryMusicListStr);

            //foreach (var item in crlist)
            //{
            //    CurrentMusicList.Add(item);
            //}
            //if (CurrentMusicList.Count == 0)
            //{

            //    //DirectoryInfo TheFolder = new DirectoryInfo(content);
            //    //foreach (FileInfo NextFile in TheFolder.GetFiles())
            //    //{
            //    //    var pattern = ".+?(\\.mp3|\\.wav|\\.flac|\\.wma|\\.ape|\\.m4a)$";
            //    //    if (Regex.IsMatch(NextFile.Name, pattern, RegexOptions.IgnoreCase))
            //    //    {
            //    //        CurrentMusicList.Add(new Music(content + NextFile.Name));
            //    //    }
            //    //}
            //}
            if (CurrentMusicList.Count > 0)
            {
                Play();
                Pause();
            }
            CurrentMusicListCountLable.DataContext = CurrentMusicList;
            bsp.PropertyChanged += Bsp_PropertyChanged;
        }

        private void Bsp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MusicEnd" && CurrentMusicList.Count > 0)
            {
                if (++CurrentIndex >= CurrentMusicList.Count)
                {
                    CurrentIndex = 0;
                }
                if (IsShufflePlay)
                {
                    Random ran = new Random();
                    CurrentIndex = ran.Next(0, CurrentMusicList.Count);
                }
                Play();
            }
        }

        private void CurrentMusicList_OnCountChange(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (CurrentMusicList.Count == 0)
            {
                CurrentMusicCanvasMini.Visibility = Visibility.Hidden;
                bsp.Stop();
                CurrentIndex = -1;
            }
            else
            {
                CurrentMusicCanvasMini.Visibility = Visibility.Visible;
            }
        }

        private void Move_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !SearchTextBox.IsFocused)
            {
                DragMove();
                e.Handled = true;
            }
        }

        private void PlayMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (bsp.IsPlaying)
            {
                Pause();
                return;
            }
            else if (bsp.IsPause)
            {
                bsp.Play();
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
            }
            else
            {
                Play();
            }
        }
        public void Pause()
        {
            bsp.Pause();
            PlayMusicButton.Visibility = Visibility.Visible;
            PauseMusicButton.Visibility = Visibility.Hidden;
        }

        public bool Play(Music music = null, NetMusic netMusic = null)
        {
            if (music != null)
            {
                CurrentIndex = CurrentMusicList.Add(music);
            }
            else if (netMusic != null)
            {
                var url = "";
                if (NetMusicHelper.CheckLink(netMusic.Url))
                {
                    url = netMusic.Url;
                }
                else
                {
                    url = NetMusicHelper.GetUrlByNetMusic(netMusic);
                }
                if (url.Length > 0)
                {
                    CurrentIndex = -1;
                    bsp.Stop();
                    bsp.FileName = url;
                }
                else
                {
                    return false;
                }
            }
            else if (CurrentMusicList.Count > 0 && CurrentIndex < 0)
            {
                CurrentIndex = 0;
                //bsp.FileName = CurrentMusicList[CurrentIndex].Url;
            }

            if (CurrentIndex >= 0 && CurrentIndex < CurrentMusicList.Count)
            {
                bsp.Stop();
                bsp.FileName = CurrentMusicList[CurrentIndex].Path;
                bsp.Play();
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
                Title = CurrentMusicList[CurrentIndex].Title + " - " + CurrentMusicList[CurrentIndex].Singer;
                HistoryMusicList.Add(new MusicHistory(CurrentMusicList[CurrentIndex]));
                SetMiniLable(CurrentMusicList[CurrentIndex]);
            }
            else if (netMusic != null)
            {
                bsp.Play();
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
                Title = netMusic.Title + " - " + netMusic.Singer;
                SetMiniLable(netMusic);
            }
            return true;
        }

        private void SetMiniLable(NetMusic netMusic)
        {
            var url = netMusic.AlbumImageUrl;
            if (string.IsNullOrWhiteSpace(url))
            {
                url = "logo.ico";
            }
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            CurrentMusicImageMini.Source = bi;
            CurrentMusicTitleMini.Content = netMusic.Title;
            CurrentMusicSingerMini.Content = netMusic.Singer;
            if (MusicDetailFrame.Content != null)
            {
                (MusicDetailFrame.Content as MusicDetailPage).Init(bi, null, netMusic);
            }
        }

        private void SetMiniLable(Music music)
        {
            var url = music.AlbumImageUrl;
            if (url == "")
            {
                url = "logo.ico";
            }
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.UriSource = new Uri(url, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            CurrentMusicImageMini.Source = bi;
            CurrentMusicTitleMini.Content = music.Title;
            CurrentMusicSingerMini.Content = music.Singer;
            if (MusicDetailFrame.Content != null)
            {
                (MusicDetailFrame.Content as MusicDetailPage).Init(bi, CurrentMusicList[CurrentIndex], null);
            }
        }

        private void LastMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (--CurrentIndex < 0)
            {
                CurrentIndex = CurrentMusicList.Count - 1;
            }
            if (IsShufflePlay && CurrentMusicList.Count > 0)
            {
                Random ran = new Random();
                CurrentIndex = ran.Next(0, CurrentMusicList.Count);
            }
            bsp.Stop();
            Play();
        }


        private void NextMusicButton_Click(object sender, RoutedEventArgs e)
        {
            var count = CurrentMusicList.Count;
            if (((++CurrentIndex >= count) || CurrentIndex == -1) && count > 0)
            {
                CurrentIndex = 0;
            }
            if (IsShufflePlay && count > 0)
            {
                Random ran = new Random();
                CurrentIndex = ran.Next(0, CurrentMusicList.Count);
            }
            bsp.Stop();
            Play();
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            Height = SystemParameters.WorkArea.Height;//获取屏幕的宽高  使之不遮挡任务栏  
            Width = SystemParameters.WorkArea.Width;
            Top = 0;
            Left = 0;

            MaxButton.Visibility = Visibility.Hidden;
            NormalButton.Visibility = Visibility.Visible;

        }
        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            Height = MinHeight;
            Width = MinWidth;
            Top = (SystemParameters.WorkArea.Height - Height) / 2;
            Left = (SystemParameters.WorkArea.Width - Width) / 2;
            MaxButton.Visibility = Visibility.Visible;
            NormalButton.Visibility = Visibility.Hidden;
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MaxButton.Visibility = Visibility.Hidden;
                NormalButton.Visibility = Visibility.Visible;
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //TitleBar.Width = Width;
            //PlayBar.Width = Width;
            //ContentBar.Height = Height-100;
        }

        private void LocalMusicButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = LocalMusic;
        }

        private void CurrentMusicImageMini_MouesOver(object sender, MouseEventArgs e)
        {
            CurrentMusicClickMini.Visibility = Visibility.Visible;
        }

        private void CurrentMusicImageMini_MouesLeave(object sender, MouseEventArgs e)
        {
            CurrentMusicClickMini.Visibility = Visibility.Hidden;
        }

        private void SoundChangeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            bsp.Volume = (float)SoundChangeSlider.Value / 10;
            if (SoundChangeSlider.Value == 0)
            {
                VolOpenButton.Visibility = Visibility.Hidden;
                VolCloseButton.Visibility = Visibility.Visible;
            }
            else
            {
                if (VolOpenButton.Visibility == Visibility.Hidden || VolCloseButton.Visibility == Visibility.Visible)
                {
                    VolOpenButton.Visibility = Visibility.Visible;
                    VolCloseButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private void VolOpenButton_Click(object sender, RoutedEventArgs e)
        {
            SoundChangeSliderValue = SoundChangeSlider.Value;
            SoundChangeSlider.Value = 0;
            //VolOpenButton.Visibility = Visibility.Hidden;
            //VolCloseButton.Visibility = Visibility.Visible;
        }
        private double SoundChangeSliderValue;
        private void VolCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundChangeSliderValue == 0)
            {
                SoundChangeSliderValue = 10;
            }
            SoundChangeSlider.Value = SoundChangeSliderValue;
            //VolOpenButton.Visibility = Visibility.Visible;
            //VolCloseButton.Visibility = Visibility.Hidden;
        }

        private void CurrentMusicListFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void CurrentMusicListButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMusicListFrame.Visibility == Visibility.Visible)
            {
                CurrentMusicListFrame.Visibility = Visibility.Hidden;
            }
            else
            {
                CurrentMusicListFrame.Visibility = Visibility.Visible;
            }
            e.Handled = true;
        }

        private void CurrentMusicCanvasMini_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MusicDetailFrame.Visibility = Visibility.Visible;
        }

        private void CurrentMusicListFrame_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (MusicDetailFrame.Visibility == Visibility.Visible && CurrentIndex > 0)
            //{
            //    (MusicDetailFrame.Content as MusicDetailPage).Init(CurrentMusicList[CurrentIndex]);
            //}
        }

        private void SearchNetMusicButton_Click(object sender, RoutedEventArgs e)
        {
            SearchNetMusic(SearchTextBox.Text);//http://music.163.com/outchain/player?type=2&id={netMusic.MusicID}&auto=1&height=66
        }
        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchNetMusic(SearchTextBox.Text);
            }
        }
        private void SearchNetMusic(string searchStr)
        {
            if (string.IsNullOrWhiteSpace(searchStr))
            {
                return;
            }
            PageFrame.Content = NetMusicSearch;
            NetMusicSearch.LodingImage.Visibility = Visibility.Visible;
            var type = NetMusicSearch.NetMusicTypeRadio.DataContext as NetMusicTypeRadioBtnViewModel;

            NetMusicList.Clear();

            Thread thread = new Thread(new ThreadStart(() => GetNetMusicList(searchStr, type.SelectItem())));
            thread.IsBackground = true;
            thread.Start();            
        }

        private void GetNetMusicList(string searchStr, NetMusicType netMusicType)
        {
            var t = NetMusicHelper.GetNetMusicList(searchStr, netMusicType);
            foreach (var item in t)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    NetMusicList.Add(item);
                }));
            }
            Dispatcher.Invoke(new Action(() =>
            {
                NetMusicSearch.LodingImage.Visibility = Visibility.Hidden;
            }));
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NoFocusButton.Focus();
        }

        private void DownLoadMusicPageButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = DownLoadMusic;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bsp.Stop();
            File.WriteAllText("Data\\CurrentMusicList.json", JsonConvert.SerializeObject(CurrentMusicList));
            File.WriteAllText("Data\\HistoryMusicList.json", JsonConvert.SerializeObject(HistoryMusicList));
            LocalMusic.LocalMusicPage_Closing();
            DownLoadMusic.DownLoadMusicPage_Closing();
        }

        private void PlayBar_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MusicSlider.Width = PlayBar.ActualWidth - 574;
        }

        private void ShufflePlayButton_Click(object sender, RoutedEventArgs e)
        {
            IsShufflePlay = false;
            ShufflePlayButton.Visibility = Visibility.Hidden;
            LoopPlayButton.Visibility = Visibility.Visible;
        }
        private bool IsShufflePlay = true;
        private void LoopPlayButton_Click(object sender, RoutedEventArgs e)
        {
            IsShufflePlay = true;
            ShufflePlayButton.Visibility = Visibility.Visible;
            LoopPlayButton.Visibility = Visibility.Hidden;
        }
    }
}
