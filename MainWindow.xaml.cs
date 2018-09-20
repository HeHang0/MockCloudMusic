using MusicCollection.SoundPlayer;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MusicCollection.Pages;
using MusicCollection.MusicManager;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using MusicCollection.MusicAPI;
using System.Threading;
using Microsoft.WindowsAPICodePack.Taskbar;
using System.Windows.Interop;
using System.Drawing;
using System.Data;
using MusicCollection.ChildWindows;
using System.Runtime.InteropServices;

namespace MusicCollection
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(int Description, int ReservedValue);
        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private float GetDpi()
        {
            IntPtr desktopWnd = IntPtr.Zero;
            IntPtr dc = GetDC(desktopWnd);
            var dpi = 100f;
            const int LOGPIXELSX = 88;
            try
            {
                dpi = GetDeviceCaps(dc, LOGPIXELSX);
            }
            finally
            {
                ReleaseDC(desktopWnd, dc);
            }
            return dpi / 96f;
        }
        public bool IsConnectInternet()
        {
            int Description = 0;
            return InternetGetConnectedState(Description, 0);
        }
        public BSoundPlayer bsp = new BSoundPlayer();
        public MusicObservableCollection<Music> CurrentMusicList = new MusicObservableCollection<Music>();
        public MusicHistoriesCollection<MusicHistory> HistoryMusicList = new MusicHistoriesCollection<MusicHistory>();
        public ObservableCollection<NetMusic> NetMusicList = new ObservableCollection<NetMusic>();
        public ObservableCollection<PlayListCollectionModel> PlayListCollection = new ObservableCollection<PlayListCollectionModel>();
        
        public int CurrentIndex = -1;

        public DiscoverMusicPage DiscoverMusic;
        public LocalMusicPage LocalMusic;
        public DownLoadMusicPage DownLoadMusic;
        private MusicDetailPage MusicDetail;
        private NetMusicSearchPage NetMusicSearch;
        private PlayListPage PlayList;
        private RankingListPage RankingList;

        private System.Windows.Controls.Image CurrentBackGround;

        ChildWondows.NotifyWindow NotifyWin;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PlayBar.DataContext = bsp;
            InitNotyfy();
            InitTaskBar();
            InitPages();
            InitMusic();
        }

        System.Windows.Forms.NotifyIcon notifyIcon;
        private void InitNotyfy()
        {
            NotifyWin = new ChildWondows.NotifyWindow(this);
            NotifyWin.Owner = this;
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = Title;//最小化到托盘时，鼠标点击时显示的文本
            notifyIcon.Icon = FromImageSource(Icon);//程序图标
            notifyIcon.Visible = true;
            notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            //notifyIcon.MouseDown += NotifyIcon_MouseDown;
            notifyIcon.MouseClick += NotifyIcon_MouseDown;
            BalloonTips("Just Listen");
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Visibility == Visibility.Hidden)
            {
                Show();
            }
            else if (Visibility == Visibility.Visible)
            {
                Hide();
            }
        }

        public void BalloonTips(string msg)
        {
            notifyIcon.BalloonTipText = msg; //设置托盘提示显示的文本
            notifyIcon.ShowBalloonTip(500);
        }

        private void NotifyIcon_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (NotifyWin == null)
                {
                    NotifyWin = new ChildWondows.NotifyWindow(this);
                    NotifyWin.Owner = this;
                }
                System.Drawing.Point pt = System.Windows.Forms.Control.MousePosition;//WPF方法
                NotifyWin.Show();
                float dpi = GetDpi();
                NotifyWin.Left = pt.X/dpi;
                NotifyWin.Top = pt.Y/dpi - NotifyWin.ActualHeight;
                NotifyWin.Activate();
            }
            //else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            //{
            //    if (Visibility == Visibility.Hidden)
            //    {
            //        Show();
            //    }
            //    else if (Visibility == Visibility.Visible)
            //    {
            //        WindowState = WindowState.Normal;
            //    }
            //}
        }

        private static Icon FromImageSource(ImageSource icon)
        {
            if (icon == null)
            {
                return null;
            }
            Uri iconUri = new Uri(icon.ToString());
            return new Icon(System.Windows.Application.GetResourceStream(iconUri).Stream);
        }
        ThumbnailToolBarButton ToolBarPlayPauseButton;
        private void InitTaskBar()
        {
            //播放按钮
            ToolBarPlayPauseButton = new ThumbnailToolBarButton(Properties.Resources.Play, "播放");
            ToolBarPlayPauseButton.Enabled = true;
            ToolBarPlayPauseButton.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(btnPlayPause_Click);


            ThumbnailToolBarButton btnNext = new ThumbnailToolBarButton(Properties.Resources.Next, "下一曲");
            btnNext.Enabled = true;
            btnNext.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(btnNext_Click);

            //上一首按钮  
            ThumbnailToolBarButton btnPre = new ThumbnailToolBarButton(Properties.Resources.Last, "上一曲");
            btnNext.Enabled = true;
            btnPre.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(btnPre_Click);

            //添加按钮  
            TaskbarManager.Instance.ThumbnailToolBars.AddButtons(new WindowInteropHelper(Application.Current.MainWindow).Handle, btnPre, ToolBarPlayPauseButton, btnNext);

            //裁剪略缩图，后面提到  
            //TaskbarManager.Instance.TabbedThumbnail.SetThumbnailClip(new WindowInteropHelper(this).Handle, new System.Drawing.Rectangle(5, 570, 45, 43));
        }

        private void btnNext_Click(object sender, ThumbnailButtonClickedEventArgs e)
        {
            
            NextMusicPlay();
        }

        private void btnPre_Click(object sender, ThumbnailButtonClickedEventArgs e)
        {
            LastMusicPlay();
        }

        private void btnPlayPause_Click(object sender, ThumbnailButtonClickedEventArgs e)
        {
            PlayMusicButton_Click(new object(), new RoutedEventArgs());
        }

        private void InitPages()
        {
            LocalMusic = new LocalMusicPage(this);
            DiscoverMusic = new DiscoverMusicPage(this);
            DownLoadMusic = new DownLoadMusicPage(this);
            MusicDetail = new MusicDetailPage(this);
            NetMusicSearch = new NetMusicSearchPage(this);
            PlayList = new PlayListPage(this);
            RankingList = new RankingListPage(this);
            DataContext = NetMusicSearch;
            PageFrame.Content = DiscoverMusic;
            CurrentBackGround = DiscoverMusicBackGround;
            CurrentMusicListFrame.Content = new CurrentMusicListAndHistoriesPages(this);
            MusicDetailFrame.Content = MusicDetail;
        }

        private void InitMusic()
        {
            CurrentMusicList.CollectionChanged += CurrentMusicList_OnCountChange;

            var CurrentMusicListStr = File.ReadAllText("Data\\CurrentMusicList.json");
            CurrentMusicList = JsonConvert.DeserializeObject<MusicObservableCollection<Music>>(CurrentMusicListStr);

            var HistoryMusicListStr = File.ReadAllText("Data\\HistoryMusicList.json");
            HistoryMusicList = JsonConvert.DeserializeObject<MusicHistoriesCollection<MusicHistory>>(HistoryMusicListStr);

            var PlayListCollectionStr = File.ReadAllText("Data\\PlayListCollection.json");
            PlayListCollection = JsonConvert.DeserializeObject<ObservableCollection<PlayListCollectionModel>>(PlayListCollectionStr);
            PlayListListBox.DataContext = PlayListCollection;

            //if (CurrentMusicList.Count > 0)
            //{
            //    PlayMusicButton_Click(new object(), new RoutedEventArgs());
            //    PlayMusicButton_Click(new object(), new RoutedEventArgs());
            //}
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
            else if (e.PropertyName == "DeviceCount" && bsp.IsPlaying)
            {
                var tmp = bsp.CurrentMusicPosition;
                Play();
                Thread thread = new Thread(new ThreadStart(() => SetPos(tmp)));
                thread.IsBackground = true;
                thread.Start();
                
            }
        }

        private void SetPos(double tmp)
        {
            Thread.Sleep(2000);
            Dispatcher.Invoke(new Action(() =>
            {
                bsp.CurrentMusicPosition = tmp;
            }));
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

        public void PlayMusicButton_Click(object sender, RoutedEventArgs e)
        {
            if (bsp.IsPlaying)
            {
                Pause();
                return;
            }
            else if (bsp.IsPause)
            {
                bsp.Play();
            }
            else
            {
                Play();
            }
            PlayMusicButton.Visibility = Visibility.Hidden;
            PauseMusicButton.Visibility = Visibility.Visible;
            ToolBarPlayPauseButton.Icon = Properties.Resources.Pause;
            NotifyWin.NotifyPalyButton.Visibility = Visibility.Hidden;
            NotifyWin.NotifyPauseButton.Visibility = Visibility.Visible;
        }
        public void Pause()
        {
            bsp.Pause();
            NotifyWin.NotifyPalyButton.Visibility = Visibility.Visible;
            NotifyWin.NotifyPauseButton.Visibility = Visibility.Hidden;
            ToolBarPlayPauseButton.Icon = Properties.Resources.Play;
            PlayMusicButton.Visibility = Visibility.Visible;
            PauseMusicButton.Visibility = Visibility.Hidden;
        }

        public bool Play(Music music = null, NetMusic netMusic = null)
        {
            var tmpIndex = CurrentIndex;
            if (music != null)
            {
                CurrentIndex = CurrentMusicList.Add(music);
            }
            else if (netMusic != null)
            {
                CurrentIndex = CurrentMusicList.Add(new Music(netMusic));
            }
            if (CurrentIndex >= 0 && CurrentIndex < CurrentMusicList.Count && CurrentMusicList[CurrentIndex].Origin != NetMusicType.LocalMusic)
            {
                var url = "";
                if (NetMusicHelper.CheckLink(CurrentMusicList[CurrentIndex].Path))
                {
                    url = CurrentMusicList[CurrentIndex].Path;
                }
                else
                {
                    url = NetMusicHelper.GetUrlByNetMusic(CurrentMusicList[CurrentIndex]);
                    if (!NetMusicHelper.CheckLink(url))
                    {
                        CurrentMusicList.RemoveAt(CurrentIndex);
                        CurrentIndex = tmpIndex;
                        Play();
                        return false;
                    }
                    CurrentMusicList[CurrentIndex].Path = url;
                }
                if (url.Length <= 0)
                {
                    CurrentMusicList.Remove(CurrentMusicList[CurrentIndex]);
                    CurrentIndex = tmpIndex;
                    return false;
                }
            }
            else if (CurrentMusicList.Count > 0 && (CurrentIndex < 0 || CurrentIndex >= CurrentMusicList.Count))
            {
                CurrentIndex = 0;
            }
            if (CurrentIndex >= 0 && CurrentIndex < CurrentMusicList.Count)
            {
                if (Regex.IsMatch(CurrentMusicList[CurrentIndex].Path, "[http|https]://") && !NetMusicHelper.CheckLink(CurrentMusicList[CurrentIndex].Path))
                {
                    CurrentMusicList[CurrentIndex].Path = string.Empty;
                    return false;
                }
                bsp.Stop();
                bsp.FileName = CurrentMusicList[CurrentIndex].Path;
                bsp.Play();
                CurrentMusicList[CurrentIndex].Duration = bsp.TotalTime;
                PlayMusicButton.Visibility = Visibility.Hidden;
                PauseMusicButton.Visibility = Visibility.Visible;
                Title = CurrentMusicList[CurrentIndex].Title + " - " + CurrentMusicList[CurrentIndex].Singer;
                notifyIcon.Text = Title.Length >= 63 ? Title.Substring(0,60) + "..." : Title;
                HistoryMusicList.Add(new MusicHistory(CurrentMusicList[CurrentIndex]));
                SetMiniLable(CurrentMusicList[CurrentIndex]);
            }
            return true;
        }

        private void SetMiniLable(Music music)
        {
            CurrentMusicImageMini.DataContext = string.IsNullOrWhiteSpace(music.AlbumImageUrl) ? "logo.ico" : music.AlbumImageUrl;
            CurrentMusicTitleMini.Content = music.Title;
            CurrentMusicSingerMini.Content = music.Singer;
            MusicDetail.Init(CurrentMusicList[CurrentIndex], CurrentMusicImageMini.Source);
        }

        private void LastMusicButton_Click(object sender, RoutedEventArgs e)
        {
            LastMusicPlay();
        }
        public void LastMusicPlay()
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
            NextMusicPlay();
        }
        public void NextMusicPlay()
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
            Hide();
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
            ChangeLeftListBackGround(LocalMusicBackGround);
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
            //NetMusicSearch.LodingImage.Visibility = Visibility.Visible;
            NetMusicSearch.MusicCount = 0;
            NetMusicSearch.Offset = 0;
            NetMusicSearch.SearchStr = searchStr;
            //var type = NetMusicSearch.NetMusicTypeRadio.DataContext as NetMusicTypeRadioBtnViewModel;

            NetMusicList.Clear();

            Thread thread = new Thread(new ThreadStart(() => NetMusicSearch.GetNetMusicList(searchStr, NetMusicSearch.Offset, NetMusicSearch.PageType)));
            thread.IsBackground = true;
            thread.Start();            
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            NoFocusButton.Focus();
        }

        private void DownLoadMusicPageButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = DownLoadMusic;
            ChangeLeftListBackGround(DownLoadMusicBackGround);
        }
        public void CloseApp()
        {
            bsp.Stop();
            notifyIcon.Dispose();
            File.WriteAllText("Data\\CurrentMusicList.json", JsonConvert.SerializeObject(CurrentMusicList));
            File.WriteAllText("Data\\HistoryMusicList.json", JsonConvert.SerializeObject(HistoryMusicList));
            File.WriteAllText("Data\\PlayListCollection.json", JsonConvert.SerializeObject(PlayListCollection));

            LocalMusic.LocalMusicPage_Closing();
            DownLoadMusic.DownLoadMusicPage_Closing();
            Application.Current.Shutdown();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
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

        private void DesktopLyricButton_Click(object sender, RoutedEventArgs e)
        {
            if (MusicDetail.DesktopLyricWin.Visibility == Visibility.Visible)
            {
                MusicDetail.DesktopLyricWin.Visibility = Visibility.Hidden;
            }
            else
            {
                MusicDetail.DesktopLyricWin.Visibility = Visibility.Visible;
            }
        }
        private void ChangeLeftListBackGround(System.Windows.Controls.Image img)
        {
            if (CurrentBackGround != img)
            {
                if (CurrentBackGround != null)
                {
                    CurrentBackGround.Visibility = Visibility.Hidden;
                }
                CurrentBackGround = img;
                CurrentBackGround.Visibility = Visibility.Visible;
            }
        }
        private void DiscoverMusicButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = DiscoverMusic;
            ChangeLeftListBackGround(DiscoverMusicBackGround);
        }

        private void RankingListButtonButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = RankingList;
            ChangeLeftListBackGround(RankingListBackGround);
        }

        private void PlayListCollectionButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var model = btn.Tag as PlayListCollectionModel;
            PlayList.Show(model);

            if (CurrentBackGround != null)
            {
                CurrentBackGround.Visibility = Visibility.Hidden;
                CurrentBackGround = null;
            }
        }

        private void AddNewPlayListButton_Click(object sender, RoutedEventArgs e)
        {
            InputStringWindow inputStringWindow = new InputStringWindow("添加歌单","歌单链接");
            if (inputStringWindow.ShowDialog() == true)
            {
                string url = inputStringWindow.InputString;
                Thread thread = new Thread(new ThreadStart(() => AddNewPlayList(url)));
                thread.IsBackground = true;
                thread.Start();
            }
        }

        private void AddNewPlayList(string url)
        {
            string name = string.Empty, imgurl = string.Empty;
            NetMusicType type;
            if (url.Contains("music.163.com"))
            {
                type = NetMusicType.CloudMusic;
            }
            else if (url.Contains("y.qq.com"))
            {
                type = NetMusicType.QQMusic;
            }
            else if (url.Contains("xiami.com"))
            {
                type = NetMusicType.XiaMiMusic;
            }
            else
            {
                return;
            }
            var nlist = NetMusicHelper.GetPlayListItems(url, type, out name, out imgurl);
            ObservableCollection<Music> mlist = new ObservableCollection<Music>();
            foreach (var item in nlist)
            {
                mlist.Add(new Music(item));
            }
            if (mlist.Count > 0)
            {

                Dispatcher.Invoke(new Action(() =>
                {
                    PlayListCollection.Add(new PlayListCollectionModel(name, imgurl, mlist));
                }));
            }
        }
        public bool GetStringFromInputStringWindow(string title, string subtitle, out string result)
        {
            InputStringWindow inputStringWindow = new InputStringWindow(title, subtitle);
            if (inputStringWindow.ShowDialog() == true)
            {
                result = inputStringWindow.InputString;
                return true;
            }
            else
            {
                result = "";
                return false;
            }
        }
    }
}
