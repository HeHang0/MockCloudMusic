using MusicCollection.MusicAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    class Playlist
    {
        public static readonly string MyDailyRecommand = "今日推荐";

        public Playlist()
        {
        }
        public Playlist(string name, string imgUrl, string url)
        {
            Name = name;
            ImgUrl = imgUrl;
            Url = url;
        }

        private string _name;
        public string Name
        {
            set
            {
                _name = EncodingHelper.XmlDecode(value);
            }
            get
            {
                return _name;
            }
        }
        public string Url { get; set; }
        public string ImgUrl { get; set; }
        public string LocalImgUrl
        {
            get
            {
                return NetMusicHelper.GetImgFromRemote(ImgUrl);
            }
        }

        public bool IsMyDailyRecommand
        {
            get
            {
                return Name == MyDailyRecommand;
            }
        }

        public bool IdCommonPlaylist
        {
            get
            {
                return Name != MyDailyRecommand;
            }
        }

        public string DayOfWeek
        {
            get
            {
                return "星期" + DayOfWeekCN[(int)DateTime.Now.DayOfWeek];
            }
        }

        public int DayOfMonth
        {
            get
            {
                return DateTime.Now.Day;
            }
        }

        private static string[] DayOfWeekCN = new string[] { "天", "一", "二", "三", "四", "五", "六" };
    }
}
