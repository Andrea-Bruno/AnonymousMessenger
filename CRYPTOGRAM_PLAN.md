# Cryptogram — Completion & MAUI Migration Plan

Active product. Continuation of the discontinued AnonymousWhiteLabel.
Goal: Cryptogram runs on **.NET MAUI (.NET 10)**, all features work, code is clean.
Updated: 2026-09-28.

## 1. What Cryptogram is

A full encrypted messenger plus a crypto wallet. It reuses the same trustless libraries as the white-label (no backend; the account is a key pair). It is more complex than the minimalist white-label base.

Feature set (observed in code):
- Chat: text, image, video, audio, PDF, location, contact card.
- Group chat.
- Voice and video calls (Agora engine, `CallHandler/`).
- Message translation (Google Cloud Translation).
- Backup and restore to Google Drive (`IDriveService`).
- PIN lock screen and passphrase recovery.
- Push notifications (Firebase).
- Proximity sensor for audio playback.
- Light and dark themes.
- Crypto wallet: Bitcoin, Ethereum, tokens, NFT (enabled in `DEBUG_RAM` mode).
- Experimental decentralized social (enabled in `DEBUG_AND` mode).

## 2. Current state (verified 2026-09-28)

- Solution: `Cryptogram\Cryptogram.sln`. Heads: **Android + iOS** (no UWP).
- Framework: **Xamarin.Forms 5.0**. Shared UI on `netstandard2.1`.
- Version: `2.6.0` / versionCode `69`.
- 27 views + 6 popup views.
- Shared libs (common with the white-label): ChatComposer, CustomViewElements, MessageCompose, NotificationService, WhiteLabelDesignComponents, Localization, Utils, XamarinShared.
- External libs: EncryptedMessaging, CommunicationChannel, SecureStorage, FullDuplexStreamSupport, VideoFileCryptographyLibrary.
- Plugins in use: Xamarin.CommunityToolkit, Plugin.Sensors, Agora (calls), Firebase (push), Google.Apis.Drive (backup), Google.Cloud.Translation, media recorder/player, QR (ZXing).
- Build configurations: `Debug`, `Release`, `DebugNuget`, `Test`, plus conditional symbols `DEBUG_RAM` (wallet), `DEBUG_ALI`, `DEBUG_AND` (social).
- Active uncommitted work: `ChatRoom.xaml` / `ChatRoom.xaml.cs`, `MainPage.xaml.cs`, `ProfilePage.xaml.cs`.

## 3. Known technical debt (observed)

- **96 `CS0618`** deprecations in the shared layer: `MessageType.*Call` → `SubApplicationCommandWithData`; `My.GetId()` → `Id`; `NotificationManager.SendNotification` overload. These touch the messaging protocol. They need a deliberate migration with tests, not a blind replace.
- **Empty `catch` blocks** that hide errors: `App.FinishCall`, `App.UploadBackup`.
- **`// JanD4rk : Must be refactored`** markers in `App.xaml.cs`.
- **Crypto wallet not wired**: the `BitcoinWalletService` lines in `DEBUG_RAM` are commented out.
- **Empty methods**: `App.OnNewMessageAddedToView`.
- **Test project broken**: `ElectrumXClient.Tests` targets `netcoreapp2.1` (EOL) and will not restore on a modern SDK. The tests are network integration tests (live ElectrumX server + `client-secrets.json`).

## 4. Plugin replacement map (Cryptogram-specific)

| Current (Xamarin) | MAUI path | Note |
|---|---|---|
| Xamarin.Forms 5.0 | Microsoft.Maui | framework |
| Xamarin.CommunityToolkit | CommunityToolkit.Maui | toast, popup, converters |
| Plugin.Sensors (proximity) | platform code or CommunityToolkit | MAUI Essentials has no proximity sensor — verify official option |
| Agora (calls) | Agora .NET MAUI SDK | research official Agora MAUI support before port |
| Firebase push | Plugin.Firebase | or MAUI-compatible push |
| Google.Apis.Drive.v3 | keep | netstandard, works as-is |
| Google.Cloud.Translation.V2 | keep | netstandard, works as-is |
| Media recorder / player | CommunityToolkit.Maui / MediaPicker | verify audio record + speaker routing |
| ZXing (QR) | ZXing.Net.Maui | scan + show |

