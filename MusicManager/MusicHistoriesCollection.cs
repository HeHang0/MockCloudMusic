﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    public class MusicHistoriesCollection<T> : MusicObservableCollection<T>
    {
        public new void Add(T item)
        {
            int index = -1;
            var music = item as MusicHistory;
            if (IsExist(music, out index))
            {
                RemoveAt(index);
            }
            base.Insert(0, item);
        }

        private bool IsExist(MusicHistory music, out int index)
        {
            var list = this as MusicHistoriesCollection<MusicHistory>;
            if (list != null)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (list[i].Path == music.Path)
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
