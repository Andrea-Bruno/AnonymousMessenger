# Piano di migrazione Xamarin.Forms → .NET MAUI — AnonymousMessenger

Documento di lavoro. Aggiornato: 2026-09-28.

## 0. Baseline verificato

- **.NET SDK** 10.0.401 + workload MAUI installati: `android 36.1.69`, `ios 26.5.10301`, `maccatalyst 26.5.10301`, `maui-windows 10.0.20`.
- **Shared `netstandard2.1`** (AnonymousWhiteLabel + Shared\* + SecureStorage + CommunicationChannel + EncryptedMessaging): compila con **0 errori, 184 warning**.
  - Warning prevalenti: `CS0618` (API deprecate: `MessageFormat.MessageType.*Call`, `BindableProperty.Create<>` generico, `NotificationManager.SendNotification`), `CS0168` (variabili `e`/`ex` dichiarate e non usate).
- **Head Android/iOS/UWP**: csproj old-style (`Xamarin.Android.CSharp.targets`, `TargetFrameworkVersion v13.0`, `Xamarin.Forms 5.0.0.2622`). **Non compilabili** con SDK .NET 10: il workload Xamarin legacy è stato rimosso.
- **Dipendenze esterne** presenti e compilabili: `SecureStorage`, `CommunicationChannel`, `EncryptedMessaging`.

## 1. Realtà del lavoro (lettura onesta)

- **49 progetti**, **107 file XAML**, **2 app parallele** (AnonymousWhiteLabel + Cryptogram), ciascuna con head Android/iOS/UWP.
- **"Testare ogni view"** richiede dispositivi/emulatori + backend di messaggistica (EncryptedMessaging) + push Firebase: **non è eseguibile in modo headless** in questa sessione. Il port può essere verificato in **compilazione**; il test funzionale delle view è un passo separato eseguito dall'utente su emulatore/device.
- **UWP non esiste in .NET MAUI** → il target Windows diventa **WinUI 3** (`net10.0-windows10.0.19041.0`).
- Molti **plugin legacy** non hanno un equivalente drop-in in MAUI (vedi §3).
- **Conseguenza:** un port completo e testato su device è un programma di **settimane**. In questa sessione consegno: (a) questo piano dettagliato, (b) un **incremento verificabile** — pulizia della shared layer (warning/dead code) + scaffold MAUI. **Non dichiarerò il port completo né le view testate su device se non è vero.**

## 2. Target (determinato da dati oggettivi — 2026-09-28)

**Target vivo = CRYPTOGRAM** (`Cryptogram\Cryptogram.sln`, package `com.Cryptogram.Cryptogram`). AnonymousWhiteLabel è la base "white label" minimalista, non il prodotto attivo.

Evidenze oggettive:
- **Versione**: Cryptogram `2.6.0` / versionCode **69** > AnonymousWhiteLabel `2.2.1` / versionCode **51** (versionCode più alto = release più recente).
- **Rebrand**: commit `bd3df7b` "renaming of the application with **Cryptogram branding**"; `e0c4258` "Add Cryptogram sub solution".
- **Ultimo commit per cartella**: Cryptogram 2025-04-08 > AnonymousWhiteLabel 2025-04-04.
- **Lavoro non committato attuale (2026-09)**: ~510 insertions concentrate nel core UI di Cryptogram (ChatRoom.xaml 291, ChatRoom.xaml.cs 214, MainPage, ProfilePage) vs ~17 in AnonymousWhiteLabel (solo bootstrap: MainActivity, App.xaml.cs, InfoPage). ~30× più pesante, e nel cuore dell'app.
- **README**: Cryptogram = derivato "più complesso" (crypto wallet, social); AnonymousWhiteLabel = base minimalista.
- **Package identity**: Cryptogram ha package proprietario `com.Cryptogram.Cryptogram`; AnonymousWhiteLabel usa `com.dtsocialize.uup` (generico).

Le shared lib (`Shared\*`) sono comuni ai due: la loro migrazione avvantaggia comunque entrambi, ma il prodotto da portare a MAUI come main è Cryptogram.