Where the official MAUI option is unclear (Agora, proximity sensor), research the official vendor docs before coding. Do not guess.

## 5. Phases and ToDo list

### Phase A — MAUI scaffold
- [ ] A.1 Create MAUI single-project for Cryptogram: `UseMaui=true`, `TargetFrameworks=net10.0-android;net10.0-ios` (add `net10.0-windows10.0.19041.0` for Windows).
- [ ] A.2 Convert `App.xaml`/`Application` to `MauiApp` + `MauiProgram.cs` (DI, handlers, CommunityToolkit.Maui).
- [ ] A.3 Move fonts/images to `Resources/Fonts` and `Resources/Images` with MAUI build actions.

### Phase B — Shared libraries to MAUI
- [ ] B.1 Retarget the shared libs from `netstandard2.1` to MAUI TFMs (required for `UseMaui`).
- [ ] B.2 Update XAML namespaces to MAUI controls.
- [ ] B.3 `Xamarin.Essentials.*` → `Microsoft.Maui.*` (Preferences, Connectivity, Launcher, Map, etc.).
- [ ] B.4 Convert custom renderers to handlers where present.

### Phase C — Cryptogram UI to MAUI
- [ ] C.1 Port the 27 views + 6 popups. Order by dependency: Login/Signup → CreatePin → NavigationTapped (container) → ChatRoom → Call → Profile/Menu → settings pages → media preview pages.
- [ ] C.2 Port ViewModels (BaseViewModel, MessageListViewModel, CallListViewModel, etc.).
- [ ] C.3 Port themes (Light/Dark) to MAUI resource dictionaries.

### Phase D — Plugin replacement
- [ ] D.1 Replace each plugin per the map in §4.
- [ ] D.2 Research and wire Agora for MAUI (calls).
- [ ] D.3 Research proximity sensor option for MAUI.
- [ ] D.4 Firebase push via Plugin.Firebase.

### Phase E — Feature completion
- [ ] E.1 Wire the crypto wallet (`DEBUG_RAM`): re-enable and complete `BitcoinWalletService` integration, or move it behind a clear feature flag.
- [ ] E.2 Complete or remove the decentralized social stub (`DEBUG_AND`).
- [ ] E.3 Verify Google Drive backup/restore end to end.
- [ ] E.4 Verify translation feature end to end.

### Phase F — Bug fixes and deprecation migration
- [ ] F.1 Migrate the 96 `CS0618` protocol deprecations to `SubApplicationCommandWithData` / `Id` / new `SendNotification`, with tests.
- [ ] F.2 Replace empty `catch` blocks with real error handling or logging.
- [ ] F.3 Resolve the `JanD4rk : Must be refactored` markers.
- [ ] F.4 Remove dead code and the `_`-prefixed duplicate folders.

### Phase G — Testing
- [ ] G.1 `dotnet build` every project → 0 warnings.
- [ ] G.2 Fix the test project (bump TFM + packages) and make the ElectrumX tests runnable.
- [ ] G.3 Smoke-test every view on Android emulator and iOS simulator.
- [ ] G.4 End-to-end: send/receive each message type, group, call, backup, wallet.

### Phase H — Final review
- [ ] H.1 Full code review after migration.
- [ ] H.2 Confirm no regressions in build and runtime.

## 6. Verification gate

A phase is done only when the project builds with 0 errors and 0 warnings, and the affected views pass a smoke test on at least one device/emulator. Full "complete and functional" requires device testing, which cannot be done headless.

## 7. Realistic scope note

This is a large migration (27 views, calls, wallet, push, backup). It is a multi-week effort. The plan above is the ordered path. Each phase is independently verifiable by build and smoke test.

## 8. Agora → SIP (done 2026-09-28)

Agora removed from the shared layer. Calls now route through `ISipCallConnector`
(`CallHandler/ISipCallConnector.cs`). Default `SipCallConnector` is a placeholder
that throws `NotImplementedException`; `CallManager` catches it and shows the
"not implemented" toast. `AgoraSettings` renamed to `SipCallSettings`. Agora
telemetry types deleted. Future work documented in `CallHandler/SIP_CALL_TODO.md`.
Verified: the change adds no new compile errors.

