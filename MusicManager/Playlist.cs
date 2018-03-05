using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    class Playlist
    {
        public Playlist()
        {
        }
        public Playlist(string name, string imgUrl, string url)
        {
            Name = name;
            ImgUrl = imgUrl;
            Url = url;
        }

        public string Name { get; set; }
        public string Url { get; set; }
        public string ImgUrl { get; set; }
    }
}
