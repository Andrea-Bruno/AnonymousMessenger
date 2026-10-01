using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(Cryptogram.CallHandler.SipCallConnector))]
namespace Cryptogram.CallHandler
{
    // Placeholder SIP call connector. This is the SIP entry point that replaces the
    // former Agora connector.
    //
    // STATUS: NOT implemented. This is a deliberate placeholder ("entry point di
    // fantasia") requested for the Agora -> SIP migration. The real SIP stack is a
    // future step. See SIP_CALL_TODO.md for what must be built:
    //   - SIP user agent (register/invite/ack/bye), RFC 3261
    //   - SIP registrar/proxy server + credentials provisioning
    //   - TURN/STUN relays for NAT traversal
    //   - Codecs: Opus (audio), H.264 (video)
    //   - SRTP media encryption + key exchange (SDP)
    //   - Platform media capture (mic/camera/speaker) per Android/iOS
    //
    // Until then, a call reaches this point and reports that SIP is not yet
    // implemented. CallManager catches this and shows the "not implemented" toast.
    public class SipCallConnector : ISipCallConnector
    {
        public void Start(string channelName, string username, bool videoCallEnable, bool isCallingByMe, bool isGroupCall, byte[] avatar)
        {
            throw new NotImplementedException(
                "SIP call transport is not implemented yet. Provide the platform SIP connector (registrar/proxy, TURN/STUN, codecs, SRTP). See SIP_CALL_TODO.md.");
        }
    }
}
