using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MusicCollection.Pages
{
    /// <summary>
    /// LocalMusicPage.xaml 的交互逻辑
    /// </summary>
    public partial class LocalMusicPage : Page
    {
        private List<string> FolderList = new List<string>();
        public MainWindow ParentWindow { get; private set; }
        public LocalMusicPage(MainWindow ParentWindow)
        {
            this.ParentWindow = ParentWindow;
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            var configPath = "LocalMusicFolderList.json";
            var content = "";
            try
            {
                content = System.IO.File.ReadAllText(configPath);
            }
            catch (System.Exception)
            {
                
            }
            

            List<string> list = new List<string> { "asdasda", "阿德飒飒啊大苏打", "32322a3s2d13" };
            var jsonList = JsonConvert.SerializeObject(list);
            File.WriteAllText("LocalMusicFolderList.json", jsonList);

            var str = File.ReadAllText("LocalMusicFolderList.json");
            List<string> newList = JsonConvert.DeserializeObject<List<string>>(str);
        }

        private void Page_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            LocalMusicDataGrid.Height = Height;
            LocalMusicDataGrid.Width = Width;
            GridSplitter1.Width = Width;
            GridSplitter2.Width = Width;
        }
        
        private void RefreshLocalListButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
