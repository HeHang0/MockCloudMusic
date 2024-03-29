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
using MusicCollection.ChildWindows;
using System.Runtime.InteropServices;
using System.ComponentModel;
using MusicCollection.Setting;
using System.Linq;

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
            SetNetEasyKey();
            InitNotyfy();
            InitTaskBar();
            InitPages();
            InitMusic();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }

        private void SetNetEasyKey()
        {
            NetMusicHelper.NetEasyCsrfToken = File.ReadAllText(EnvironmentSingle.NetEasyCsrfTokenPath);
            NetMusicHelper.NetEasyMusicU = File.ReadAllText(EnvironmentSingle.NetEasyMusicUPath);
            NetMusicHelper.QQMusicU = File.ReadAllText(EnvironmentSingle.QQMusicUPath);
            NetMusicHelper.MiguMusicU = File.ReadAllText(EnvironmentSingle.MiguMusicUPath);
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
            //BalloonTips("Just Listen");
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (Visibility == Visibility.Hidden)
            {
                Show();
                Activate();
                if(WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }
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
            else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                NotifyIcon_MouseDoubleClick(null, null);
            }
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
            ToolBarPlayPauseButton = new ThumbnailToolBarButton(Properties.Resources.play, "播放");
            ToolBarPlayPauseButton.Enabled = true;
            ToolBarPlayPauseButton.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(btnPlayPause_Click);


            ThumbnailToolBarButton btnNext = new ThumbnailToolBarButton(Properties.Resources.ff, "下一曲");
            btnNext.Enabled = true;
            btnNext.Click += new EventHandler<ThumbnailButtonClickedEventArgs>(btnNext_Click);

            //上一首按钮  
            ThumbnailToolBarButton btnPre = new ThumbnailToolBarButton(Properties.Resources.rw, "上一曲");
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
            CurrentBackGround = MyFavoriteBackGround;
            DiscoverMusic.SetMyFavorite(true);
            CurrentMusicListFrame.Content = new CurrentMusicListAndHistoriesPages(this);
            MusicDetailFrame.Content = MusicDetail;
        }

        private void InitMusic()
        {
            CurrentMusicList.CollectionChanged += CurrentMusicList_OnCountChange;

            var CurrentMusicListStr = File.ReadAllText(EnvironmentSingle.ConfigCurrentMusicList);
            CurrentMusicList = JsonConvert.DeserializeObject<MusicObservableCollection<Music>>(CurrentMusicListStr);

            var HistoryMusicListStr = File.ReadAllText(EnvironmentSingle.ConfigHistoryMusicList);
            HistoryMusicList = JsonConvert.DeserializeObject<MusicHistoriesCollection<MusicHistory>>(HistoryMusicListStr);

            var PlayListCollectionStr = File.ReadAllText(EnvironmentSingle.ConfigPlayListCollection);
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

        private void Bsp_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MusicEnd" && CurrentMusicList.Count > 0)
            {
                //if (++CurrentIndex >= CurrentMusicList.Count)
                //{
                //    CurrentIndex = 0;
                //}
                if (IsShufflePlay == PlayMode.ShufflePlay)
                {
                    Random ran = new Random();
                    CurrentIndex = ran.Next(0, CurrentMusicList.Count);
                }
                else if (IsShufflePlay == PlayMode.LoopPlay && ++CurrentIndex >= CurrentMusicList.Count)
                {
                    CurrentIndex = 0;
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
            ToolBarPlayPauseButton.Icon = Properties.Resources.suspend;
            NotifyWin.NotifyPalyButton.Visibility = Visibility.Hidden;
            NotifyWin.NotifyPauseButton.Visibility = Visibility.Visible;
        }
        public void Pause()
        {
            bsp.Pause();
            NotifyWin.NotifyPalyButton.Visibility = Visibility.Visible;
            NotifyWin.NotifyPauseButton.Visibility = Visibility.Hidden;
            ToolBarPlayPauseButton.Icon = Properties.Resources.play;
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
                (CurrentMusicListFrame.Content as CurrentMusicListAndHistoriesPages).MusicChange(CurrentIndex);
            }
            return true;
        }

        private void SetMiniLable(Music music)
        {
            CurrentMusicImageMini.DataContext = string.IsNullOrWhiteSpace(music.AlbumImageUrl) ? "logo.ico" : NetMusicHelper.GetImgFromRemote(music.AlbumImageUrl);
            CurrentMusicTitleMini.Content = music.Title;
            CurrentMusicSingerMini.Content = music.Singer;
            MusicDetail.Init(CurrentMusicList[CurrentIndex], CurrentMusicImageMini.Source);
            CurrentMusicCanvasMini.Visibility = Visibility.Visible;
        }

        private void LastMusicButton_Click(object sender, RoutedEventArgs e)
        {
            LastMusicPlay();
        }
        public void LastMusicPlay()
        {
            if (IsShufflePlay == PlayMode.ShufflePlay)
            {
                Random ran = new Random();
                CurrentIndex = ran.Next(0, CurrentMusicList.Count);
            }
            else if (IsShufflePlay == PlayMode.LoopPlay && --CurrentIndex < 0)
            {
                CurrentIndex = CurrentMusicList.Count - 1;
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
            if (IsShufflePlay == PlayMode.ShufflePlay)
            {
                Random ran = new Random();
                CurrentIndex = ran.Next(0, CurrentMusicList.Count);
            }
            else if (IsShufflePlay == PlayMode.LoopPlay && ((++CurrentIndex >= count) || CurrentIndex == -1) && count > 0)
            {
                CurrentIndex = 0;
            }
            bsp.Stop();
            Play();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this;
            if (loginWindow.ShowDialog() == true)
            {
                //MessageBox.Show("登录成功");
            }
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            NormalLeft = Left;
            NormalTop = Top;
            NormalWidth = Width;
            NormalHeight = Height;
            MainWindowGrid.Margin = new Thickness(6);
            WindowState = WindowState.Maximized;
            SetBorderRadius();
        }
        private double NormalLeft = 0;
        private double NormalTop = 0;
        private double NormalWidth = 0;
        private double NormalHeight = 0;
        private void NormalButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindowGrid.Margin = new Thickness(8);
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            Width = NormalWidth;
            Height = NormalHeight;
            Left = NormalLeft;
            Top = NormalTop;
            GridRow1.Height = GridLength.Auto;
            GridRow2.Height = GridLength.Auto;
            GridCol1.Width = GridLength.Auto;
            GridCol2.Width = GridLength.Auto;
            SetBorderRadius();
        }

        private void SetBorderRadius()
        {
            if (WindowState == WindowState.Maximized)
            {
                TopBlockBorder.CornerRadius = new CornerRadius(0, 0, 0, 0);
                BottomBlockBorder.CornerRadius = new CornerRadius(0, 0, 0, 0);
            }
            else
            {
                TopBlockBorder.CornerRadius = new CornerRadius(10, 10, 0, 0);
                BottomBlockBorder.CornerRadius = new CornerRadius(0, 0, 10, 10);
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                {
                    NormalButton_Click(null, null);
                }
                else
                {
                    MaxButton_Click(null, null);
                }
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                MaxButton.Visibility = Visibility.Hidden;
                NormalButton.Visibility = Visibility.Visible;
            }
            else
            {
                MaxButton.Visibility = Visibility.Visible;
                NormalButton.Visibility = Visibility.Hidden;
            }
            ProcessWorkArea();
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "WorkArea")
            {
                if (WindowState == WindowState.Maximized)
                {
                    ProcessWorkArea();
                }
            }
        }

        private void ProcessWorkArea()
        {
            var StatusBarWidth = SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Size.Width;
            var StatusBarHeight = SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Size.Height;


            //GridRow1.Height = GridLength.Auto;
            //GridRow2.Height = GridLength.Auto;
            //GridCol1.Width = GridLength.Auto;
            //GridCol2.Width = GridLength.Auto;
            GridRow1.Height = new GridLength(1);
            GridRow2.Height = new GridLength(1);
            GridCol1.Width = new GridLength(1);
            GridCol2.Width = new GridLength(1);
            if (StatusBarWidth == 0 && StatusBarHeight == 0) return;
            if (StatusBarWidth == 0) StatusBarWidth = SystemParameters.PrimaryScreenWidth;
            if (StatusBarHeight == 0) StatusBarHeight = SystemParameters.PrimaryScreenHeight;

            if(SystemParameters.PrimaryScreenWidth == SystemParameters.WorkArea.Size.Width)
            {
                if (SystemParameters.WorkArea.Top == 0)
                {
                    //下
                    GridRow2.Height = new GridLength(StatusBarHeight);
                }
                else if (SystemParameters.WorkArea.Top > 0)
                {
                    //上
                    GridRow1.Height = new GridLength(StatusBarHeight);
                }
            }
            else if(SystemParameters.PrimaryScreenHeight == SystemParameters.WorkArea.Size.Height)
            {
                if (SystemParameters.WorkArea.Right == SystemParameters.PrimaryScreenWidth)
                {
                    //左
                    GridCol1.Width = new GridLength(StatusBarWidth);
                }
                else if (SystemParameters.WorkArea.Right == SystemParameters.WorkArea.Size.Width)
                {
                    //右
                    GridCol2.Width = new GridLength(StatusBarWidth);
                }
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
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
            CurrentMusicListFrame.Visibility = CurrentMusicListFrame.Visibility == Visibility.Visible ? Visibility.Hidden : Visibility.Visible;
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
            File.WriteAllText(EnvironmentSingle.ConfigCurrentMusicList, JsonConvert.SerializeObject(CurrentMusicList));
            File.WriteAllText(EnvironmentSingle.ConfigHistoryMusicList, JsonConvert.SerializeObject(HistoryMusicList));
            File.WriteAllText(EnvironmentSingle.ConfigPlayListCollection, JsonConvert.SerializeObject(PlayListCollection));

            LocalMusic.LocalMusicPage_Closing();
            DownLoadMusic.DownLoadMusicPage_Closing();
            Application.Current.Shutdown();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
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
            IsShufflePlay = PlayMode.LoopPlay;
            ShufflePlayButton.Visibility = Visibility.Hidden;
            LoopPlayButton.Visibility = Visibility.Visible;
            SinglePlayButton.Visibility = Visibility.Hidden;
        }
        private PlayMode IsShufflePlay = PlayMode.ShufflePlay;
        private void LoopPlayButton_Click(object sender, RoutedEventArgs e)
        {
            IsShufflePlay = PlayMode.SinglePlay;
            ShufflePlayButton.Visibility = Visibility.Hidden;
            LoopPlayButton.Visibility = Visibility.Hidden;
            SinglePlayButton.Visibility = Visibility.Visible;
        }

        private void SinglePlayButton_Click(object sender, RoutedEventArgs e)
        {
            IsShufflePlay = PlayMode.ShufflePlay;
            ShufflePlayButton.Visibility = Visibility.Visible;
            LoopPlayButton.Visibility = Visibility.Hidden;
            SinglePlayButton.Visibility = Visibility.Hidden;
        }

        public void DesktopLyricButton_Click(object sender, RoutedEventArgs e)
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
            DiscoverMusic.SetMyFavorite(false);
            ChangeLeftListBackGround(DiscoverMusicBackGround);
        }

        private void MyFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            PageFrame.Content = DiscoverMusic;
            DiscoverMusic.SetMyFavorite(true);
            ChangeLeftListBackGround(MyFavoriteBackGround);
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
            inputStringWindow.Owner = this;
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
                type = NetMusicType.MiguMusic;
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
            inputStringWindow.Owner = this;
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

        enum PlayMode
        {
            ShufflePlay, LoopPlay, SinglePlay
        }
    }
}
