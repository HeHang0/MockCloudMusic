using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MusicCollection.Setting
{
    public class Utils
    {
        private const int IMAGE_MAX_SIZE = 1024 * 1024;

        public static string Md5Func(string source)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = string.Empty;
            for (int i = 0; i < md5Data.Length; i++)
            {
                //返回一个新字符串，该字符串通过在此实例中的字符左侧填充指定的 Unicode 字符来达到指定的总长度，从而使这些字符右对齐。
                // string num=12; num.PadLeft(4, '0'); 结果为为 '0012' 看字符串长度是否满足4位,不满足则在字符串左边以"0"补足
                //调用Convert.ToString(整型,进制数) 来转换为想要的进制数
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            //使用 PadLeft 和 PadRight 进行轻松地补位
            destString = destString.PadLeft(32, '0');
            return destString.ToLower();
        }

        public static void CheckImageFileScale(string fileName)
        {
            try
            {
                FileInfo fi = new FileInfo(fileName);
                if (!fi.Exists || fi.Length < IMAGE_MAX_SIZE) return;
                Bitmap bmp = FileToBitmap(fileName);
                int ratio = (int)(fi.Length / IMAGE_MAX_SIZE);
                Bitmap thumbnial = GetThumbnail(bmp, 2 * bmp.Height / ratio, 2 * bmp.Width / ratio);
                thumbnial.Save(fileName);
            }
            catch (System.Exception)
            {
            }
        }

        public static Bitmap FileToBitmap(string fileName)
        {
            // 打开文件
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 转换成 Stream
            Stream stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }

        public static Bitmap GetThumbnail(Bitmap imgSource, int destHeight, int destWidth = 0)
        {
            ImageFormat thisFormat = imgSource.RawFormat;
            // 按比例缩放           
            int sWidth = imgSource.Width;
            int sHeight = imgSource.Height;
            int sW;
            int sH;
            if (sHeight > destHeight || sWidth > destWidth)
            {
                if ((sWidth * destHeight) > (sHeight * destWidth))
                {
                    sW = destWidth;
                    sH = destWidth * sHeight / sWidth;
                }
                else
                {
                    sH = destHeight;
                    sW = sWidth * destHeight / sHeight;
                }
            }
            else
            {
                sW = sWidth;
                sH = sHeight;
            }
            Bitmap outBmp = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage(outBmp);
            g.Clear(System.Drawing.Color.Transparent);
            // 设置画布的描绘质量         
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgSource, new Rectangle((destWidth - sW) / 2, (destHeight - sH) / 2, sW, sH), 0, 0, imgSource.Width, imgSource.Height, GraphicsUnit.Pixel);
            g.Dispose();
            // 以下代码为保存图片时，设置压缩质量     
            EncoderParameters encoderParams = new EncoderParameters();
            long[] quality = new long[1];
            quality[0] = 100;
            EncoderParameter encoderParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            encoderParams.Param[0] = encoderParam;
            imgSource.Dispose();
            return outBmp;
        }

        public static Bitmap GaussianBlur(Bitmap bmp, int radius = 10)
        {
            Mat src;
            using (src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmp))
            using (Mat dst = new Mat())
            {
                //注意：size 参数一定要是奇数 (均值模糊)  Y 轴模糊
                Cv2.Blur(src, dst, new OpenCvSharp.Size(radius, radius), new OpenCvSharp.Point(-1, -1));
                return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
            }
        }

        public static ImageSource GaussianBlur(string imagePath, int radius = 10)
        {
            if (!File.Exists(imagePath)) return null;
            try
            {
                using (Mat src = OpenCvSharp.Cv2.ImRead(imagePath))
                using (Mat dst = new Mat())
                {
                    //注意：size 参数一定要是奇数 (均值模糊)  Y 轴模糊
                    Cv2.Blur(src, dst, new OpenCvSharp.Size(radius, radius), new OpenCvSharp.Point(-1, -1));
                    var gs = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(dst);
                    var imgs = BitmapToImageSource(gs);
                    return imgs;
                }
                //start = DateTime.Now;
                //var gs = GaussianBlur(bmp, radius);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ImageSource GaussianBlur(ImageSource imageSource, int radius = 10)
        {
            var bmp = ImageSourceToBitmap(imageSource);
            var gs = GaussianBlur(bmp, radius);
            var imgs = BitmapToImageSource(gs);
            return imgs;
        }

        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            Bitmap bmp = new Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // 坑点：选Format32bppRgb将不带透明度

            BitmapData data = bmp.LockBits(
            new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        public static ImageSource BitmapToImageSource(Bitmap bitmap)

        {

            //Bitmap bitmap = icon.ToBitmap();

            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(

               hBitmap,

                IntPtr.Zero,

                Int32Rect.Empty,

                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))

            {

                throw new System.ComponentModel.Win32Exception();

            }

            return wpfBitmap;

        }
    }
}
