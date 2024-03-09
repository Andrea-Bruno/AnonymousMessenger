namespace MessageCompose.Services
{
	public interface IAudioRecorder
	{
		void StartRecording();
		void StopRecording();
		void PlayRecording();
		byte[] GetOutput();
		void DeleteOutput();

	}
}
