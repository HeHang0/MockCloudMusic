using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace MusicCollection.Pages
{
    /// <summary>
    /// MusicDetailPage.xaml 的交互逻辑
    /// </summary>
    public partial class MusicDetailPage : Page
    {
        private MainWindow ParentWindow;
        private List<LyricLine> Lyric = new List<LyricLine>();
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
            //ChildWindows.DesktopLyricWindow.TopMostTool.SetTopMost(DesktopLyricWin);
            if (!haveLyric)
            {
                timer.Stop();
                return;
            }
            RotateTransform rotateTransform = new RotateTransform((angle+=0.03f) * 180 / 3.142);
            rotateTransform.CenterX = Image.ActualWidth / 2;
            rotateTransform.CenterY = Image.ActualHeight / 2;
            Image.RenderTransform = rotateTransform;
            if (CurrentLyricIndex < Lyric.Count && CurrentLyricIndex > LastLyricLineIndex && Lyric[CurrentLyricIndex].StartTime <= ParentWindow.bsp.CurrentTime)
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
                    if (LastLyricLineIndex > 0)
                    {
                        var lastRun = LyricTextBlock.Inlines.ElementAt(LastLyricLineIndex - 1);
                        lastRun.Foreground = run.Foreground;
                    }
                    run.Foreground = Brushes.Red;
                    LyricBlock.ScrollToVerticalOffset(LyricBlock.ScrollableHeight * LastLyricLineIndex * 1.0 / Count);
                    if (!string.IsNullOrWhiteSpace(((Run)run).Text))
                    {
                        DesktopLyricWin.Lyric.Content = ((Run)run).Text;
                    }
                }
            }
        }
        

        private int LastLyricLineIndex = -1;
        private int Count = 0;

        public void Init( Music music, ImageSource source)
        {
            angle = 0;
            Lyric.Clear();
            LyricTextBlock.Inlines.Clear();
            if (source != null)
            {
                AlbumImage.ImageSource = source;
            }
            else
            {
                Image.DataContext = "logo.ico";
            }
            LyricBlock.ScrollToVerticalOffset(0);
            DesktopLyricWin.Lyric.Content = music.Title + music.Singer;
            timer.Stop();
            Thread thread = new Thread(new ThreadStart(() => GetLiric(music)));
            thread.IsBackground = true;
            thread.Start();            
        }

        private void GetLiric(Music music)
        {
            var lyricPath = "";
            if (music != null)
            {
                if (!string.IsNullOrWhiteSpace(music.LyricPath))
                {
                    if (Regex.IsMatch(music.LyricPath, "[a-zA-z]+://[^\\s]*"))
                    {
                        if (NetMusicHelper.CheckLink(music.LyricPath))
                        {
                            music.LyricPath = NetMusicHelper.GetLyricByUrl(music,music.LyricPath);
                        }
                        else
                        {
                            lyricPath = NetMusicHelper.GetLyricByMusic(music);
                            music.LyricPath = lyricPath;
                        }
                    }
                    lyricPath = music.LyricPath;
                }
                else
                {
                    lyricPath = NetMusicHelper.GetLyricByMusic(music);
                    music.LyricPath = lyricPath;
                }
            }

            Dispatcher.Invoke(new Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(lyricPath))
                {
                    var lrcEncoding = EncodingHelper.GetType(lyricPath);
                    var lrcList = System.IO.File.ReadLines(lyricPath, lrcEncoding);
                    foreach (var item in lrcList)
                    {
                        var pattern = "\\[([0-9.:]*)\\]";
                        MatchCollection mc = Regex.Matches(item, pattern);
                        foreach (Match line in mc)
                        {
                            Lyric.Add(new LyricLine(Regex.Replace(item, "\\[([0-9.:]*)\\]", ""), TimeSpan.Parse("00:" + line.Groups[1].Value)));
                        }
                    }
                    Lyric = Lyric.OrderBy(m => m.StartTime).ToList();
                    foreach (var item in Lyric)
                    {
                        item.Content = Regex.Replace(item.Content, "<[\\d]{1,4}>", "");
                        LyricTextBlock.Inlines.Add(new Run(item.Content + "\n\n"));
                    }
                    Count = LyricTextBlock.Inlines.Count;
                    CurrentLyricIndex = 0;
                    LastLyricLineIndex = -1;
                    haveLyric = true;
                    timer.Start();
                }
                else
                {
                    haveLyric = false;
                    for (int i = 0; i < 10; i++)
                    {
                        LyricTextBlock.Inlines.Add("\n");
                    }
                    LyricTextBlock.Inlines.Add(new Run("               用 心 去 感 受 音 乐\n") { Foreground = Brushes.Red, FontSize = 16 });
                }
            }));
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