Assunzioni residue:
2. Le cartelle con prefisso `_` (`Shared\_ChatComposer`, `Shared\_CustomViewElements`, `Shared\_MessageCompose`, `Shared\_XamarinShared`, `Shared\_WhiteLabelDesignComponents`, `Shared\_NotificationService`, `Cryptogram\_Keyboard`, `Cryptogram\_Localization`, `Cryptogram\_Utils`) sono **duplicati/backup non referenziati** dalla soluzione principale → candidati a eliminazione.
3. Le modifiche già presenti nel working tree (file `modified`) sono lavoro dell'utente: preservate, non revertite.

## 3. Mappa plugin legacy → sostituti MAUI

| Plugin Xamarin | Stato | Sostituto MAUI |
|---|---|---|
| Xamarin.Forms 5.0 | core | `Microsoft.Maui.*` (framework incluso) |
| Xamarin.Essentials 1.8.1 | incluso in MAUI | `Microsoft.Maui.Storage` / `Microsoft.Maui.*` (API quasi 1:1) |
| ZXing.Net.Mobile / .Forms | no MAUI | `ZXing.Net.Maui` / CommunityToolkit MAAP Barcode |
| Rg.Plugins.Popup | no MAUI | `CommunityToolkit.Maui` Popup |
| Plugin.Fingerprint | no MAUI | CommunityToolkit.Maui Biometric / MAUI Essentials |
| Plugin.MediaManager.Forms | no MAUI | CommunityToolkit.Maui / player nativo |
| Xamarin.Plugin.FilePicker | obsoleto | MAUI `FilePicker` (Essentials) |
| Plugin.Sensors | no MAUI | MAUI `SensorExtensions` / Essentials |
| Plugin.AudioRecorder | no MAUI | MAUI `MediaRecorder` / CommunityToolkit |
| Xam.Plugin.LatestVersion | no MAUI | MAUI `AppInfo` + marketplace URI |
| Xamarin.Firebase.Iid / GooglePlayTasks | no MAUI | Firebase MAUI (`Plugin.Firebase`) o push alternativo |
| Google.Apis.Drive.v3 / Google.Cloud.Translation.V2 | OK multi-target | restano (net-standard compatible) |
| GeoJSON.Net | OK | resta |

## 4. Piano a fasi — ToDo list

### Fase 0 — Fondamenta e pulizia shared (verificabile ORA)
- [ ] 0.1 Eliminare warning banali shared: variabili non usate (`CS0168`), `using` inutilizzati.
- [ ] 0.2 Eliminare dead code / blocchi `#if false` / codice commentato morto nella shared.
- [ ] 0.3 Sostituire API deprecate `CS0618` dove il rimpiazzo è sicuro e locale; documentare quelle che richiedono il contratto `SubApplicationCommandWithData` (EncryptedMessaging).
- [ ] 0.4 Ricompilare shared → obiettivo **0 warning**.

### Fase 1 — Scaffold MAUI single-project (AnonymousWhiteLabel)
- [ ] 1.1 Nuovo progetto `Microsoft.NET.Sdk` con `UseMaui=true`, `TargetFrameworks=net10.0-android;net10.0-ios;net10.0-maccatalyst` (+ `net10.0-windows10.0.19041.0`).
- [ ] 1.2 Convertire `App.xaml`/`Application` → `MauiApp` + `MauiProgram.cs` (host DI, handler, toolkit).
- [ ] 1.3 Spostare risorse (font, immagini) in `Resources/` con convenzioni MAUI (`ResizeAppIcon`, `MauiFont`, `MauiImage`).

### Fase 2 — Migrazione shared UI (XAML + handler)
- [ ] 2.1 Allineare namespace XAML (`xmlns` controlli MAUI) nei progetti Shared.
- [ ] 2.2 Convertire custom renderer → handler dove presenti.
- [ ] 2.3 `Xamarin.Essentials.*` → `Microsoft.Maui.*` (Preferences→Preferences, Connectivity→Connectivity, ecc.).
- [ ] 2.4 Multi-target delle shared lib da `netstandard2.1` a TFM MAUI (necessario per `UseMaui`).

### Fase 3 — Head Android
- [ ] 3.1 `MainActivity` → `MauiActivity` (platform-specific), migrare `FirebaseInstanceId`, battery optimization, notification intent.
- [ ] 3.2 `AndroidManifest.xml` → `Platforms/Android/AndroidManifest.xml` MAUI.
- [ ] 3.3 Risorse Android → `Platforms/Android/Resources`.

