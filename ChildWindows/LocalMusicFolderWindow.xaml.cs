using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicCollection.ChildWondows
{
    /// <summary>
    /// LocalMusicFolderWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LocalMusicFolderWindow : Window
    {
        private MainWindow ParentWindow;
        private Pages.LocalMusicPage ParentPage;
        private ObservableCollection<LocalFolderListViewModel> LocalFolderListView = new ObservableCollection<LocalFolderListViewModel>();

        public LocalMusicFolderWindow(MainWindow mwin, Pages.LocalMusicPage lmpage)
        {
            ParentWindow = mwin;
            ParentPage = lmpage;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Left = ParentWindow.Left + (ParentWindow.Width - Width) / 2;
            Top = ParentWindow.Top + (ParentWindow.Height - Height) / 2;
            
            foreach (var item in ParentPage.FolderList)
            {
                LocalFolderListView.Add(new LocalFolderListViewModel(item));
            }
            LocalMusicFolderListDataGrid.DataContext = LocalFolderListView;
        }

        private void MouseUpHandler(object sender, RoutedEventArgs e)
        {
            if (Left <= ParentWindow.Left)
            {
                Left = ParentWindow.Left;
            }
            if (Left >= ParentWindow.Left + ParentWindow.Width - Width)
            {
                Left = ParentWindow.Left + ParentWindow.Width - Width;
            }
            if (Top <= ParentWindow.Top)
            {
                Top = ParentWindow.Top;
            }
            if (Top >= ParentWindow.Top + ParentWindow.Height - Height)
            {
                Top = ParentWindow.Top + ParentWindow.Height - Height;
            }
        }
        private void MouseDownHandler(object sender, RoutedEventArgs e)
        {
            DragMove();
            MouseUpHandler(MoveBar, e);
        }

        private void DataGridCheckBoxColumn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var a = (sender as DataGridRow);
            var b = a.Item as LocalFolderListViewModel;
            if (!(bool)b.IsChecked)
            {
                b.IsChecked = true;
            }
            else
            {
                b.IsChecked = false;
            }
        }

        private void LocalMusicFolderConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ParentPage.FolderList.Clear();
            foreach (var item in LocalFolderListView)
            {
                if (item.IsChecked)
                {
                    ParentPage.FolderList.Add(item.Path);
                }
            }
            Close();
        }

        private void AddFolderToListButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog m_Dialog = new FolderBrowserDialog();
            DialogResult result = m_Dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }
            string m_Dir = m_Dialog.SelectedPath.Trim() + "\\";
            LocalFolderListView.Add(new LocalFolderListViewModel(m_Dir));
        }        

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        class LocalFolderListViewModel : INotifyPropertyChanged
        {
            public LocalFolderListViewModel(string path)
            {
                IsChecked = true;
                Path = path;
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private bool _isChecked;
            public bool IsChecked
            {
                get
                {
                    return _isChecked;
                }
                set
                {
                    _isChecked = value;
                    NotifyPropertyChanged();
                }
            }

            private string _path;
            public string Path
            {
                get
                {
                    if (Regex.IsMatch(_path, "C:\\\\Users\\\\([^\\\\]+)\\\\Music\\\\"))
                    {
                        return "我的音乐";
                    }
                    else
                    {
                        return _path;
                    }
                }
                set
                {
                    _path = value;
                    NotifyPropertyChanged();
                }
            }

            private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
