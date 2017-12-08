using NAudio.Wave;
using System;

namespace MusicCollection.SoundPlayer
{
    public class BSoundPlayer2
    {
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
        //private BSoundPlayer()
        //{

        //}
        private IWavePlayer wavePlayer;
        private AudioFileReader audioFileReader;

        public string FileName = string.Empty;        
        public bool IsPlaying { get; private set; }
        public bool IsPause { get; private set; }
        public bool IsStop { get; private set; } = false;
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
            set
            {
                if (audioFileReader != null)
                {
                    audioFileReader.CurrentTime = value;
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
            get { return volume; }
            set
            {
                if (value >= 0 && value <= 1f)
                {
                    volume = value;
                    if (audioFileReader != null)
                    {
                        audioFileReader.Volume = value;
                    }
                }
            }
        }

        public void Play()
        {
            if (string.IsNullOrEmpty(FileName) || !System.IO.File.Exists(FileName))
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
            audioFileReader = new AudioFileReader(FileName);
            audioFileReader.Volume = volume;
            wavePlayer.Init(audioFileReader);
            wavePlayer.PlaybackStopped += OnPlaybackStopped;
            wavePlayer.Play();
            IsPlaying = true;
            IsStop = false;
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
            Stop();
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
