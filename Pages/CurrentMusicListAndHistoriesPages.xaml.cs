﻿using MusicCollection.ChildWindows;
using MusicCollection.MusicManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MusicCollection.Pages
{
    /// <summary>
    /// CurrentMusicListAndHistoriesPages.xaml 的交互逻辑
    /// </summary>
    public partial class CurrentMusicListAndHistoriesPages : Page
    {
        private MainWindow ParentWindow;
        public CurrentMusicListAndHistoriesPages(MainWindow mainWindow)
        {
            ParentWindow = mainWindow;
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            CurrentMusicOrHistoriesDataGrid.DataContext = ParentWindow.CurrentMusicList;
            CurrentMusicCountLable.Content = $"总{ParentWindow.CurrentMusicList.Count}首";
            HistoriesMusicCountLable.Content = $"总{ParentWindow.HistoryMusicList.Count}首";
            ParentWindow.CurrentMusicList.CollectionChanged += CurrentMusicList_OnCountChange;
            ParentWindow.HistoryMusicList.CollectionChanged += HistoryMusicList_OnCountChange;
        }
        
        public void MusicChange(int index)
        {
            if(CurrentMusicListButtonHelper.Visibility == Visibility.Visible)
            {
                CurrentMusicOrHistoriesDataGrid.SelectedIndex = index;
            }
        }

        private void UpdateHistoriesTimeDescribe()
        {
            foreach (var item in ParentWindow.HistoryMusicList)
            {
                var tts = (item.LastPlayTime - new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day)).TotalHours;
                var ts = (DateTime.Now - item.LastPlayTime);
                var tm = (int)ts.TotalMinutes;
                var th = (int)ts.TotalHours;
                var td = (int)ts.TotalDays;
                var describe = "";
                if (tm <= 1)
                {
                    describe = "刚刚";
                }
                else if (tm < 60 && tts > 0)
                {
                    describe = tm + "分钟前";
                }
                else if (th < 24 && tts > 0)
                {
                    describe = th + "小时前";
                }
                else if (tts < 24 )
                {
                    describe = "昨天";
                }
                else
                {
                    describe = item.LastPlayTime.ToString("yyyy-MM-dd");
                }
                item.LastPlayTimeDescribe = describe;
            }
        }

        private void HistoryMusicList_OnCountChange(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            HistoriesMusicCountLable.Content = $"总{ParentWindow.HistoryMusicList.Count}首";
            UpdateHistoriesTimeDescribe();
        }

        private void CurrentMusicList_OnCountChange(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CurrentMusicCountLable.Content = $"总{ParentWindow.CurrentMusicList.Count}首";
        }

        private void CurrentMusicDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGridRow dgr = sender as DataGridRow;
                var music = dgr.Item as Music;
                if (music != null)
                {
                    ParentWindow.CurrentIndex = ParentWindow.CurrentMusicList.Add(music);
                    ParentWindow.bsp.Stop();
                    ParentWindow.Play(music);
                }
                e.Handled = true;
            }
        }

        private void CollectionCurrentMusicListButton_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            if (ParentWindow.GetStringFromInputStringWindow("添加歌单", "歌单名称", out name))
            {
                if (!string.IsNullOrWhiteSpace(name))
                {
                    ParentWindow.PlayListCollection.Add(new PlayListCollectionModel(name, "", ParentWindow.CurrentMusicList));
                }
                else
                {
                    MessageBox.Show("名称不能为空");
                }
            }
        }

        private void ClearCurrentMusicListButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentMusicListButtonHelper.Visibility == Visibility.Visible)
            {
                ParentWindow.CurrentMusicList.Clear();
            }
            else if (HistoryMusicListButtonHelper.Visibility == Visibility.Visible)
            {
                ParentWindow.HistoryMusicList.Clear();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            ParentWindow.CurrentMusicListFrame.Visibility = Visibility.Hidden;
        }

        private void CurrentMusicListButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentMusicOrHistoriesDataGrid.DataContext = ParentWindow.CurrentMusicList;
            CurrentMusicOrHistoriesDataGrid.Columns[2].Visibility = Visibility.Visible;
            CurrentMusicOrHistoriesDataGrid.Columns[3].Visibility = Visibility.Hidden;
            CurrentMusicListButton.Visibility = Visibility.Hidden;
            CurrentMusicListButtonHelper.Visibility = Visibility.Visible;
            HistoryMusicListButton.Visibility = Visibility.Visible;
            HistoryMusicListButtonHelper.Visibility = Visibility.Hidden;
            CurrentMusicCountLable.Visibility = Visibility.Visible;
            HistoriesMusicCountLable.Visibility = Visibility.Hidden;
        }

        private void HistoryMusicListButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateHistoriesTimeDescribe();
            CurrentMusicOrHistoriesDataGrid.DataContext = ParentWindow.HistoryMusicList;

            //ICollectionView view = CollectionViewSource.GetDefaultView(CurrentMusicOrHistoriesDataGrid.ItemsSource);
            //view.SortDescriptions.Clear();
            //SortDescription sd = new SortDescription("LastPlayTime", ListSortDirection.Descending);
            //view.SortDescriptions.Add(sd);

            CurrentMusicOrHistoriesDataGrid.Columns[2].Visibility = Visibility.Hidden;
            CurrentMusicOrHistoriesDataGrid.Columns[3].Visibility = Visibility.Visible;
            CurrentMusicListButton.Visibility = Visibility.Visible;
            CurrentMusicListButtonHelper.Visibility = Visibility.Hidden;
            HistoryMusicListButton.Visibility = Visibility.Hidden;
            HistoryMusicListButtonHelper.Visibility = Visibility.Visible;
            CurrentMusicCountLable.Visibility = Visibility.Hidden;
            HistoriesMusicCountLable.Visibility = Visibility.Visible;
        }
    }
}
