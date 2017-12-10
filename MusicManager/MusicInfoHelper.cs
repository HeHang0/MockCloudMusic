using Shell32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace MusicCollection.MusicManager
{
    public class MusicInfoHelper
    {
        public static Dictionary<MusicInfos, string> GetInfo(string path)
        {
            var Info = new Dictionary<MusicInfos, string>();
            ShellClass sh = new ShellClass();
            Folder dir = sh.NameSpace(Path.GetDirectoryName(path));
            FolderItem item = dir.ParseName(Path.GetFileName(path));
            Info.Add(MusicInfos.Title, dir.GetDetailsOf(item, 21));
            Info.Add(MusicInfos.Singer, dir.GetDetailsOf(item, 13));
            Info.Add(MusicInfos.Album, dir.GetDetailsOf(item, 14));
            Info.Add(MusicInfos.Size, dir.GetDetailsOf(item, 1));
            Info.Add(MusicInfos.BitRate, dir.GetDetailsOf(item, 28));
            Info.Add(MusicInfos.Duration, dir.GetDetailsOf(item, 27));
            Info.Add(MusicInfos.AlbumImageUrl, GetImage(path));

            if (string.IsNullOrWhiteSpace(Info[MusicInfos.Singer]))
            {
                Info[MusicInfos.Singer] = dir.GetDetailsOf(item, 232);
            }
            if (string.IsNullOrWhiteSpace(Info[MusicInfos.Title]))
            {
                Info[MusicInfos.Title] = Path.GetFileNameWithoutExtension(path);
            }
            else if (!Path.GetFileNameWithoutExtension(path).Contains(Info[MusicInfos.Title]))
            {
                Info[MusicInfos.Title] = Path.GetFileNameWithoutExtension(path);
            }

            var virtualLrc = Path.GetDirectoryName(path) + "\\" + Path.GetFileNameWithoutExtension(path) + ".lrc";
            var virtualLrc2 = "DownLoad\\Lyric\\" + Path.GetFileNameWithoutExtension(path) + ".lrc";
            if (File.Exists(virtualLrc))
            {
                Info.Add(MusicInfos.LyricUrl, virtualLrc);
            }
            else if(File.Exists(virtualLrc2))
            {
                Info.Add(MusicInfos.LyricUrl, Path.GetFullPath(virtualLrc2));
            }
            else
            {
                Info.Add(MusicInfos.LyricUrl, string.Empty);
            }
            return Info;
        }
        private static string GetImage(string path)
        {
            var mp3 = path;
            path = path.GetHashCode().ToString();
            if (!Directory.Exists("AlbumImage\\"))//如果不存在就创建文件夹
            {
                Directory.CreateDirectory("AlbumImage\\");
            }
            var url = "AlbumImage\\" + path;
            if (!File.Exists(url))
            {
                TagLib.File file = TagLib.File.Create(mp3);
                if (file.Tag.Pictures.Count() > 0)
                {
                    try
                    {
                        MemoryStream stream = new MemoryStream(file.Tag.Pictures[0].Data.Data);
                        Image img = Image.FromStream(stream);
                        img.Save(url, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    catch (Exception)
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }                
            }            
            return System.Windows.Forms.Application.StartupPath + "\\" + url; ;
        }
        private static string ReadMp3(string path)
        {
            var url = "AlbumImage\\" + path.GetHashCode().ToString();
            if (File.Exists(url))
            {
                return System.Windows.Forms.Application.StartupPath + "\\" + url;
            }
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[10];
            string mp3ID = "";
            string tags = "";
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer, 0, 10);
            int size = (buffer[6] & 0x7F) * 0x200000 + (buffer[7] & 0x7F) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9] & 0x7F);
            mp3ID = Encoding.Default.GetString(buffer, 0, 3);
            if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
            {
                //如果有扩展标签头就跨过 10个字节
                if ((buffer[5] & 0x40) == 0x40)
                {
                    fs.Seek(10, SeekOrigin.Current);
                    size -= 10;
                }
                tags = ReadFrame(fs, size, path);
            }
            fs.Close();
            return tags;
        }
        private static string ReadFrame(FileStream fs, int size,string path)
        {
            byte[] buffer = new byte[10];
            while (size > 0)
            {

                //读取标签帧头的10个字节
                fs.Read(buffer, 0, 10);
                size -= 10;
                //得到标签帧ID
                string FramID = Encoding.Default.GetString(buffer, 0, 4);
                //计算标签帧大小，第一个字节代表帧的编码方式
                int frmSize = 0;

                frmSize = buffer[4] * 0x1000000 + buffer[5] * 0x10000 + buffer[6] * 0x100 + buffer[7];
                if (frmSize == 0)
                {
                    //就说明真的没有信息了
                    break;
                }
                //bFrame 用来保存帧的信息
                byte[] bFrame = new byte[frmSize];
                fs.Read(bFrame, 0, frmSize);
                size -= frmSize;
                if (FramID.CompareTo("APIC") == 0)
                {
                    int i = 0;
                    while (true)
                    {

                        if (255 == bFrame[i] && 216 == bFrame[i + 1])
                        {
                            //在
                            break;

                        }
                        i++;
                    }

                    byte[] imge = new byte[frmSize - i];
                    fs.Seek(-frmSize + i, SeekOrigin.Current);
                    fs.Read(imge, 0, imge.Length);

                    MemoryStream ms = new MemoryStream(imge);
                    Image img = Image.FromStream(ms, true);
                    if (Directory.Exists("AlbumImage\\") == false)//如果不存在就创建文件夹
                    {
                        Directory.CreateDirectory("AlbumImage\\");
                    }
                    path = path.GetHashCode().ToString();
                    var url = "AlbumImage\\" + path;
                    FileStream save = new FileStream(url, FileMode.Create);
                    img.Save(save, System.Drawing.Imaging.ImageFormat.Png);
                    save.Close();
                    return System.Windows.Forms.Application.StartupPath + "\\" + url;
                }


            }
            return "";
        }
        public enum MusicInfos { Title, Singer, Album, Duration, BitRate, Size, AlbumImageUrl, LyricUrl };
    }
}
