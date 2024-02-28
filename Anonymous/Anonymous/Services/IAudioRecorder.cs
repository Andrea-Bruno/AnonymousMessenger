namespace Telegraph.Services
{
	public interface IAudioRecorder
	{
		void StartRecording();
		void StopRecording();
		void PlayRecording();
		byte[] GetOutput();

	}
}
