using MusicCollection.MusicAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    public class DownLoadMusic:Music
    {
        public DownLoadMusic(NetMusic music) : base(music)
        {

        }
        private bool isDownLoaded = false;
        public bool IsDownLoaded
        {
            get
            {
                return isDownLoaded;
            }
            set
            {
                isDownLoaded = value;
                OnPropertyChanged("IsDownLoaded");
            }
        }
        private bool isDownLoading = false;
        public bool IsDownLoading
        {
            get
            {
                return isDownLoading;
            }
            set
            {
                isDownLoading = value;
                OnPropertyChanged("IsDownLoading");
            }
        }
    }
}
