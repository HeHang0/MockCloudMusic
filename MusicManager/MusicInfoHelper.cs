using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace MusicCollection.MusicManager
{
    public class MusicInfoHelper
    {
        //public static Dictionary<MusicInfos, string> GetInfo(string path)
        //{
        //    string tags = string.Empty;
        //    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        //    byte[] buffer = new byte[10];
        //    string mp3ID = "";

        //    fs.Seek(0, SeekOrigin.Begin);
        //    fs.Read(buffer, 0, 10);
        //    int size = (buffer[6] & 0x7F) * 0x200000 + (buffer[7] & 0x7F) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9] & 0x7F);
        //    mp3ID = Encoding.Default.GetString(buffer, 0, 3);
        //    if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
        //    {
        //        //如果有扩展标签头就跨过 10个字节
        //        if ((buffer[5] & 0x40) == 0x40)
        //        {
        //            fs.Seek(10, SeekOrigin.Current);
        //            size -= 10;
        //        }
        //        //tags = ReadFrame(fs, size);
        //    }
        //    var Info = GetOthersInfo(fs);
        //    fs.Close();
        //    Info.Add(MusicInfos.AlbumImageUrl, tags);
        //    return Info;
        //}
        public static Dictionary<MusicInfos, string> ReadMp3(string path)
        {
            Dictionary<MusicInfos, string> tags = new Dictionary<MusicInfos, string>();
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[10];
            // fs.Read(buffer, 0, 128);
            string mp3ID = "";

            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer, 0, 10);
            int size = (buffer[6] & 0x7F) * 0x200000 + (buffer[7] & 0x7F) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9] & 0x7F);
            //int size = (buffer[6] & 0x7F) * 0X200000 * (buffer[7] & 0x7f) * 0x400 + (buffer[8] & 0x7F) * 0x80 + (buffer[9]);
            mp3ID = Encoding.Default.GetString(buffer, 0, 3);
            if (mp3ID.Equals("ID3", StringComparison.OrdinalIgnoreCase))
            {
                //如果有扩展标签头就跨过 10个字节
                if ((buffer[5] & 0x40) == 0x40)
                {
                    fs.Seek(10, SeekOrigin.Current);
                    size -= 10;
                }
                tags = ReadFrame(fs, size);
            }
            var length = fs.Length * 1.0/1024;
            if (length < 1000)
            {
                tags.Add(MusicInfos.Size, Math.Round(length,2) + "KB");
            }
            else
            {
                tags.Add(MusicInfos.Size, Math.Round(length/1024, 1) + "MB");
            }
            return tags;
        }
        public static Dictionary<MusicInfos, string> ReadFrame(FileStream fs, int size, string imageUrl = "")
        {
            var dic = new Dictionary<MusicInfos, string>();
            string[] ID3V2 = new string[6];
            byte[] buffer = new byte[10];
            while (size > 0)
            {
                //fs.Read(buffer, 0, 1);
                //if (buffer[0] == 0)
                //{
                //    size--;
                //    continue;
                //}
                //fs.Seek(-1, SeekOrigin.Current);
                //size++;
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
                string str = GetFrameInfoByEcoding(bFrame, bFrame[0], frmSize - 1);
                if (FramID.CompareTo("TIT2") == 0)
                {
                    dic.Add(MusicInfos.Title, str);
                }
                else if (FramID.CompareTo("TPE1") == 0)
                {
                    dic.Add(MusicInfos.Singer, str);
                }
                else if (FramID.CompareTo("TALB") == 0)
                {
                    dic.Add(MusicInfos.Album, str);
                }
                else if (FramID.CompareTo("APIC") == 0)
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
                    Image img = Image.FromStream(ms);
                    if (Directory.Exists(imageUrl + "AlbumImage\\") == false)//如果不存在就创建file文件夹
                    {
                        Directory.CreateDirectory(imageUrl + "AlbumImage\\");
                    }
                    var url = imageUrl + "AlbumImage\\" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random(DateTime.Now.Millisecond).Next());
                    FileStream save = new FileStream(url, FileMode.Create);
                    img.Save(save, System.Drawing.Imaging.ImageFormat.Png);
                    dic.Add(MusicInfos.AlbumImageUrl, url);
                }


            }
            return dic;
        }
        public static string GetFrameInfoByEcoding(byte[] b, byte conde, int length)
        {
            string str = "";
            switch (conde)
            {
                case 0:
                    str = Encoding.GetEncoding("ISO-8859-1").GetString(b, 1, length);
                    break;
                case 1:
                    str = Encoding.GetEncoding("UTF-16LE").GetString(b, 1, length);
                    break;
                case 2:
                    str = Encoding.GetEncoding("UTF-16BE").GetString(b, 1, length);
                    break;
                case 3:
                    str = Encoding.UTF8.GetString(b, 1, length);
                    break;
            }
            return str;
        }

        //private static Dictionary<MusicInfos, string> GetOthersInfo(FileStream fs)
        //{
        //    byte[] b = new byte[128];
        //    string title = string.Empty;
        //    string singer = string.Empty;
        //    string album = string.Empty;
        //    string year = string.Empty;
        //    string comm = string.Empty;
        //    string length = string.Empty;
        //    //FileStream fs = new FileStream(path, FileMode.Open);
        //    fs.Seek(-128, SeekOrigin.End); //查找
        //    fs.Read(b, 0, 128); //读取

        //    string sFlag = Encoding.Default.GetString(b, 0, 3);
        //    if (sFlag.Equals("TAG"))
        //    {
        //        title = Encoding.Default.GetString(b, 3, 30).Trim(' ');

        //        singer = Encoding.Default.GetString(b, 33, 30).Trim(' ');

        //        album = Encoding.Default.GetString(b, 63, 30).Trim(' ');

        //        year = Encoding.Default.GetString(b, 93, 4).Trim(' ');

        //        comm = Encoding.Default.GetString(b, 97, 30).Trim(' ');

        //        if (fs.Length * 1.0 / 1024 < 1024)
        //        {
        //            length = Math.Round(fs.Length * 1.0 / 1024, 0) + "KB";
        //        }
        //        else
        //        {
        //            length = Math.Round(fs.Length * 1.0 / (1024 * 1024), 2) + "MB";
        //        }
        //        //Console.WriteLine("Comm:" + comm);
        //    }
        //    fs.Close();
        //    Dictionary<MusicInfos, string> InfoDic = new Dictionary<MusicInfos, string>();


        //    return InfoDic;
        //}
        public enum MusicInfos { Title, Singer, Album, Duration, Year, Size, AlbumImageUrl };
    }
}
