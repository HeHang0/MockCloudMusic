using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    public class MusicObservableCollection: ObservableCollection<Music>
    {
        public new int Add(Music music)
        {
            int index = -1;
            if (IsExist(music, out index))
            {
                return index;
            }
            else
            {
                base.Add(music);
                index = this.Count - 1;
            }
            
            return index;
        }

        private bool IsExist(Music music, out int index)
        {

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Url == music.Url)
                {
                    index = i;
                    return true;
                }
            }
            index = -1;
            return false;
        }
    }
}
