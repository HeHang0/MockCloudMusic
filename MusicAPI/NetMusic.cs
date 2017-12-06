using System.ComponentModel;

namespace MusicCollection.MusicAPI
{
    public class NetMusic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 音乐标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 歌手
        /// </summary>
        public string Singer { get; set; }
        /// <summary>
        /// 音乐ID
        /// </summary>
        public string Album { get; set; }
        public string MusicID { get; set; }
        /// <summary>
        /// 音乐来源
        /// </summary>
        public NetMusicType Origin { get; set; }
        /// <summary>
        /// 是否已下载
        /// </summary>
        private bool _isDownloaded = false;
        public bool IsDownLoaded
        {
            get
            {
                return _isDownloaded;
            }
            set
            {
                _isDownloaded = value;
                OnPropertyChanged("IsDownLoaded");
            }
        }
        /// <summary>
        /// 正在下载
        /// </summary>
        private bool _isDownLoading = false;
        public bool IsDownLoading
        {
            get
            {
                return _isDownLoading;
            }
            set
            {
                _isDownLoading = value;
                OnPropertyChanged("IsDownLoading");
            }
        }


        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
