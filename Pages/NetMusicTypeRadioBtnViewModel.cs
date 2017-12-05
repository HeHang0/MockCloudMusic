using MusicCollection.MusicAPI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicCollection.Pages
{
     class NetMusicTypeRadioBtnViewModel
    {
        public ObservableCollection<NetMusicTypeRdBtn> Insertions { get; set; }

        public NetMusicTypeRadioBtnViewModel()
        {
            Insertions = new ObservableCollection<NetMusicTypeRdBtn>();
            Insertions.Add(new NetMusicTypeRdBtn() { Text = "网易云音乐", Tag = NetMusicType.CloudMusic, IsChecked = true });
            //Insertions.Add(new NetMusicTypeRdBtn() { Text = "QQ音乐", Tag = NetMusicType.QQMusic });
        }
        public NetMusicType SelectItem()
        {
            foreach (var item in Insertions)
            {
                if (item.IsChecked)
                {
                    return item.Tag;
                }
            }
            return NetMusicType.CloudMusic;
        }
    }
    class NetMusicTypeRdBtn
    {
        public string Text { get; set; }
        public NetMusicType Tag { get; set; }
        public bool IsChecked { get; set; }
    }
}
