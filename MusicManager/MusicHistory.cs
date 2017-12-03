using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    public class MusicHistory:Music
    {
        public MusicHistory()
        {
        }
        public MusicHistory(Music music) : base(music)
        {
            LastPlayTime = DateTime.Now;
        }

        public DateTime LastPlayTime { get; set; } = DateTime.Now;
        public string LastPlayTimeDescribe { get; set; }
    }
}
