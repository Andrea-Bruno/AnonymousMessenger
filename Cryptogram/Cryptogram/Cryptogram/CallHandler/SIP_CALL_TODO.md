# SIP call transport — future step

Agora has been removed. Calls now route through `ISipCallConnector`
(`CallHandler/ISipCallConnector.cs`). The default implementation
`CallHandler/SipCallConnector.cs` is a **placeholder** that throws
`NotImplementedException`. `CallManager` catches it and shows the
"functionality not implemented" toast.

This file lists what must be built to make calls work over SIP.

## What is already in place
- `ISipCallConnector` — the cross-platform call entry point.
- `SipCallSettings` (`CallHandler/Helpers/SipCallSettings.cs`) — call state holder
  (room, user, encryption, call kind). Renamed from the former `AgoraSettings`.
- `CallManager` — orchestration: permissions check, connector call, signaling
  messages over the existing EncryptedMessaging channel, notifications.
- `EncryptionType` — kept as-is.

## What must be implemented (future step)
1. **SIP user agent** (RFC 3261): REGISTER, INVITE, ACK, BYE, CANCEL.
2. **SIP registrar / proxy server** + user credential provisioning.
   - Server choice, hosting, and DNS/SRV records are to be defined.
3. **NAT traversal**: STUN for discovery, TURN for relay.
4. **Codecs**: Opus (audio), H.264 (video).
5. **Media encryption**: SRTP with key exchange carried in SDP.
6. **Platform media capture** per target:
   - `Platforms/Android/` — AudioRecord/AudioTrack, Camera2, audio routing.
   - `Platforms/iOS/` — AVAudioEngine, AVCaptureSession, speaker/earpiece.
7. **Platform connector registration**: replace the placeholder with a real
   `ISipCallConnector` per platform in `MauiProgram.cs` (DI), or via the
   platform connector pattern used during the MAUI migration.
8. **Call UI wiring**: the call screen (former Agora `RoomActivity` /
   `CallRoomViewController`) must be rebuilt against the SIP connector during the
   MAUI migration. The old Agora UI files are removed with the Agora heads.

## Signaling note
Call setup/teardown signaling (start/end/decline/ringing) already travels over the
EncryptedMessaging channel in `CallManager`. The SIP layer needs only the media
session. Decide whether SIP signaling also goes over EncryptedMessaging (to keep
the no-backend model) or over a standard SIP server. This is a design decision for
the future step.

## Where the old Agora code was
- Shared `CallHandler/`: `IAudioCallConnector`, `AgoraSettings`, `AgoraQuality`,
  `AgoraNetworkStat`, `AgoraStreamStat`, `AgoraTestConstants` — deleted.
- Android head: `Xamarin.Agora.Full.Android` package, `Call/AgoraRtcHandler.cs`,
  `Call/AgoraQualityHandler.cs`, `CallServices/RoomActivity.cs`, proguard Agora
  keeps, "powered by agora" string — to be removed with the Android head migration.
- iOS head: `DT.Xamarin.Agora` binding, `Call/CallRoomViewController.cs`,
  `Call/CallConnector.cs`, `Call/AgoraRtcDelegate.cs` — to be removed with the
  iOS head migration.
