using System.ComponentModel;

namespace MusicCollection.MusicAPI
{
    public class NetMusic : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public NetMusic()
        {
        }
        public NetMusic(NetMusic music)
        {
            Title = music.Title;
            Singer = music.Singer;
            Album = music.Album;
            AlbumImageUrl = music.AlbumImageUrl;
            MusicID = music.MusicID;
            Origin = music.Origin;
            LyricPath = music.LyricPath;
            Url = music.Url;
            Remark = music.Remark;
            IsDownLoaded = music.IsDownLoaded;
            IsDownLoading = music.IsDownLoading;
        }


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
        public string AlbumImageUrl { get; set; }
        public string MusicID { get; set; }
        /// <summary>
        /// 音乐来源
        /// </summary>
        public NetMusicType Origin { get; set; }

        private string lyricPath;
        public string LyricPath
        {
            get
            {
                return lyricPath;
            }
            set
            {
                lyricPath = value;
            }
        }
        private string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                OnPropertyChanged("Url");
            }
        }
        private string remark;
        public string Remark
        {
            get
            {
                return remark;
            }
            set
            {
                remark = value;
            }
        }

        /// <summary>
        /// 是否已下载
        /// </summary>
        private bool isDownloaded = false;
        public bool IsDownLoaded
        {
            get
            {
                return isDownloaded;
            }
            set
            {
                isDownloaded = value;
                OnPropertyChanged("IsDownLoaded");
            }
        }
        /// <summary>
        /// 正在下载
        /// </summary>
        private bool isDownLoading = false;
        public bool IsDownLoading
        {
            get
            {
                return isDownLoading;
            }
            set
            {
                isDownLoading = value;
                OnPropertyChanged("IsDownLoading");
            }
        }


        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
