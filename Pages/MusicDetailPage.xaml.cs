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
        Timer timer = new Timer();
        public MusicDetailPage(MainWindow mainWindow)
        {
            ParentWindow = mainWindow;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (ParentWindow.CurrentIndex >= 0)
            {
                Init(ParentWindow.CurrentMusicList[ParentWindow.CurrentIndex]);
            }
            timer.Enabled = true;
            timer.Interval = 100;//执行间隔时间,单位为毫秒;此时时间间隔为1分钟
            timer.Elapsed += new ElapsedEventHandler(CheckTime);
            timer.Start();
        }

        private int LastLyricLineIndex = -1;
        private int Count = 0;
        private void CheckTime(object sender, ElapsedEventArgs e)
        {
            if (ParentWindow.bsp.IsPlaying)
            {
                if (CurrentLyricIndex > LastLyricLineIndex && Lyric[CurrentLyricIndex].StartTime <= ParentWindow.bsp.CurrentTime)
                {
                    LastLyricLineIndex = CurrentLyricIndex++;
                    if (CurrentLyricIndex == Count)
                    {
                        timer.Stop();
                        CurrentLyricIndex = -1;
                        LastLyricLineIndex = -1;
                    }
                    try
                    {
                        var run = LyricTextBlock.Inlines.ElementAt(LastLyricLineIndex);
                        Dispatcher.Invoke(new Action(() => {
                            run.Foreground = Brushes.Red;
                            LyricBlock.ScrollToVerticalOffset(LyricBlock.ScrollableHeight * LastLyricLineIndex * 1.0 / Count);
                        }));
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public void Init(Music music)
        {
            timer.Stop();
            Lyric.Clear();
            LyricTextBlock.Inlines.Clear();
            AlbumImage.ImageSource = ParentWindow.CurrentMusicImageMini.Source;
            if (music.LyricUrl.Length > 0)
            {
                var lrcEncoding = EncodingHelper.GetType(music.LyricUrl);
                var lrcList = System.IO.File.ReadLines(music.LyricUrl, lrcEncoding);
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
                timer.Start();
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    LyricTextBlock.Inlines.Add("\n");
                }
                LyricTextBlock.Inlines.Add(new Run("               用 心 去 感 受 音 乐\n") { Foreground = Brushes.Red, FontSize = 16 });
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

        internal void StopTimer()
        {
            timer.Stop();
        }

        private void HiddenButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.MusicDetailFrame.Visibility = Visibility.Hidden;
        }
    }
}
