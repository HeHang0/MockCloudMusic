using MusicCollection.MusicAPI;
using System.Collections.ObjectModel;

namespace MusicCollection.MusicManager
{
    public class NetMusicObservableCollection<T> : ObservableCollection<T>
    {
        public new int Add(T item)
        {
            int index = -1;
            var music = item as NetMusic;
            if (music != null && IsExist(music, out index))
            {
                return index;
            }
            else
            {
                base.Add(item);
                index = Count - 1;
            }

            return index;
        }

        private bool IsExist(NetMusic music, out int index)
        {
            var list = this as NetMusicObservableCollection<NetMusic>;
            if (list != null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (list[i].Url == music.Url)
                    {
                        index = i;
                        return true;
                    }
                }
            }
            index = -1;
            return false;
        }
    }
}