## 9. Real build state (updated 2026-09-28)

The Cryptogram shared project now **compiles with 0 errors** (209 warnings).
Resolution of the three blockers:
- `Views/ChatRoom.xaml` — xmlns fixed: `local1`/`local11` now point at
  `CustomViewElements;assembly=CustomViewElements` (where BasePage, KeyboardView,
  CustomLabel, CustomEditor actually live), not `clr-namespace:Cryptogram`.
- `Views/ChatRoom.xaml.cs` — reverted to committed HEAD. The uncommitted WIP
  code-behind referenced named XAML elements (VideoProgress, NewMessageScrollInfo,
  MFTranslate, MFCopy, MFCount) absent from the current XAML; inventing that XAML
  would be guessing. HEAD is the last coherent, compiling ChatRoom. The WIP is
  preserved at `%TEMP%\ChatRoom_wip_backup\` for rollback or a future coherent
  re-apply.
- `Views/LogsPage.xaml.cs` — rewritten to the public `Context` API
  (`HasConnectivity`, `IsConnected`, `EntryPoint`). The internal-only
  `Context.Channel` diagnostics (ClientExists, QueueCount, LastPostParts,
  PostCounter, ErrorLog) are shown as `n/a` until a public accessor exists
  upstream. The unused `using CommunicationChannel;` was removed.

## 11. Warning cleanup (done 2026-09-28)

After the build was fixed, the warning landscape was split by owner. All
mechanically-fixable warnings in **our** projects (Cryptogram + Shared) are now
resolved:
- `My.GetId()` → `My.Id` (identical implementation) — all 11 call sites.
- `CS0168` unused exception variables → `catch (Exception)` (App, CallManager,
  CallPage, MainPage, ChatList).
- `CS0108` hidden members → added `new` (QuestionAnswerViewModel,
  QuestionAnswersInnerViewModel); renamed `SuccessPopup` XAML `x:Name="Title"` →
  `TitleLabel` (it generated a field hiding `Page.Title`).
- `CS4014` un-awaited async → explicit `_ =` fire-and-forget (ChatRoom
  HideStickyDateHeader, EditItemPage toast, GroupUserSelectPage toast).
- Dead fields removed: ChatRoom `_downIconUnreadedMessagecount`, `lastScrollY`,
  `DISABLE_SCROLLING_DIFF`, `isMessageSent`, `previousScrollYState`,
  `canReadPosts`, `scrollContentSize`, `lastLoadedElement` (all only referenced
  in commented-out scroll code, plus the always-false `if`/`else` that read them);
  `VerifyPassphrase.parts` (shadowed by a local); `App.Instance` (never read).

Remaining warnings are NOT ours to fix or are deliberately deferred:
- **Protocol `CS0618`** (Cryptogram 13 + XamarinShared 37): `MessageType.*Call`
  → `SubApplicationCommandWithData` and `Contact.MessageContainerUI`. This is a
  wire-protocol redesign entangled with the SIP signaling step and needs
  two-device send/receive tests — not a blind replace (see §8, §F.1).
- **`CS0649` ×2 (ChatComposer)**: `ChatList._currentSwipeView` and
  `ContactList._onItemClicked` are read but never assigned — incomplete feature
  wiring (a latent bug). Needs a decision: wire the callback (feature completion)
  or remove the vestigial field.
- **Library warnings** (EncryptedMessaging, CommunicationChannel, SecureStorage,
  FullDuplexStreamSupport): all `CS1591` and the libs' own `CS0168/0169/0414/
  0162/1573/1574`. These projects must not be modified.

Next: the MAUI migration (Phases A–C). Device smoke-testing still requires
emulators and cannot be done headless.

## 10. Alignment guides (to apply during migration)

Two guides govern the target:
1. Professional MAUI app guide: MVVM with CommunityToolkit.Mvvm
   (`[ObservableProperty]`, `[RelayCommand]`, `IMessenger`), Clean Architecture
   layers, DI in `MauiProgram.cs` (pages/ViewModels `Transient`), Shell
   navigation with `IQueryAttributable`, no deprecated controls (`ListView`/
   `TableView`/`Frame`/`*AndExpand`), compiled bindings (`x:DataType`),
   accessibility (`SemanticProperties`), `.resx` localization, SecureStorage for
   secrets, xUnit + Appium + XHarness testing, GitHub Actions CI, Serilog/NLog +
   Sentry, Meziantou.Analyzer + `.editorconfig` + `dotnet format`.
2. Xamarin→MAUI CLI porting manual: `dotnet workload install maui`; single-project
   structure (`Platforms/`, `Resources/`); `Xamarin.Forms`→`Microsoft.Maui.Controls`,
   `Xamarin.Essentials`→`Microsoft.Maui.*`; renderer→handler; `MauiProgram.cs` DI;
   build per TFM.

These are applied in Phases A–C. They require the project to compile first (§9).

## 12. Verified MAUI migration surface (measured 2026-10-01)

Concrete, measured scope (not estimates):

**Project layers (by UI dependency):**
- **No UI, stays netstandard2.1** (referenceable by MAUI as-is): `Localization`
  (resx only), `EncryptedMessaging`, `CommunicationChannel`, `VideoFileCryptographyLibrary`,
  `SecureStorage`, `FullDuplexStreamSupport`, `ElectrumXClient`, `CryptoWalletLibrary`.
- **UI class libs to retarget to `net10.0-android;net10.0-ios` + `UseMaui=true`:**
  `Utils` (uses Xamarin.Forms types), `XamarinShared`, `CustomViewElements`,
  `ChatComposer`, `MessageCompose`, `NotificationService`, `WhiteLabelDesignComponents`,
  `Banking`, `CryptoWalletUI`.
- **App:** `Cryptogram.csproj` (shared UI, ~27 views + 6 popups) merged with the
  Xamarin heads into one MAUI single-project.

**Custom controls are heavily used — NOT dead:** 145 XAML usages of
CustomLabel / BasePage / CustomEntry / KeyboardView / etc. across the app. The
control types stay; each needs a MAUI handler or a fallback-to-base decision.
~26 renderers (Android + iOS) → handlers; TouchEffects → MAUI effects.

**Plugin replacements (native code / open-source NuGet per the agreed policy):**
- Agora → SIP (already stubbed, §8).
- Rg.Plugins.Popup → CommunityToolkit.Maui Popup.
- Xamarin.Essentials → Microsoft.Maui.* (built-in).
- Plugin.Fingerprint → platform Biometric (Android BiometricPrompt / iOS LocalAuthentication).
- Plugin.MediaManager.Forms / Plugin.AudioRecorder → native MediaPlayer/AVAudioPlayer via DI service.
- Plugin.Sensors (proximity) → native SensorManager / UIDevice proximity via DI service.
- Lottie → CommunityToolkit.Maui (Lottie) or skip.
- Microsoft.AppCenter → Sentry (per guide).
- glidex.forms / CircleImageView → MAUI handler (round image) + built-in caching.
- Xamarin.Auth → MAUI WebAuthenticator.
- AndHUD → CommunityToolkit.Maui or native progress.
- ZXing.Net.Mobile.Forms → ZXing.Net.MAUI.
- Xamarin.Plugin.FilePicker → MAUI FilePicker (built-in).
- Xamarin.Forms.PancakeView / CircularProgressBar / MR.Gestures → MAUI equivalents or custom handlers.

**~20 platform services** (Android + iOS) to port to `Platforms/` + DI.

**Why not completable/verifiable headless:** the UI cannot be smoke-tested without
devices/emulators; a "compiles" result does not prove the app works. Realistic
effort: multi-week professional migration across ~100 files.

**Execution division of labor:** I can do the code conversion and get the MAUI
project to COMPILE + PACKAGE for `net10.0-android` (the workload is installed);
running the APK and smoke-testing each view requires the user's device/emulator.
Recommended incremental order: retarget leaf UI libs one at a time (build-verify
each), then the app head, then per-view XAML + handlers, testing on device as each
view lands.
