using Android.Content.Res;
using System;
using System.IO;
using Uri = Android.Net.Uri;
using Xamarin.Forms;
using Anonymous.Droid.Services;
using Android.OS;
using AndroidApp = Android.App.Application;
using Android.Media;
using XamarinShared.ViewCreator;

[assembly: Dependency(typeof(AudioPlayerManager))]
namespace Anonymous.Droid.Services
{
    public class AudioPlayerManager : IAudioPlayer
    { 
       ///<Summary>
       /// Raised when audio playback completes successfully 
       ///</Summary>
        public event EventHandler PlaybackEnded;

        Android.Media.MediaPlayer player;

        static int index = 0;
        public static PowerManager.WakeLock WakeLock = ((PowerManager)AndroidApp.Context.GetSystemService(AndroidApp.PowerService)).NewWakeLock(WakeLockFlags.ProximityScreenOff | WakeLockFlags.AcquireCausesWakeup, "tag");

        ///<Summary>
        /// Length of audio in seconds
        ///</Summary>
        public double Duration
        { get { return player == null ? 0 : ((double)player.Duration) / 1000.0; } }

        ///<Summary>
        /// Current position of audio playback in seconds
        ///</Summary>
        public double CurrentPosition
        { get { return player == null ? 0 : ((double)player.CurrentPosition) / 1000.0; } }

        ///<Summary>
        /// Playback volume (0 to 1)
        ///</Summary>
        public double Volume
        {
            get { return _volume; }
            set { SetVolume(_volume = value, Balance); }
        }
        double _volume = 0.5;

        ///<Summary>
        /// Balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right
        ///</Summary>
        public double Balance
        {
            get { return _balance; }
            set { SetVolume(Volume, _balance = value); }
        }
        double _balance = 0;

        ///<Summary>
        /// Indicates if the currently loaded audio file is playing
        ///</Summary>
        public bool IsPlaying
        { get { return player == null ? false : player.IsPlaying; } }

        ///<Summary>
        /// Continously repeats the currently playing sound
        ///</Summary>
        public bool Loop
        {
            get { return _loop; }
            set { _loop = value; if (player != null) player.Looping = _loop; }
        }
        bool _loop;

        ///<Summary>
        /// Indicates if the position of the loaded audio file can be updated
        ///</Summary>
        public bool CanSeek
        { get { return player == null ? false : true; } }

        public string Path => path;

        private string path;

        /// <summary>
        /// Instantiates a new SimpleAudioPlayer
        /// </summary>
        public AudioPlayerManager()
        {
            player = new Android.Media.MediaPlayer() { Looping = Loop };
            player.Completion += OnPlaybackEnded;
        }

        ///<Summary>
        /// Load wav or mp3 audio file as a stream
        ///</Summary>
        public bool Load(System.IO.Stream audioStream)
        {
            player.Reset();

            DeleteFile(path);

            //cache to the file system
            path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), $"cache{index++}.wav");
            var fileStream = File.Create(path);
            audioStream.CopyTo(fileStream);
            fileStream.Close();

            try
            {
                player.SetDataSource(path);
            }
            catch
            {
                try
                {
                    var context = Android.App.Application.Context;
                    player?.SetDataSource(context, Uri.Parse(Uri.Encode(path)));
                }
                catch
                {
                    return false;
                }
            }

