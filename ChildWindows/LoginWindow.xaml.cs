using MusicCollection.MusicAPI;
using MusicCollection.MusicManager;
using MusicCollection.Setting;
using QRCoder;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MusicCollection.ChildWindows
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Thread thread2 = new Thread(new ThreadStart(CheckAllLogin));
            thread2.IsBackground = true;
            thread2.Start();
        }

        private void CheckAllLogin()
        {
            var miguUser = NetMusicHelper.GetAccountFromMigu(NetMusicHelper.MiguMusicU);
            SetMiguMusicUserInfo(miguUser);
            var qqUser = NetMusicHelper.GetAccountFromQQMusic(NetMusicHelper.QQMusicU);
            SetQQMusicUserInfo(qqUser);
            var cloudUser = NetMusicHelper.GetAccountFromCloudMusic();
            SetCloudMusicUserInfo(cloudUser);
        }

        private void SetMiguMusicUserInfo(User miguUser)
        {
            if (miguUser == null)
            {
                Dispatcher.Invoke(() =>
                {
                    MiguMusicNoLogin.Visibility = Visibility.Visible;
                    MiguMusicLogin.Visibility = Visibility.Collapsed;
                    MiguLoginCookie.Text = string.Empty;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    MiguMusicName.Text = miguUser.Name;
                    MiguMusicImage.DataContext = string.IsNullOrWhiteSpace(miguUser.Image) ? "logo.ico" : NetMusicHelper.GetImgFromRemote(miguUser.Image);
                    MiguMusicNoLogin.Visibility = Visibility.Collapsed;
                    MiguMusicLogin.Visibility = Visibility.Visible;
                    MiguLoginCookie.Text = NetMusicHelper.MiguMusicU;
                });
            }
        }

        private void SetQQMusicUserInfo(User qqUser)
        {
            if (qqUser == null)
            {
                Dispatcher.Invoke(() =>
                {
                    QQMusicNoLogin.Visibility = Visibility.Visible;
                    QQMusicLogin.Visibility = Visibility.Collapsed;
                    QQLoginCookie.Text = string.Empty;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    QQMusicName.Text = qqUser.Name;
                    QQMusicImage.DataContext = string.IsNullOrWhiteSpace(qqUser.Image) ? "logo.ico" : NetMusicHelper.GetImgFromRemote(qqUser.Image);
                    QQMusicNoLogin.Visibility = Visibility.Collapsed;
                    QQMusicLogin.Visibility = Visibility.Visible;
                    QQLoginCookie.Text = NetMusicHelper.QQMusicU;
                });
            }
        }

        private void SetCloudMusicUserInfo(User cloudUser)
        {
            if(cloudUser == null)
            {
                Dispatcher.Invoke(() =>
                {
                    CloudMusicNoLogin.Visibility = Visibility.Visible;
                    CloudMusicLogin.Visibility = Visibility.Collapsed;
                });
                ShowQRCode();
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    CloudMusicName.Text = cloudUser.Name;
                    CloudMusicImage.DataContext = string.IsNullOrWhiteSpace(cloudUser.Image) ? "logo.ico" : NetMusicHelper.GetImgFromRemote(cloudUser.Image);
                    CloudMusicNoLogin.Visibility = Visibility.Collapsed;
                    CloudMusicLogin.Visibility = Visibility.Visible;
                });
            }
        }

        private void ShowQRCode()
        {
            if (CheckCloudMusicLoging) return;
            string unikey = NetMusicHelper.GetUnikeyFromCloudMusic();
            Dispatcher.Invoke(new Action(() =>
            {
                if (string.IsNullOrWhiteSpace(unikey))
                {
                    //DialogResult = false;
                    //Close();
                    return;
                }
                string strCode = "http://music.163.com/login?codekey=" + unikey;
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(strCode, QRCodeGenerator.ECCLevel.Q);
                QRCode qrcode = new QRCode(qrCodeData);

                // qrcode.GetGraphic 方法可参考最下发“补充说明”
                Bitmap qrCodeImage = qrcode.GetGraphic(5, Color.Black, Color.White, null, 15, 6, false);
                LoginQRCode.Source = Utils.BitmapToImageSource(qrCodeImage);
                Thread thread = new Thread(new ThreadStart(() => CheckCloudMusicLogin(unikey)));
                thread.IsBackground = true;
                thread.Start();
                LoginMessageRect.Visibility = Visibility.Hidden;
            }));
        }

        private bool CheckCloudMusicLoging = false;
        private void CheckCloudMusicLogin(string unikey)
        {
            CheckCloudMusicLoging = true;
            while (Visibility == Visibility.Visible && IsVisible)
            {
                int loginMsg = NetMusicHelper.GetLoginStateFromCloudMusic(unikey);
                if (loginMsg == 802) //授权中
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        LoginMessage.Content = "授权中";
                        LoginMessageRect.Visibility = Visibility.Visible;
                    }));
                    Thread.Sleep(1000);
                }
                else if (loginMsg == 803) //登陆成功
                {
                    NetMusicHelper.GetAccountFromCloudMusic();
                    Dispatcher.Invoke(new Action(() =>
                    {
                        LoginMessage.Content = "网易云登陆成功";
                        LoginMessageRect.Visibility = Visibility.Visible;
                        DialogResult = true;
                        Close();
                    }));
                    return;
                }
                else if (loginMsg == 801) //等待扫码
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    ShowQRCode();
                    return;
                }
            }
            CheckCloudMusicLoging = false;
        }

        private void MiguLogin(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MiguLoginCookie.Text)) return;
            var miguUser = NetMusicHelper.GetAccountFromMigu(MiguLoginCookie.Text);
            SetMiguMusicUserInfo(miguUser);
        }

        private void MiguLogout(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MiguLoginCookie.Text = string.Empty;
            NetMusicHelper.MiguMusicU = string.Empty;
            File.WriteAllText(EnvironmentSingle.MiguMusicUPath, string.Empty);
            SetMiguMusicUserInfo(null);
        }

        private void QQLogin(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(QQLoginCookie.Text)) return;
            var miguUser = NetMusicHelper.GetAccountFromQQMusic(QQLoginCookie.Text);
            SetQQMusicUserInfo(miguUser);
        }

        private void QQLogout(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            QQLoginCookie.Text = string.Empty;
            NetMusicHelper.QQMusicU = string.Empty;
            File.WriteAllText(EnvironmentSingle.QQMusicUPath, string.Empty);
            SetQQMusicUserInfo(null);
        }

        private void CloudMusicLogout(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            File.WriteAllText(EnvironmentSingle.NetEasyCsrfTokenPath, string.Empty);
            File.WriteAllText(EnvironmentSingle.NetEasyMusicUPath, string.Empty);
            NetMusicHelper.NetEasyCsrfToken = string.Empty;
            NetMusicHelper.NetEasyMusicU = string.Empty;
            SetCloudMusicUserInfo(null);
        }

        private void OpenUrlTag(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var label = sender as Control;
            var url = label.Tag as string;
            if (!string.IsNullOrEmpty(url))
            {
                System.Diagnostics.Process.Start(url);
            }
        }

        private void TextBoxClearRetuen(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text = textBox.Text.Trim().Replace("\n", "").Replace("\r", "");
        }
    }
}
