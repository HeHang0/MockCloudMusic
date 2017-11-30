using System;

namespace MusicCollection.MusicManager
{
    public class Music
    {
        public Music()
        {
           
        }
        public Music(string url)
        {
            var info = MusicInfoHelper.GetInfo(url);

            Title = info[MusicInfoHelper.MusicInfos.Title];
            Singer = info[MusicInfoHelper.MusicInfos.Singer];
            Album = info[MusicInfoHelper.MusicInfos.Album];
            Size = info[MusicInfoHelper.MusicInfos.Size];
            BitRate = info[MusicInfoHelper.MusicInfos.BitRate];
            AlbumImageUrl = info[MusicInfoHelper.MusicInfos.AlbumImageUrl];
            var time = new TimeSpan();
            TimeSpan.TryParse(info[MusicInfoHelper.MusicInfos.Duration], out time);
            Duration = time;
            Url = url;
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
        public TimeSpan Duration { get; set; }
        
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
        public string Url { get; set; }
        /// <summary>
        /// 专辑图
        /// </summary>
        public string AlbumImageUrl { get; set; }
    }
}
