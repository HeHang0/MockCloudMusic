using NAudio.Wave;
using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace MusicCollection.SoundPlayer
{
    public class BSoundPlayer: INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //private class InnerInstance
        //{
        //    /// <summary>
        //    /// 当一个类有静态构造函数时，它的静态成员变量不会被beforefieldinit修饰
        //    /// 就会确保在被引用的时候才会实例化，而不是程序启动的时候实例化
        //    /// </summary>
        //    static InnerInstance() { }
        //    internal static BSoundPlayer instance = new BSoundPlayer();
        //}
        //public static BSoundPlayer getInstance()
        //{
        //    return InnerInstance.instance;
        //}

        private IWavePlayer wavePlayer;
        private WaveStream audioFileReader;
        private DispatcherTimer timer = new DispatcherTimer();
        public string FileName = string.Empty;
        private bool _isPlaying = false;
        public BSoundPlayer()
        {
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += TimerOnTick;
        }
        const double sliderMax = 10.0;
        private void TimerOnTick(object sender, EventArgs e)
        {
            if (audioFileReader != null)
            {
                OnPropertyChanged("CurrentTime");
                _currentMusicPosition = Math.Min(sliderMax, audioFileReader.Position * sliderMax / audioFileReader.Length);
                OnPropertyChanged("CurrentMusicPosition");
            }
        }

        private double _currentMusicPosition = 0;
        public double CurrentMusicPosition
        {
            get { return _currentMusicPosition; }
            set
            {
                if (_currentMusicPosition != value)
                {
                    _currentMusicPosition = value;
                    if (audioFileReader != null)
                    {
                        var pos = (long)(audioFileReader.Length * _currentMusicPosition / sliderMax);
                        audioFileReader.Position = pos; // media foundation will worry about block align for us
                    }
                    OnPropertyChanged("CurrentMusicPosition");
                }
            }
        }

        public bool IsPlaying
        {
            get
            {
                return wavePlayer != null && wavePlayer.PlaybackState == PlaybackState.Playing;
            }
            private set
            {
                _isPlaying = value;
                OnPropertyChanged("IsPlaying");
            }
        }
        private bool _isPause = false;
        public bool IsPause
        {
            get
            {
                return _isPause;
            }
            private set
            {
                _isPause = value;
                OnPropertyChanged("IsPause");
            }
        }
        private bool _isStop = false;
        public bool IsStop
        {
            get
            {
                return wavePlayer == null || wavePlayer.PlaybackState == PlaybackState.Stopped; ;
            }
            private set
            {
                _isStop = value;
                OnPropertyChanged("IsStop");
            }
        }
        public PlaybackState PlaybackState
        {
            get
            {
                if (wavePlayer != null)
                {
                    return wavePlayer.PlaybackState;
                }
                else
                {
                    return PlaybackState.Stopped;
                }                
            }
        }

        public TimeSpan CurrentTime
        {
            get
            {
                if (audioFileReader == null)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return audioFileReader.CurrentTime;
                }
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                if (audioFileReader == null)
                {
                    return TimeSpan.Zero;
                }
                else
                {
                    return audioFileReader.TotalTime;
                }
            }
        }

        private float volume = 1f;
        public float Volume
        {
            get { return volume*10; }
            set
            {
                volume = value*1.0f/10;
                if (wavePlayer != null)
                {
                    wavePlayer.Volume = volume;
                }
                if (volume == 0)
                {
                    IsMute = true;
                    NoMute = !IsMute;
                }
                else
                {
                    IsMute = false;
                    NoMute = !IsMute;
                }
                OnPropertyChanged("Volume");
                OnPropertyChanged("IsMute");
                OnPropertyChanged("NoMute");
            }
        }

        public bool IsMute { get; private set; } = false;
        public bool NoMute { get; private set; } = true;


        public void Play()
        {
            if (string.IsNullOrEmpty(FileName))// || !System.IO.File.Exists(FileName))
            {
                return;
            }

            if (IsPlaying)
            {
                return;
            }
            if (IsPause)
            {
                wavePlayer.Play();
                IsPause = false;
                IsPlaying = true;
                return;
            }

            wavePlayer = new WaveOut();
            audioFileReader = new MediaFoundationReader(FileName);//AudioFileReader(FileName); 
            //audioFileReader.Volume = volume;
            wavePlayer.Volume = volume;
            wavePlayer.Init(audioFileReader);
            wavePlayer.PlaybackStopped += OnPlaybackStopped;
            wavePlayer.Play();
            timer.Start();
            OnPropertyChanged("TotalTime");
            IsPlaying = true;
            IsStop = false;
            IsPause = false;
        }

        public void Pause()
        {
            if (wavePlayer != null)
            {
                wavePlayer.Pause();
                IsPause = true;
                IsPlaying = false;
            }
        }

        public void Stop()
        {
            if (wavePlayer != null)
            {
                //wavePlayer.Stop();
                DisPose();
            }
        }

        private void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            CurrentMusicPosition = 0;
            Stop();
            timer.Stop();
            OnPropertyChanged("MusicEnd");
        }
        private void DisPose()
        {
            if (audioFileReader != null)
            {
                audioFileReader.Dispose();
                audioFileReader = null;
            }
            if (wavePlayer != null)
            {
                wavePlayer.Dispose();
                wavePlayer = null;
            }

            IsPlaying = false;
            IsPause = false;
            IsStop = true;
        }
    }
}
