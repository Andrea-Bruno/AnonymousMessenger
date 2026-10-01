namespace Cryptogram.CallHandler
{
    // SIP-based call connector. Replaces the former Agora connector.
    // This is the SIP entry point for voice/video calls.
    //
    // STATUS: placeholder. The SIP transport (registrar/proxy, TURN/STUN relays,
    // codecs Opus/H.264, SRTP key exchange) and server provisioning are NOT
    // implemented yet. See SIP_CALL_TODO.md for the future step.
    public interface ISipCallConnector
    {
        void Start(string channelName, string username, bool videoCallEnable, bool isCallingByMe, bool isGroupCall, byte[] avatar);
    }
}