### Fase 4 — Head iOS / MacCatalyst
- [ ] 4.1 `AppDelegate` → `Platforms/iOS/AppDelegate.cs` (MauiApplication).
- [ ] 4.2 `Info.plist`/`Entitlements` → `Platforms/iOS`.
- [ ] 4.3 Estensioni (CryptogramIosExtension/ServiceExtension) valutate a parte.

### Fase 5 — Windows (WinUI 3)
- [ ] 5.1 Sostituire UWP con `net10.0-windows` head MAUI.
- [ ] 5.2 Migrare `App.xaml`/`MainPage.xaml` UWP-specific.

### Fase 6 — Sostituzione plugin
- [ ] 6.1 Sostituire ogni plugin legacy secondo la mappa §3, aggiornando i call-site.
- [ ] 6.2 Push: valutare `Plugin.Firebase` o alternativa.

### Fase 7 — Bug, logica, ridondanze
- [ ] 7.1 Individuare e correggere bug logici (duplicazioni app, provider, gestione stato).
- [ ] 7.2 Completare lavoro incompiuto (TODO/partial).
- [ ] 7.3 Eliminare ridondanze tra le due app e le shared duplicate.

### Fase 8 — Test
- [ ] 8.1 `dotnet build` ogni progetto → 0 warning.
- [ ] 8.2 `dotnet test` ElectrumXClient.Tests + eventuali unit test.
- [ ] 8.3 Test funzionale view su emulatore Android / iOS simulator / Windows (utente).

### Fase 9 — Review finale
- [ ] 9.1 Review completo del codice post-migrazione.
- [ ] 9.2 Verifica assenza regressioni in compilazione.

## 5. Strumenti diagnostici
- `dotnet build -warnaserror` per gate warning.
- .NET MAUI analyzers (inclusi nel workload).
- Eventuale `dotnet format` per stile.
- Test: xUnit per ElectrumXClient.Tests (già presente).

## 6. Nota su "completato"
Il criterio "tutto corretto e testato" per un port di queste dimensioni **non** è raggiungibile e verificabile in una sola sessione headless. Verrà consegnato ciò che è **verificabile in compilazione** e chiaramente marcato ciò che richiede device.

## 7. Progresso sessione 2026-09-28 (verificato)

Fatto e verificato (compilazione `AnonymousWhiteLabel.csproj` → **0 errori**):
- **CS0168 eliminati**: 13 siti `catch (Exception e/ex)` con variabile non usata → `catch (Exception)` (Utils, DesignElementsHandler, Calls, Outgoing/IncomingBaseView, MessageReadStatus, ChatPageSupport, ViewCreator/Utils, MessageViewCreator, BasePage, AttachmentPopupPage ×3).
- **Fix MAUI necessario**: `CustomAvatar.ContactProperty` convertito da `BindableProperty.Create<CustomAvatar, Contact>(...)` (overload generico **inesistente in MAUI**) a `BindableProperty.Create(nameof(Contact), typeof(Contact), typeof(CustomAvatar), null)`.
- Piano + ToDo list (questo file).

Non fatto — e perché (vincoli reali):
- **96 `CS0618` residui** = deprecazioni di **protocollo di messaggistica** (`MessageType.*Call → SubApplicationCommandWithData`, `My.GetId() → Id`, overload `SendNotification`). Cambiarli "alla cieca" tocca la logica di messaggistica di EncryptedMessaging e **rischia di introdurre bug**: richiedono migrazione deliberata + test (Fase 7), non pulizia automatica.
- **Test ElectrumXClient.Tests**: progetto fermo a `netcoreapp2.1` (EOL) + NUnit 3.10 + Test SDK 15.8 → **non restore** su SDK moderno (NU1201). Inoltre sono **integration test di rete** (server ElectrumX vivo + `client-secrets.json`): non deterministici in ambiente headless. Fix consigliato: bump TFM `net8.0`/`net10.0` + NUnit 3.14/adapter 4.5/Test SDK 17.11; l'esecuzione verde richiede rete e secrets.
- **Port MAUI vero e proprio** (Fasi 1–6): le shared lib usano tipi `Xamarin.Forms` che **non coesistono** con MAUI → conversione "big-bang", non incrementale. Richiede settimane + device per il test delle ~100 view + backend + push Firebase.

Prossimo passo consigliato: confermare **target vivo** (AnonymousWhiteLabel vs Cryptogram) e autorizzare la conversione big-bang di una shared lib alla volta con build gate, oppure procedere su slice specifiche verificabili.
