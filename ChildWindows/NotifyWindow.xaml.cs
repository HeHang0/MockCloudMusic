using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicCollection.ChildWondows
{
    /// <summary>
    /// NotifyWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NotifyWindow : Window
    {
        public static int SW_SHOW = 5;
        public static int SW_NORMAL = 1;
        public static int SW_MAX = 3;
        public static int SW_HIDE = 0;
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);    //窗体置顶
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);    //取消窗体置顶
        public const uint SWP_NOMOVE = 0x0002;    //不调整窗体位置
        public const uint SWP_NOSIZE = 0x0001;    //不调整窗体大小
        MainWindow ParentWindow;
        public NotifyWindow(MainWindow parentWindow)
        {
            SetWindowPos(new WindowInteropHelper(this).Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
            ParentWindow = parentWindow;
            InitializeComponent();
            Title = string.Empty;
        }
        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MusicTitle.Content = ParentWindow.Title.Length > 12 ? ParentWindow.Title.Substring(0, 12) + "..." : ParentWindow.Title;
            MusicTitle.ToolTip = ParentWindow.Title;
            Activate();
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            MusicTitle.Content = ParentWindow.Title.Length > 12 ? ParentWindow.Title.Substring(0, 12) + "..." : ParentWindow.Title;
            MusicTitle.ToolTip = ParentWindow.Title;
            Activate();
        }

        private void NotifyButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.PlayMusicButton_Click(new object(), new RoutedEventArgs());
            Hide();
        }

        private void NotifyNextButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.LastMusicPlay();
            Hide();
        }

        private void NotifyLastButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.NextMusicPlay();
            Hide();
        }

        private void NotifyTitleButton(object sender, RoutedEventArgs e)
        {
            ParentWindow.MusicDetailFrame.Visibility = Visibility.Visible;
            Hide();
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Hide();
        }
    }
}