            return PreparePlayer();
        }

        ///<Summary>
        /// Load wav or mp3 audio file from the iOS Resources folder
        ///</Summary>
        public bool Load(string fileName)
        {
            player.Reset();

            AssetFileDescriptor afd = Android.App.Application.Context.Assets.OpenFd(fileName);

            player?.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);

            return PreparePlayer();
        }

        bool PreparePlayer()
        {
            try
            {
                player?.Prepare();
            }
            catch (Java.Lang.Throwable)
            {
                //Media player Java.Lang.IllegalStateException: Exception of type 'Java.Lang.IllegalStateException' was thrown
            }
            return (player == null) ? false : true;
        }

        void DeletePlayer()
        {
            Stop();

            if (player != null)
            {
                player.Completion -= OnPlaybackEnded;
                player.Release();
                player.Dispose();
                player = null;
            }

            DeleteFile(path);
            path = string.Empty;
        }

        void DeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) == false)
            {
                try
                {
                    File.Delete(path);
                }
                catch
                {
                }
            }
        }

        ///<Summary>
        /// Begin playback or resume if paused
        ///</Summary>
        public void Play()
        {
            if (player == null)
                return;

            if (IsPlaying)
            {
                Pause();
                Seek(0);
            }

            player.Start();
        }

        ///<Summary>
        /// Stop playack and set the current position to the beginning
        ///</Summary>
        public void Stop()
        {
#pragma warning disable CS0618 // 'MediaPlayer.SetAudioStreamType(Stream)' è obsoleto: 'deprecated'
            player.SetAudioStreamType(Android.Media.Stream.Music);
#pragma warning restore CS0618 // 'MediaPlayer.SetAudioStreamType(Stream)' è obsoleto: 'deprecated'
            if (!IsPlaying)
                return;

            Pause();
            Seek(0);
        }

        ///<Summary>
        /// Pause playback if playing (does not resume)
        ///</Summary>
        public void Pause()
        {
            player?.Pause();
        }

        ///<Summary>
        /// Set the current playback position (in seconds)
        ///</Summary>
        public void Seek(double position)
        {
            if (CanSeek)
                player?.SeekTo((int)position * 1000);
        }

        ///<Summary>
        /// Sets the playback volume as a double between 0 and 1
        /// Sets both left and right channels
        ///</Summary>
        void SetVolume(double volume, double balance)
        {
            volume = Math.Max(0, volume);
            volume = Math.Min(1, volume);

            balance = Math.Max(-1, balance);
            balance = Math.Min(1, balance);

            // Using the "constant power pan rule." See: http://www.rs-met.com/documents/tutorials/PanRules.pdf
            var left = Math.Cos((Math.PI * (balance + 1)) / 4) * volume;
            var right = Math.Sin((Math.PI * (balance + 1)) / 4) * volume;

            player?.SetVolume((float)left, (float)right);
        }

        void OnPlaybackEnded(object sender, EventArgs e)
        {
            PlaybackEnded?.Invoke(sender, e);

            //this improves stability on older devices but has minor performance impact
            if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.M)
            {
                player.SeekTo(0);
                player.Stop();
                player.Prepare();
            }
        }

        bool isDisposed = false;

        ///<Summary>
		/// Dispose SimpleAudioPlayer and release resources
		///</Summary>
       	protected virtual void Dispose(bool disposing)
        {
            if (isDisposed || player == null)
                return;

            if (disposing)
                DeletePlayer();

            isDisposed = true;
        }

        ~AudioPlayerManager()
        {
            Dispose(false);
        }

        ///<Summary>
        /// Dispose SimpleAudioPlayer and release resources
        ///</Summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void ChangeSpeaker(bool isEarpieceEnabled, int currentPosition)
        {
            if (path == null)
                return;
            player.Stop();
            player.Reset();
            AudioManager am = (AudioManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.AudioService);

            try
            {
                if (isEarpieceEnabled)
                {
                    am.SpeakerphoneOn = false;
                    am.Mode = Mode.InCall;
                    player.SetAudioAttributes(
                                new AudioAttributes
                                   .Builder()
                                   .SetContentType(AudioContentType.Speech)
                                   .SetUsage(AudioUsageKind.VoiceCommunication)
                                   .Build());
                    WakeLock.Acquire();
                }
                else
                {
                    am.SpeakerphoneOn = true;
                    am.Mode = Mode.Normal;
                    player.SetAudioAttributes(
                                new AudioAttributes
                                   .Builder()
                                   .SetContentType(AudioContentType.Music)
                                   .SetUsage(AudioUsageKind.Media)
                                   .Build());
                    if (WakeLock.IsHeld)
                        WakeLock.Release();
                }
            }
            catch (Java.Lang.Throwable th)
            {
            }
            player.SetDataSource(path);

            PreparePlayer();
            Seek(currentPosition);
            Play();
        }
    }
}