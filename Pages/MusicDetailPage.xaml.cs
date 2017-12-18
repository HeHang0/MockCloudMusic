using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MusicCollection.Pages
{
    /// <summary>
    /// MusicDetailPage.xaml 的交互逻辑
    /// </summary>
    public partial class MusicDetailPage : Page
    {
        private MainWindow ParentWindow;
        private ObservableCollection<LyricLine> Lyric = new ObservableCollection<LyricLine>();
        private int CurrentLyricIndex = -1;
        private DispatcherTimer timer = new DispatcherTimer();

        public ChildWindows.DesktopLyricWindow DesktopLyricWin;
        public MusicDetailPage(MainWindow mainWindow)
        {
            ParentWindow = mainWindow;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            DesktopLyricWin = new ChildWindows.DesktopLyricWindow();
            DesktopLyricWin.Owner = ParentWindow;

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerOnTick;
            ParentWindow.bsp.PropertyChanged += Bsp_PropertyChanged;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private bool haveLyric = false;
        private void Bsp_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var bsp = sender as SoundPlayer.BSoundPlayer;
            if (e.PropertyName == "IsPlaying")
            {
                if (bsp.IsPlaying)
                {
                    timer.Start();
                }
                else
                {
                    timer.Stop();
                }
            }
            else if (e.PropertyName == "IsStop")
            {
                if (bsp.IsStop)
                {
                    RotateTransform rotateTransform = new RotateTransform(0);
                    Image.RenderTransform = rotateTransform;//Spirit = new Image();
                }
            }
        }
        private float angle = 0;
        private void TimerOnTick(object sender, EventArgs e)
        {
            if (!haveLyric)
            {
                timer.Stop();
                return;
            }
            RotateTransform rotateTransform = new RotateTransform((angle+=0.03f) * 180 / 3.142);
            rotateTransform.CenterX = Image.ActualWidth / 2;
            rotateTransform.CenterY = Image.ActualHeight / 2;
            Image.RenderTransform = rotateTransform;
            if (CurrentLyricIndex > LastLyricLineIndex && Lyric[CurrentLyricIndex].StartTime <= ParentWindow.bsp.CurrentTime)
            {
                LastLyricLineIndex = CurrentLyricIndex++;
                if (CurrentLyricIndex == Count)
                {
                    CurrentLyricIndex = -1;
                    LastLyricLineIndex = -1;
                }
                else
                {
                    var run = LyricTextBlock.Inlines.ElementAt(LastLyricLineIndex);
                    run.Foreground = Brushes.Red;
                    LyricBlock.ScrollToVerticalOffset(LyricBlock.ScrollableHeight * LastLyricLineIndex * 1.0 / Count);
                    DesktopLyricWin.Lyric.Content = run;
                }
            }
        }
        

        private int LastLyricLineIndex = -1;
        private int Count = 0;

        public void Init(BitmapImage bi, Music music = null, NetMusic netMusic = null)
        {
            angle = 0;
            Lyric.Clear();
            LyricTextBlock.Inlines.Clear();
            AlbumImage.ImageSource = bi;
            LyricBlock.ScrollToVerticalOffset(0);
            var lyricPath = "";
            if (music != null)
            {
                if (!string.IsNullOrWhiteSpace(music.LyricPath))
                {
                    lyricPath = music.LyricPath;
                }
                else
                {
                    lyricPath = NetMusicHelper.GetLyricByMusic(music);
                    music.LyricPath = lyricPath;
                }
            }
            else if (netMusic != null)
            {
                if (!string.IsNullOrWhiteSpace(netMusic.LyricPath))
                {
                    lyricPath = netMusic.LyricPath;
                }
                else
                {
                    lyricPath = NetMusicHelper.GetLyricByNetMusic(netMusic);
                }
            }

            if (!string.IsNullOrWhiteSpace(lyricPath))
            {
                var lrcEncoding = EncodingHelper.GetType(lyricPath);
                var lrcList = System.IO.File.ReadLines(lyricPath, lrcEncoding);
                foreach (var item in lrcList)
                {
                    Lyric.Add(new LyricLine(item));
                }
                foreach (var item in Lyric)
                {
                    LyricTextBlock.Inlines.Add(new Run(item.Content + "\n\n"));
                }
                Count = LyricTextBlock.Inlines.Count;
                CurrentLyricIndex = 0;
                LastLyricLineIndex = -1;
                haveLyric = true;
            }
            else
            {
                haveLyric = false;
                for (int i = 0; i < 10; i++)
                {
                    LyricTextBlock.Inlines.Add("\n");
                }
                LyricTextBlock.Inlines.Add(new Run("               用 心 去 感 受 音 乐\n") { Foreground = Brushes.Red, FontSize = 16 });
                DesktopLyricWin.Lyric.Content = "               用 心 去 感 受 音 乐\n";
            }
            
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ImageBack.ActualHeight / ImageBack.ActualWidth < 750 / 800)
            {
                Image.Height = ImageBack.ActualHeight * 0.27;
                Image.Width = Image.Height;
                var left = (ImageBack.Margin.Left + (3.0 / 8) * ImageBack.ActualHeight * 80 / 75);
                var bottom = (ImageBack.Margin.Bottom + ImageBack.ActualHeight * 233 / 750);
                var right = ActualWidth - left - Image.Width;
                var top = ActualHeight - bottom - Image.Height;
                Image.Margin = new Thickness(left, top, right, bottom);
                LyricBlock.Margin = new Thickness((3.0 / 8) * ImageBack.ActualHeight + ImageBack.Margin.Left, LyricBlock.Margin.Top, LyricBlock.Margin.Right, LyricBlock.Margin.Bottom);
            }
            else
            {
                Image.Height = ImageBack.ActualWidth * 0.25;
                Image.Width = Image.Height;
                var left = (ImageBack.Margin.Left + ImageBack.ActualWidth*3/8);
                var bottom = (ImageBack.Margin.Bottom + (75.0 / 80) * ImageBack.ActualWidth * 233 / 750);
                var right = ActualWidth - left - Image.Width;
                var top = ActualHeight - bottom - Image.Height;
                Image.Margin = new Thickness(left, top, right, bottom);
                LyricBlock.Margin = new Thickness(ImageBack.ActualWidth + ImageBack.Margin.Left, LyricBlock.Margin.Top, LyricBlock.Margin.Right, LyricBlock.Margin.Bottom);
            }

        }

        private void HiddenButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.MusicDetailFrame.Visibility = Visibility.Hidden;
        }
    }
}
