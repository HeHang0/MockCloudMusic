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

namespace MusicCollection.ChildWindows
{
    /// <summary>
    /// DesktopLyricWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class DesktopLyricWindow : Window
    {
        public DesktopLyricWindow()
        {
            InitializeComponent();
            TopMostTool.SetTopMost(this);
        }

        public class TopMostTool
        {
            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);

            public static void SetTopMost(DesktopLyricWindow win)
            {
                SetWindowPos(new WindowInteropHelper(win).Handle, -1, 0, 0, 0, 0, 1 | 2);
            }
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Lyric_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                Lyric.FontSize++;
            }
            else
            {
                Lyric.FontSize--;
            }
        }
    }
}
