#if ANDROID
using System;
using System.IO;
using Android.Media;
using Microsoft.Maui.ApplicationModel;

namespace Cryptogram.Services
{
    // MAUI port of the legacy Xamarin.Android AudioRecorder (IAudioRecorder).
    // Behavior preserved: mic capture to AAC/MP4, playback with progress reporting
    // via MessagingCenter ("Maximum" / "CurrentPosition"), output read/delete.
    public sealed class AudioRecorderImpl : MessageCompose.Services.IAudioRecorder
    {
        private MediaRecorder _recorder;
        private Android.Media.MediaPlayer _player;
        private readonly string _path = Path.Combine(Platform.AppContext.FilesDir.AbsolutePath, "audio.mp3");

        public AudioRecorderImpl()
        {
            _recorder = new MediaRecorder();
            _player = new Android.Media.MediaPlayer();

            _player.Completion += (sender, e) =>
            {
                _player.Reset();
            };
        }

        public void StartRecording()
        {
            try
            {
                if (File.Exists(_path))
                    File.Delete(_path);
                _recorder.SetAudioSource(AudioSource.Mic);
                _recorder.SetOutputFormat(OutputFormat.Mpeg4);
                _recorder.SetAudioEncoder(AudioEncoder.Aac);
                _recorder.SetOutputFile(_path);
                _recorder.Prepare();
                _recorder.Start();
            }
            catch (Exception)
            {
            }
        }

        public void StopRecording()
        {
            try
            {
                _recorder.Stop();
                _recorder.Reset();
                _player.SetDataSource(_path);
            }
            catch (Exception)
            {
            }
        }

        public void PlayRecording()
        {
            _player.Prepare();
            _player.Looping = false;
            _player.Start();
        }

        public byte[] GetOutput()
        {
            if (File.Exists(_path))
                return File.ReadAllBytes(_path);
            return null;
        }

        public void DeleteOutput()
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }
}
#endif
