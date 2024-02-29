using AnonymousWhiteLabel.UWP.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Xamarin.Forms;
using XamarinShared.ViewCreator;

[assembly: Dependency(typeof(AudioPlayer))]
namespace AnonymousWhiteLabel.UWP.Services
{
    internal class AudioPlayer : IAudioPlayer
    {
        public event EventHandler PlaybackEnded;
        public event EventHandler MediaSourceInitialized;
        public event EventHandler NaturalDurationChanged;
        MediaPlayer player;
        private MediaSource mediaSource;


        ///<Summary>
        /// Length of audio in seconds
        ///</Summary>
        public double Duration
        {
            get
            {
                //System.Threading.Thread.Sleep(1000);
                var a = player.PlaybackSession.NaturalDuration;
                return player == null ? 0 : a.TotalSeconds;
            }
        }

        ///<Summary>
        /// Current position of audio in seconds
        ///</Summary>
        public double CurrentPosition
        { get { return player == null ? 0 : player.PlaybackSession.Position.TotalSeconds; } }

        ///<Summary>
        /// Playback volume (0 to 1)
        ///</Summary>
        public double Volume
        {
            get => player?.Volume ?? 0;
            set => SetVolume(value, Balance);
        }

        ///<Summary>
        /// Balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right
        ///</Summary>
        public double Balance
        {
            get => player?.AudioBalance ?? 0;
            set { SetVolume(Volume, value); }
        }

        ///<Summary>
        /// Indicates if the currently loaded audio file is playing
        ///</Summary>
        public bool IsPlaying
        {
            get
            {
                if (player == null || player.PlaybackSession == null)
                    return false;
                return player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing; //might need to expand
            }
        }

        ///<Summary>
        /// Continously repeats the currently playing sound
        ///</Summary>
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                if (player != null)
                    player.IsLoopingEnabled = _loop;
            }
        }
        bool _loop;

        ///<Summary>
        /// Indicates if the position of the loaded audio file can be updated
        ///</Summary>
        public bool CanSeek
        { get { return player == null ? false : player.PlaybackSession.CanSeek; } }



        ///<Summary>
        /// Load wave or mp3 audio file from a stream
        ///</Summary>
        public bool Load(Stream audioStream)
        {
            DeletePlayer();

            player = GetPlayer();

            if (player != null)
            {
                mediaSource = MediaSource.CreateFromStream(audioStream?.AsRandomAccessStream(), string.Empty);
                //mediaSource.OpenOperationCompleted += OnMediaSourceInitialized;
                player.PlaybackSession.NaturalDurationChanged += OnNaturalDurationChanged; 
                player.Source = mediaSource;
                player.MediaEnded += OnPlaybackEnded;
            }

            return (player == null || player.Source == null) ? false : true;
        }

        ///<Summary>
        /// Load wave or mp3 audio file from assets folder in the UWP project
        ///</Summary>
        public bool Load(string fileName)
        {
            DeletePlayer();

            player = GetPlayer();

            if (player != null)
            {
                player.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/" + fileName));
                player.MediaEnded += OnPlaybackEnded;
            }

            return player != null && player.Source != null;
        }

        void DeletePlayer()
        {
            Stop();

            if (player != null)
            {
                player.MediaEnded -= OnPlaybackEnded;
                player.Dispose();
                player = null;
                mediaSource.OpenOperationCompleted -= OnMediaSourceInitialized;
                mediaSource.Dispose();
                mediaSource = null;
            }
        }

        private void OnPlaybackEnded(MediaPlayer sender, object args)
        {
            PlaybackEnded?.Invoke(sender, EventArgs.Empty);
        }

        private void OnMediaSourceInitialized(MediaSource sender, object args)
        {
            MediaSourceInitialized?.Invoke(sender, EventArgs.Empty);
        }

        private void OnNaturalDurationChanged(Windows.Media.Playback.MediaPlaybackSession sender, object args)
        {
            NaturalDurationChanged?.Invoke(sender, EventArgs.Empty);
        }



        ///<Summary>
        /// Begin playback or resume if paused
        ///</Summary>
        public void Play()
        {
            if (player == null || player.Source == null)
                return;

            if (player.PlaybackSession.PlaybackState == MediaPlaybackState.Playing)
            {
                Pause();
                Seek(0);
                player.Play();
            }
            else
            {
                player.Play();
            }
        }

        ///<Summary>
        /// Pause playback if playing (does not resume)
        ///</Summary>
        public void Pause()
        {
            player?.Pause();
        }

        ///<Summary>
        /// Stop playack and set the current position to the beginning
        ///</Summary>
        public void Stop()
        {
            if (player != null)
            {
                Pause();
                Seek(0);
            }
        }

        ///<Summary>
        /// Seek a position in seconds in the currently loaded sound file 
        ///</Summary>
        public void Seek(double position)
        {
            if (player == null || player.PlaybackSession == null)
                return;

            //if (player.PlaybackSession.CanSeek)
            player.PlaybackSession.Position = TimeSpan.FromSeconds(position);
        }

        void SetVolume(double volume, double balance)
        {
            if (player == null || isDisposed) return;

            player.Volume = Math.Min(1, Math.Max(0, volume));
            player.AudioBalance = Math.Min(1, Math.Max(-1, balance));
        }

        MediaPlayer GetPlayer()
        {
            return new MediaPlayer() { AutoPlay = false, IsLoopingEnabled = _loop };
        }

        bool isDisposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed || player == null)
                return;

            if (disposing)
                DeletePlayer();

            isDisposed = true;
        }



        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }


        public string Path => null;

        public void ChangeSpeaker(bool isEarpieceEnabled, int currentPosition) { }
    }
}
