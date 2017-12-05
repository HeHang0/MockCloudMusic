namespace MusicCollection.MusicAPI
{
    public class NetMusic
    {
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
    }
}
