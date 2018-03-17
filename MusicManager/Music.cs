using System;
using MusicCollection.MusicAPI;
using System.ComponentModel;

namespace MusicCollection.MusicManager
{
    public class Music : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Music()
        {

        }
        public Music(Music music)
        {
            Title = music.Title;
            Singer = music.Singer;
            Album = music.Album;
            Size = music.Size;
            BitRate = music.BitRate;
            AlbumImageUrl = music.AlbumImageUrl;
            Duration = music.Duration;
            Path = music.Path;
            LyricPath = music.LyricPath;
            MusicID = music.MusicID;
            Origin = music.Origin;
        }
        public Music(NetMusic music)
        {
            Title = music.Title;
            Singer = music.Singer;
            Album = music.Album;
            AlbumImageUrl = music.AlbumImageUrl;
            Path = music.Url;
            LyricPath = music.LyricPath;
            Duration = music.Duration;
            Origin = music.Origin;
            MusicID = music.MusicID;
        }

        public Music(string path)
        {
            var info = MusicInfoHelper.GetInfo(path);

            Title = info[MusicInfoHelper.MusicInfos.Title];
            Singer = info[MusicInfoHelper.MusicInfos.Singer];
            Album = info[MusicInfoHelper.MusicInfos.Album];
            Size = info[MusicInfoHelper.MusicInfos.Size];
            BitRate = info[MusicInfoHelper.MusicInfos.BitRate];
            AlbumImageUrl = info[MusicInfoHelper.MusicInfos.AlbumImageUrl];
            LyricPath = info[MusicInfoHelper.MusicInfos.LyricUrl];
            var time = new TimeSpan();
            TimeSpan.TryParse(info[MusicInfoHelper.MusicInfos.Duration], out time);
            Duration = time;
            Path = path;
        }

        public Music(Music music, NetMusic net_music) : this(music)
        {
            Title = net_music.Title;
            Singer = net_music.Singer;
            Album = net_music.Album;
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
        /// 专辑
        /// </summary>
        public string Album { get; set; }

        /// <summary>
        /// 音乐时长
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        
        /// <summary>
        /// 音乐大小
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// 比特率
        /// </summary>
        public string BitRate { get; set; }

        /// <summary>
        /// 本地路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 专辑图
        /// </summary>
        public string AlbumImageUrl { get; set; }
        /// <summary>
        /// 歌词路径
        /// </summary>
        public string LyricPath { get; set; } = string.Empty;
        /// <summary>
        /// 歌曲来源
        /// </summary>
        public NetMusicType Origin { get; set; } = NetMusicType.LocalMusic;
        /// <summary>
        /// 音乐ID
        /// </summary>
        public string MusicID { get; set; }
        /// <summary>
        /// 音乐是否可播放
        /// </summary>
        private bool isDisable = false;
        public bool IsDisable {
            get
            {
                return isDisable;
            }
            set
            {
                isDisable = value;
                OnPropertyChanged("IsDisable");
            }
        }
    }
}
