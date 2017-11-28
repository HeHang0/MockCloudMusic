using Shell32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    public class Music
    {
        public Music()
        {

        }
        public Music(string url)
        {
            ShellClass sh = new ShellClass();
            Folder dir = sh.NameSpace(System.IO.Path.GetDirectoryName(""));
            FolderItem item = dir.ParseName(System.IO.Path.GetFileName(""));
            Title = dir.GetDetailsOf(item, 21);
            //Info[1] = dir.GetDetailsOf(item, 20);
            Info[6] = dir.GetDetailsOf(item, 14);
            Info[3] = dir.GetDetailsOf(item, 27);
            Info[3] = Info[3].Substring(Info[3].IndexOf(":") + 1);
            Info[4] = dir.GetDetailsOf(item, 1);
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
        public double Size { get; set; }
        /// <summary>
        /// 本地路径
        /// </summary>
        public string Url { get; set; }
    }
}
