namespace Telegraph.CallHandler
{
    public interface IAudioCallConnector
    {
        void Start(string channelName, string username, bool videoCallEnable, bool isCallingByMe, bool isGroupCall, byte[] avatar );

    }
}
