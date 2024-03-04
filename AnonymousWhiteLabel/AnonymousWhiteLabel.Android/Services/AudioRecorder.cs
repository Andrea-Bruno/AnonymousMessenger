using Android.Media;
using System;
using Xamarin.Forms;
using System.Threading.Tasks;
using AnonymousWhiteLabel.Droid.Services;
using System.IO;
using XamarinShared.ViewCreator;

[assembly: Dependency(typeof(AudioRecorder))]
namespace AnonymousWhiteLabel.Droid.Services
{
    public class AudioRecorder : IAudioRecorder
    {
        private MediaRecorder _recorder;
        private Android.Media.MediaPlayer _player;
        private readonly string _path = Path.Combine(Android.App.Application.Context.FilesDir.AbsolutePath, "audio.mp3");

        public AudioRecorder()
        {


            _recorder = new MediaRecorder();
            _player = new Android.Media.MediaPlayer();

            _player.Completion += (sender, e) => _player.Reset();
        }

        public static void SaveRecording()
        {
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

        public void ResetRecording()
        {
            _recorder = new MediaRecorder();
            _player = new Android.Media.MediaPlayer();

            _player.Completion += (sender, e) => _player.Reset();
        }

        public async void PlayRecording()
        {

            _player.Prepare();
            _player.Looping = false;
            _player.Start();
            MessagingCenter.Send(this, "Maximum", _player.Duration);
            while (_player.IsPlaying)
            {
                await Task.Delay(100).ConfigureAwait(true);
                // Time = _player.CurrentPosition;
                MessagingCenter.Send(this, "CurrentPosition", _player.CurrentPosition);
            }

        }

        public async void ValueChanged(int value)
        {
            _player.SeekTo(value);
            while (_player.IsPlaying)
            {
                await Task.Delay(100).ConfigureAwait(true);
                // Time = _player.CurrentPosition;
                MessagingCenter.Send(this, "CurrentPosition", _player.CurrentPosition);
            }
        }

        public byte[] GetOutput()
        {
            if (File.Exists(_path))
                return File.ReadAllBytes(_path);
            return null;
        }

        public void DeleteOutput() {}
    }
}