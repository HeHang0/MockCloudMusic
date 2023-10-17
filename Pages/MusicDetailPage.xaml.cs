using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using MusicCollection.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
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
        private const int CurrenLineFontSizeRatio = 5;

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
            ImageCircle.DataContext = "logo.ico";
            PlayBackImage.DataContext = "logo.ico";

            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerOnTick;
            ParentWindow.bsp.PropertyChanged += Bsp_PropertyChanged;
        }

        private Storyboard AlbumLogoStoryboard = new Storyboard();
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation()
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(25),
                RepeatBehavior = RepeatBehavior.Forever,
            };
            Storyboard.SetTargetName(animation, "AlbumLogoAnimation");
            Storyboard.SetTargetProperty(animation, new PropertyPath("(0)", new DependencyProperty[] { RotateTransform.AngleProperty }));
            AlbumLogoStoryboard.Children.Add(animation);
            Timeline.SetDesiredFrameRate(AlbumLogoStoryboard, 20);
            AlbumLogoStoryboard.Begin(ImageCircle, true);
            AlbumLogoStoryboard.Pause(ImageCircle);
        }

        private void ImagePlayNeedleRun(bool forward=true)
        {
            var animation = new DoubleAnimation()
            {
                //From = forward ? -30 : 10,
                To = forward ? 0 : -40,
                Duration = TimeSpan.FromSeconds(0.8)
            };
            ImagePlayNeedle.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, animation);
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
                    AlbumLogoStoryboard.Resume(ImageCircle);
                    ImagePlayNeedleRun(true);
                }
                else
                {
                    AlbumLogoStoryboard.Pause(ImageCircle);
                    timer.Stop();
                    ImagePlayNeedleRun(false);
                }
            }
        }
        //private float angle = 0;
        private void TimerOnTick(object sender, EventArgs e)
        {
            //ChildWindows.DesktopLyricWindow.TopMostTool.SetTopMost(DesktopLyricWin);
            if (!haveLyric)
            {
                timer.Stop();
                return;
            }
            //RotateTransform rotateTransform = new RotateTransform((angle+=0.03f) * 180 / 3.142);
            //rotateTransform.CenterX = ImageCircle.ActualWidth / 2;
            //rotateTransform.CenterY = ImageCircle.ActualHeight / 2;
            //ImageCircle.RenderTransform = rotateTransform;
            if (LastLyricLineIndex > 0 && LastLyricLineIndex < Lyric.Count && Lyric[LastLyricLineIndex].StartTime > ParentWindow.bsp.CurrentTime)
            {
                for (int i = CurrentLyricIndex-1; i >= 0; i--)
                {
                    if(Lyric[i].StartTime <= ParentWindow.bsp.CurrentTime)
                    {
                        var lastRun = LyricTextBlock.Inlines.ElementAt(LastLyricLineIndex);
                        lastRun.Foreground = Brushes.Black;
                        lastRun.FontSize -= CurrenLineFontSizeRatio;
                        CurrentLyricIndex = i;
                        LastLyricLineIndex = i - 1;
                        break;
                    }
                }
            }
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
                        lastRun.FontSize -= CurrenLineFontSizeRatio;
                    }
                    run.Foreground = Brushes.Red;
                    run.FontSize += CurrenLineFontSizeRatio;
                    LyricBlock.ScrollToVerticalOffset(LyricBlock.ScrollableHeight * LastLyricLineIndex * 1.0 / Count);
                    if (DesktopLyricWin.Visibility == Visibility.Visible)
                    {
                        DesktopLyricWin.Lyric.Content = ((Run)run).Text;
                        DesktopLyricWin.SetTopMost();
                    }
                }
            }
        }
        

        private int LastLyricLineIndex = -1;
        private int Count = 0;

        public void Init(Music music, ImageSource source)
        {
            //angle = 0;
            Lyric.Clear();
            LyricTextBlock.Inlines.Clear();
            if (source != null)
            {
                AlbumImage.ImageSource = source;
                System.Windows.Media.Imaging.BitmapFrame bf = null;
                try
                {
                    bf = (System.Windows.Media.Imaging.BitmapFrame)source;
                }
                catch (Exception)
                {
                }
                music.AlbumImageUrl = NetMusicHelper.GetImgFromRemote(music.AlbumImageUrl);
                if (bf != null && bf.IsDownloading && music.AlbumImageUrl.StartsWith("http"))
                {
                    new Thread(new ThreadStart(() => SetPlayBackImage(music.AlbumImageUrl, bf))) { IsBackground = true }.Start();
                }
                else if (!music.AlbumImageUrl.StartsWith("http"))
                {
                    PlayBackImage.Source = Utils.GaussianBlur(music.AlbumImageUrl, 61);
                    SetPlayBackImageRG(PlayBackImage.Source.Width, PlayBackImage.Source.Height);
                }
                else
                {
                    PlayBackImage.Source = Utils.GaussianBlur(source, 61);
                    SetPlayBackImageRG(PlayBackImage.Source.Width, PlayBackImage.Source.Height);
                }
            }
            else
            {
                ImageCircle.DataContext = "logo.ico";
                PlayBackImage.DataContext = "logo.ico";
            }

            LyricTitle.Content = ParentWindow.Title;
            LyricBlock.ScrollToVerticalOffset(0);

            DesktopLyricWin.Lyric.Content = ParentWindow.Title;
            DesktopLyricWin.Title = ParentWindow.Title;
            timer.Stop();
            Thread thread = new Thread(new ThreadStart(() => GetLiric(music)));
            thread.IsBackground = true;
            thread.Start();
        }

        private void SetPlayBackImage(string albumImageUrl, System.Windows.Media.Imaging.BitmapFrame source)
        {
            if (string.IsNullOrWhiteSpace(albumImageUrl)) return;
            for (int i = 0; i < 20; i++)
            {
                if(!NetMusicHelper.GetImgFromRemote(albumImageUrl).StartsWith("http"))
                {
                    break;
                }
                else
                {
                    Thread.Sleep(300);
                }
            }
            albumImageUrl = NetMusicHelper.GetImgFromRemote(albumImageUrl);
            if (!albumImageUrl.StartsWith("http"))
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    PlayBackImage.Source = Utils.GaussianBlur(albumImageUrl, 61);
                    if (PlayBackImage.Source != null)
                    {
                        SetPlayBackImageRG(PlayBackImage.Source.Width, PlayBackImage.Source.Height);
                    }
                }));
            }
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
                if (!string.IsNullOrWhiteSpace(lyricPath) && File.Exists(lyricPath))
                {
                    var lrcEncoding = EncodingHelper.GetType(lyricPath);
                    var lrcList = System.IO.File.ReadLines(lyricPath, lrcEncoding);
                    foreach (var item in lrcList)
                    {
                        var pattern = "\\[([0-9.:]*)\\]";
                        MatchCollection mc = Regex.Matches(item, pattern);
                        foreach (Match line in mc)
                        {
                            try
                            {
                                Lyric.Add(new LyricLine(Regex.Replace(item, "\\[([0-9\\.:]*)\\]", ""), TimeSpan.Parse("00:" + line.Groups[1].Value)));
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    Lyric = Lyric.OrderBy(m => m.StartTime).ToList();
                    foreach (LyricLine item in Lyric)
                    {
                        //item.Content = Regex.Replace(item.Content, "<[\\d]{1,4}>", "");
                        LyricTextBlock.Inlines.Add(new Run(item.Content.Trim() + "\n\n"));
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
            var tmpHeight = ImageBack.ActualHeight * 0.41;
            var tmpWidth = ImageBack.ActualWidth * 0.41;
            ImageCircle.Height = Math.Max(tmpHeight, tmpWidth);
            ImageCircle.Width = ImageCircle.Height;
            ImageCircleBorder.Height = ImageBack.ActualHeight + 10;
            ImageCircleBorder.Width = ImageBack.ActualHeight + 10;
            ImageCircleBlackBorder.Height = Math.Max(ImageBack.ActualHeight - 8, 0);
            ImageCircleBlackBorder.Width = Math.Max(ImageBack.ActualHeight - 8, 0);
            //var left = (ImageBack.Margin.Left + (ImageBack.ActualWidth / 2) - (ImageCircle.Width / 2));
            //var bottom = (ImageBack.Margin.Bottom + ImageBack.ActualHeight * 233 / 750);
            //var right = ActualWidth - left - ImageCircle.Width;
            //var top = (ImageBack.Margin.Top + ImageBack.ActualHeight * 0.4147);
            //ImageCircle.Margin = new Thickness(left, top, 0, 0);
            //AlbumImage.Rect = new Rect(0, 0, ImageCircle.Width, ImageCircle.Height);
            ImagePlayNeedle.Width = ImageCircle.Height / 0.41;
            ImagePlayNeedle.Height = ImagePlayNeedle.Width;
            ImagePlayNeedle.Margin = new Thickness(ImagePlayNeedle.Margin.Left, -ImageBack.ActualHeight*1.2, ImagePlayNeedle.Margin.Right, ImagePlayNeedle.Margin.Bottom);
            SetPlayBackImageRG();
        }

        private void SetPlayBackImageRG(double width = -1, double height = -1)
        {
            double imgWidth = width != 1 ? width : PlayBackImage.ActualWidth;
            double imgHeight = height != 1 ? height : PlayBackImage.ActualHeight;
            if (imgWidth > imgHeight)
            {
                double tmpHeight = imgHeight;
                imgHeight = PlayBackImageBorder.ActualHeight;
                imgWidth = imgHeight * imgWidth / tmpHeight;
            }
            else
            {
                double tmpWidth = imgWidth;
                imgWidth = PlayBackImageBorder.ActualWidth;
                imgHeight = imgWidth * imgHeight / tmpWidth;
            }
            double pbiH = (imgHeight - PlayBackImageBorder.ActualHeight) / 2;
            double pbiW = (imgWidth - PlayBackImageBorder.ActualWidth) / 2;
            PlayBackImageRG.Rect = new Rect(pbiW, pbiH, PlayBackImageBorder.ActualWidth, PlayBackImageBorder.ActualHeight);
            if (ParentWindow.WindowState == WindowState.Maximized) PlayBackImageBorder.CornerRadius = new CornerRadius(0);
            else PlayBackImageBorder.CornerRadius = new CornerRadius(10);
        }

        private void HiddenButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.MusicDetailFrame.Visibility = Visibility.Hidden;
        }

        private void ImagePlayNeedle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ParentWindow.PlayMusicButton_Click(new object(), new RoutedEventArgs());
        }
    }
}
