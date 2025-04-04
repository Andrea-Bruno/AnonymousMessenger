using AVFoundation;
using Foundation;
using System;
using System.IO;
using AnonymousWhiteLabel.iOS.Services;
using XamarinShared.ViewCreator;
using MessageCompose.Services;


[assembly: Xamarin.Forms.Dependency(typeof(AudioRecorder))]
namespace AnonymousWhiteLabel.iOS.Services
{
    public class AudioRecorder : IAudioRecorder
    {
        private AVAudioRecorder recorder;
#pragma warning disable CS0169 // Il campo 'AudioRecorder._url' non viene mai usato
        private NSUrl _url;
#pragma warning restore CS0169 // Il campo 'AudioRecorder._url' non viene mai usato
#pragma warning disable CS0169 // Il campo 'AudioRecorder._settings' non viene mai usato
        private NSDictionary _settings;
#pragma warning restore CS0169 // Il campo 'AudioRecorder._settings' non viene mai usato
        private NSUrl audioFilePath;
        private Status status;

        protected Status Status
        {
            get
            {
                return status;
            }

            set
            {
                if (status != value)
                {
                    status = value;
                }
            }
        }


        public void StartRecording()
        {
            Console.WriteLine("Begin Recording");

            var session = AVAudioSession.SharedInstance();
            session.RequestRecordPermission((granted) =>
            {
                Console.WriteLine($"Audio Permission: {granted}");

                if (granted)
                {
                    session.SetCategory(AVAudioSession.CategoryRecord, out NSError error);
                    if (error == null)
                    {
                        session.SetActive(true, out error);
                        if (error != null)
                        {
                            Status = Status.PreparingError;
                        }
                        else
                        {
                            var isPrepared = PrepareAudioRecording() && recorder.Record();
                            if (isPrepared)
                            {
                                Status = Status.Recording;
                            }
                            else
                            {
                                Status = Status.PreparingError;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine(error.LocalizedDescription);
                    }
                }
                else
                {
                    Console.WriteLine("YOU MUST ENABLE MICROPHONE PERMISSION");
                }
            });
        }

        public void StopRecording()
        {
            if (recorder != null)
            {
                recorder.Stop();
                Status = Status.Recorded;
            }
        }

        private NSUrl CreateOutputUrl()
        {
            var fileName = $"Myfile-{DateTime.Now.ToString("yyyyMMddHHmmss")}.aac";
            var tempRecording = Path.Combine(Path.GetTempPath(), fileName);

            return NSUrl.FromFilename(tempRecording);
        }

        private void OnDidPlayToEndTime(object sender, NSNotificationEventArgs e)
        {
            Status = Status.Recorded;
        }

        public byte[] GetOutput()
        {
            AVAudioSession.SharedInstance().SetCategory(AVAudioSession.CategoryPlayback, out NSError error);
            if (File.Exists(audioFilePath.Path))
                return File.ReadAllBytes(audioFilePath.Path);
            return null;

        }

        public void PlayRecording()
        {
            throw new NotImplementedException();
        }


        private bool PrepareAudioRecording()
        {
            var result = false;

            audioFilePath = CreateOutputUrl();

            var audioSettings = new AudioSettings
            {
                SampleRate = 44100,
                NumberChannels = 1,
                AudioQuality = AVAudioQuality.Low,
                Format = AudioToolbox.AudioFormatType.MPEG4AAC,
            };

            // Set recorder parameters
            recorder = AVAudioRecorder.Create(audioFilePath, audioSettings, out NSError error);
            if (error == null)
            {
                // Set Recorder to Prepare To Record
                if (!recorder.PrepareToRecord())
                {
                    recorder.Dispose();
                    recorder = null;
                }
                else
                {
                    recorder.FinishedRecording += OnFinishedRecording;
                    result = true;
                }
            }
            else
            {
                Console.WriteLine(error.LocalizedDescription);
            }

            return result;
        }

        private void OnFinishedRecording(object sender, AVStatusEventArgs e)
        {
            recorder.Dispose();
            recorder = null;

            Console.WriteLine($"Done Recording (status: {e.Status})");
        }

        public void DeleteOutput() {}
    }

    public enum Status
    {
        Unknown,
        PreparingError,
        Recording,
        Recorded,
        Playing,
    }
}