using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MusicCollection.ChildWindows
{
    /// <summary>
    /// InputStringWindow.xaml 的交互逻辑
    /// </summary>
    public partial class InputStringWindow : Window
    {
        public string InputString = string.Empty;
        
        public InputStringWindow(string title = "", string subtitle = "")
        {
            InitializeComponent();
            WindowTitle.Text = title;
            SubTitle.Content = subtitle;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            InputString = InputStringTextBox.Text;
            DialogResult = true;
        }

        private void DragWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
