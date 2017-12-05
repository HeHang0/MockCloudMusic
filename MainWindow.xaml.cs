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
        Timer timer = new System.Timers.Timer();

        private LocalMusicPage LocalMusic;
        private MusicDetailPage MusicDetail;
        private NetMusicSearchPage NetMusicSearch;

        public MainWindow()
        {
            InitializeComponent();
            InitMusic();
            InitTimer();
            InitPages();
        }

        private void InitPages()
        {
            LocalMusic = new LocalMusicPage(this);
            MusicDetail = new MusicDetailPage(this);
            PageFrame.Content = LocalMusic;
            CurrentMusicListFrame.Content = new CurrentMusicListAndHistoriesPages(this);
            MusicDetailFrame.Content = new MusicDetailPage(this);
        }

        private void InitMusic()
        {
            CurrentMusicList.OnCountChange += CurrentMusicList_OnCountChange;
            var content = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) + "\\";

            if (!File.Exists("CurrentMusicList.json"))
            {
                File.WriteAllText("CurrentMusicList.json", JsonConvert.SerializeObject(new MusicObservableCollection<Music>()));
            }
            if (!File.Exists("HistoryMusicList.json"))
            {
                File.WriteAllText("HistoryMusicList.json", JsonConvert.SerializeObject(new MusicHistoriesCollection<MusicHistory>()));
            }


            var CurrentMusicListStr = File.ReadAllText("CurrentMusicList.json");
            CurrentMusicList = JsonConvert.DeserializeObject<MusicObservableCollection<Music>>(CurrentMusicListStr);

            var HistoryMusicListStr = File.ReadAllText("HistoryMusicList.json");
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
                Pause() ;
            }
            CurrentMusicListCountLable.DataContext = CurrentMusicList;
        }

        private void CurrentMusicList_OnCountChange(object sender)
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

        private void InitTimer()
        {
            //System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 1000;//执行间隔时间,单位为毫秒;此时时间间隔为1分钟
            timer.Elapsed += new System.Timers.ElapsedEventHandler((s, e) => CheckTime(s, e));
        }

        private void CheckTime(object s, ElapsedEventArgs e)
        {
            if (bsp.IsPlaying)
            {
                Dispatcher.Invoke(new Action(() => {
                    CurrentTimeLabel.Content = bsp.CurrentTime.ToString(@"mm\:ss");
                    MusicSlider.Value = (bsp.CurrentTime.TotalSeconds / bsp.TotalTime.TotalSeconds) * 10;
                }));
            }
            else if (bsp.IsStop && CurrentMusicList.Count > 0)
            {
                Dispatcher.Invoke(new Action(() => {
                    if (++CurrentIndex >= CurrentMusicList.Count)
                    {
                        CurrentIndex = 0;
                    }
                    bsp.Stop();
                    Play();
                }));
            }
            //else if (bsp.PlaybackState == NAudio.Wave.PlaybackState.Stopped)
            //{
            //    if (++CurrentIndex >= FileList.Count)
            //    {
            //        CurrentIndex = 0;
            //    }
            //    bsp.Stop();
            //    Play();
            //}
        }

        private void Move_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !SearchTextBox.IsFocused)
            {
                timer.Stop();
                DragMove();
                e.Handled = true;
                timer.Start();
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

        public void Play(Music music = null)
        {            
            if (CurrentIndex >= 0 && CurrentIndex < CurrentMusicList.Count)
            {
                bsp.FileName = music == null ? CurrentMusicList[CurrentIndex].Url :music.Url;
                bsp.Play();
                //SetMiniLable(CurrentMusicList[CurrentIndex]);
                ToTalTimeLabel.Content = bsp.TotalTime.ToString(@"mm\:ss");
                timer.Start();
            }
            else if (CurrentMusicList.Count > 0)
            {
                CurrentIndex = 0;
                bsp.FileName = music == null ? CurrentMusicList[CurrentIndex].Url : music.Url;
                bsp.Play();
                ToTalTimeLabel.Content = bsp.TotalTime.ToString(@"mm\:ss");
                timer.Start();
            }

            if (CurrentIndex >= 0)
            {
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
                Title = CurrentMusicList[CurrentIndex].Title + " - " + CurrentMusicList[CurrentIndex].Singer;
                HistoryMusicList.Add(new MusicHistory(CurrentMusicList[CurrentIndex]));
                SetMiniLable(CurrentMusicList[CurrentIndex]);
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
                (MusicDetailFrame.Content as MusicDetailPage).Init(CurrentMusicList[CurrentIndex]);
            }
        }

        private void LastMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (--CurrentIndex < 0)
            {
                CurrentIndex = CurrentMusicList.Count - 1;
            }
            bsp.Stop();
            Play();
        }


        private void NextMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (++CurrentIndex >= CurrentMusicList.Count)
            {
                CurrentIndex = 0;
            }
            bsp.Stop();
            Play();
        }

        private void MusicSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentIndex >= 0)
            {
                var CurrentSeconds = (int)(bsp.TotalTime.TotalSeconds * MusicSlider.Value / 10);
                bsp.CurrentTime = new TimeSpan(0, 0, CurrentSeconds);
                timer.Start();
                bsp.Play();
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
            }
        }        

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MusicSlider.AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(MusicSlider_MouseLeftButtonUp), true);
            MusicSlider.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(MusicSlider_MouseLeftButtonDown), true);
        }

        private void MusicSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            timer.Close();
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
            if (MusicDetailFrame.Content != null)
            {
                (MusicDetailFrame.Content as MusicDetailPage).StopTimer();
            }
            Application.Current.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //TitleBar.Width = Width;
            //PlayBar.Width = Width;
            //ContentBar.Height = Height-100;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NoFocusButton.Focus();
        }

        private void LocalMusicButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = LocalMusic;            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Close();
            File.WriteAllText("CurrentMusicList.json", JsonConvert.SerializeObject(CurrentMusicList));
            File.WriteAllText("HistoryMusicList.json", JsonConvert.SerializeObject(HistoryMusicList));
            LocalMusic.ParentWindow_Closing();
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
            VolOpenButton.Visibility = Visibility.Hidden;
            VolCloseButton.Visibility = Visibility.Visible;
        }
        private double SoundChangeSliderValue;
        private void VolCloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (SoundChangeSliderValue == 0)
            {
                SoundChangeSliderValue = 10;
            }
            SoundChangeSlider.Value = SoundChangeSliderValue;
            VolOpenButton.Visibility = Visibility.Visible;
            VolCloseButton.Visibility = Visibility.Hidden;
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
            if (MusicDetailFrame.Visibility == Visibility.Visible && CurrentIndex > 0)
            {
                (MusicDetailFrame.Content as MusicDetailPage).Init(CurrentMusicList[CurrentIndex]);
            }
        }

        private void SearchNetMusicButton_Click(object sender, RoutedEventArgs e)
        {
            SearchNetMusic(SearchTextBox.Text);
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
            if (NetMusicSearch == null)
            {
                NetMusicSearch = new NetMusicSearchPage(this);
            }
            PageFrame.Content = NetMusicSearch;

            var type = NetMusicSearch.NetMusicTypeRadio.DataContext as NetMusicTypeRadioBtnViewModel;

            NetMusicList.Clear();
            var t = NetMusicHelper.GetNetMusicList(searchStr, type.SelectItem());
            foreach (var item in t)
            {
                NetMusicList.Add(item);
            }
        }
    }
}
