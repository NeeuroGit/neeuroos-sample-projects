```markdown
# NeeuroOS Unity SDK Integration Guide for AI Coding Agents

This document describes how an automated coding agent should integrate the **NeeuroOS SDK** into an existing or new Unity project targeting Android, iOS, and/or Windows.[page:1]

---

## 1. Prerequisites

- Unity version  
  - Minimum Unity 2022.2.4f1.[page:1]
- Target platforms & OS  
  - Android: minimum Android 5.0 (API level 21), Android SDK ≥ API 26, Gradle 7.5.1+.[page:1]  
  - iOS: minimum iOS 8, Xcode 14+.[page:1]  
  - Windows: PC with Bluetooth capability (built‑in or dongle).[page:1]
- Hardware  
  - SenzeBand 2 device.[page:1]
- Developer Code  
  - A valid Neeuro **Developer Code** to authenticate the SDK with Neeuro servers.[page:1]

---

## 2. High‑Level Integration Flow

1. Import NeeuroOS Unity SDK into the Unity project.[page:1]  
2. Configure build targets (Android / iOS / Windows) and project player settings.[page:1]  
3. Add NeeuroOS Prefabs (NSB_BLE, NSB_EEG) to a bootstrap scene.[page:1]  
4. Implement an initialization script that:  
   - Injects the Developer Code.  
   - Registers callbacks for EEG/PPG/metrics data and connection/authentication events.[page:1]
5. Implement device scan and connect UI/workflow.[page:1]  
6. Implement start/stop data acquisition and data handling logic.[page:1]  
7. Configure platform‑specific build settings and produce final builds.[page:1]

---

## 3. Unity Project Setup

### 3.1 Project Creation / Selection

- Use or create a 3D/URP Unity project with the required minimum version (2022.2.4f1+).[page:1]  
- Ensure scripting backend and API compatibility are set to values compatible with native plugins (typically IL2CPP for mobile).[page:1]

### 3.2 Import NeeuroOS Unity SDK

> Note: The SDK package is supplied by Neeuro and contains Unity plugins, scripts, and a sample project or scenes.[page:1]

Steps for an agent:

- Copy or import the provided Unity package into the project (e.g., via `Assets > Import Package > Custom Package` if a `.unitypackage` is provided).[page:1]  
- Verify that:  
  - C# scripts for NeeuroOS are present under a dedicated folder.  
  - Native libraries/plugins for Android, iOS, and/or Windows are present under `Plugins` with proper platform settings.[page:1]

---

## 4. Adding NeeuroOS Components to a Scene

### 4.1 Prefabs

The Unity SDK provides prefabs including:[page:1]

- `NSB_BLE` – handles Bluetooth scanning, connection, and disconnection.  
- `NSB_EEG` – handles EEG/PPG data streaming and higher‑level metrics (attention, relaxation, workload, frequency bands, etc.).[page:1]

Agent actions:

- Create or choose a bootstrap scene (e.g., `NeeuroBootstrap`).  
- Add the `NSB_BLE` and `NSB_EEG` prefabs into the scene hierarchy.  
- Ensure they are not destroyed on load if needed (e.g., mark with `DontDestroyOnLoad` in a manager script) so data streaming can persist across scenes.

---

## 5. Initialization and Developer Code

The Developer Code is required to authenticate with Neeuro servers before data can be used.[page:1]

Agent tasks:

1. Create a C# script, for example `NeeuroInitializer.cs`.  
2. Attach it to an empty GameObject in the bootstrap scene.  
3. Implement initialization logic that:  
   - Passes the Developer Code into the NeeuroOS Unity API.  
   - Wires necessary callbacks for authentication status and result, and validity period.[page:1]

Pseudo‑structure:

```csharp
public class NeeuroInitializer : MonoBehaviour
{
    [SerializeField] private string developerCode = "<INSERT_DEVELOPER_CODE>";

    void Start()
    {
        // Example concept: call SDK init entry point if exposed by the Unity plugin.
        // The actual method name and parameters are defined by the Neeuro Unity package.
        NeeuroUnityAPI.Initialize(
            developerCode,
            OnAuthenticationStatus,
            OnAuthenticationResult,
            OnAuthenticationValidityPeriod
        );
    }

    private void OnAuthenticationStatus(bool status)
    {
        // Handle ongoing authentication status updates (e.g., countdown validity).
    }

    private void OnAuthenticationResult(bool success)
    {
        // Handle success/failure of authentication.
        // Data streaming should be blocked if success == false.
    }

    private void OnAuthenticationValidityPeriod(long ms)
    {
        // Optional: store validity period in state/UI.
    }
}
```

> The Unity plugin mirrors the native flow: initialize → authenticate → check validity period before starting data streams.[page:1]

---

## 6. Scanning and Connecting to SenzeBand

The logical flow is equivalent to the Android SDK: **Initialisation → Scanning → Connecting → Authenticating**.[page:1]

Agent tasks:

1. Create a manager script (e.g., `SenzeBandManager.cs`) that interacts with `NSB_BLE`.  
2. Provide methods such as `StartScan()`, `StopScan()`, `ConnectToDevice(string macAddress)`, `Disconnect()`.[page:1]  
3. Register listeners for scan results and connection callbacks (success, broken, fail).[page:1]

Conceptual API usage (names may differ in the actual Unity package):

```csharp
public class SenzeBandManager : MonoBehaviour
{
    public void StartScan()
    {
        // Trigger BLE scan via Unity wrapper.
        NeeuroUnityAPI.StartStopScanning(true);
    }

    public void StopScan()
    {
        NeeuroUnityAPI.StartStopScanning(false);
    }

    // Called by SDK when a device is found
    private void OnDeviceFound(string deviceName, string macAddress)
    {
        // Store devices for UI selection.
    }

    public void ConnectToDevice(string macAddress)
    {
        NeeuroUnityAPI.Connect(macAddress);
    }

    public void Disconnect()
    {
        NeeuroUnityAPI.Disconnect();
    }

    private void OnConnectionSucceeded()
    {
        // Move to authentication/impedance check/calibration steps.
    }

    private void OnConnectionBroken()
    {
        // Clean up state and update UI.
    }

    private void OnConnectionFailed()
    {
        // Notify user/agent and optionally retry.
    }
}
```

These callbacks mirror the native Android `connectionCallBackInterface` methods such as `connectionSucceed`, `connectionBroken`, `connectionFail`.[page:1]

---

## 7. Impedance Check and Calibration

Before using EEG data, the SDK recommends impedance and calibration steps.[page:1]

Agent tasks:

- Expose methods mapped to commands:
  - Switch to AC mode (impedance measurement) and back to DC mode.  
  - Start/stop calibration for accelerometer/orientation.[page:1]

Conceptual mapping (Unity wrapper around `grabInputCommand` equivalents):

```csharp
public void StartImpedanceCheck()
{
    NeeuroUnityAPI.AcLeadoff();   // COMMAND_AC_LEADOFF equivalent
}

public void StopImpedanceCheck()
{
    NeeuroUnityAPI.DcLeadoff();   // COMMAND_DC_LEADOFF equivalent
}

public void StartCalibration()
{
    NeeuroUnityAPI.StartCalibration(); // COMMAND_CAL_START
}

public void StopCalibration()
{
    NeeuroUnityAPI.StopCalibration();  // COMMAND_CAL_STOP
}
```

Agent should optionally display impedance values and channel status via callbacks analogous to `EEG_GetImpedance` and `EEG_ChannelStatus` in native docs.[page:1]

---

## 8. Starting and Stopping Data Streams

The SDK provides commands to start/stop EEG and PPG streams and exposes multiple data callbacks.[page:1]

Agent tasks:

1. Implement methods for controlling streams:  

```csharp
public void StartEEG()
{
    NeeuroUnityAPI.StartEEG();  // COMMAND_START equivalent
}

public void StopEEG()
{
    NeeuroUnityAPI.StopEEG();   // COMMAND_STOP equivalent
}

public void StartPPG()
{
    NeeuroUnityAPI.StartPPG();  // COMMAND_PPG_START
}

public void StopPPG()
{
    NeeuroUnityAPI.StopPPG();   // COMMAND_PPG_STOP
}
```

2. Register data callbacks for:
   - Raw EEG and filtered EEG.  
   - Attention, relaxation, mental workload.  
   - SPO2 and heart rate.  
   - Frequency bands (Alpha, Beta, Theta, Delta, Gamma, and sub‑bands).  
   - Impedance and channel status.  
   - Accelerometer values, orientation, etc.[page:1]

Example callback handler mirroring native EEG delegate semantics:

```csharp
private void OnAttentionChanged(float attention)
{
    // attention is clamped between 0.0 and 1.0 in native examples.
    // Use this value to drive gameplay/UX.
}

private void OnRelaxationChanged(float relaxation) { }

private void OnWorkloadChanged(float workload) { }

private void OnRawEEGReceived(int[] eegSample) { }

private void OnFilteredEEGReceived(float[] eegSample) { }

private void OnSPO2AndHeartRate(int spo2, int heartRate, int heartRateDetectFlag) { }

private void OnFrequencyBands(float[,] abdtValues)
{
    // 2D array with Alpha, Beta, Delta, Theta, Gamma and sub‑bands.
}
```

These align with methods such as `EEG_GetAttention`, `EEG_GetRelaxation`, `EEG_GetMentalWorkload`, `EEG_GetRawData`, `EEG_GetFilteredData`, `GetSPO2AndHeartRate`, `EEG_GetABDTRaw`, `EEG_GetABDTNorm` in native docs.[page:1]

---

## 9. Platform‑Specific Build Configuration

### 9.1 Android

Agent steps:[page:1]

- Switch Build Target to Android.  
- Ensure:
  - Minimum API Level ≥ 21, Target API Level ≥ 26.  
  - Gradle version 7.5.1 or later.  
  - Required permissions (Bluetooth, location as required by OS version) are declared (may be handled by the plugin).[page:1]  
- Build and run to device, verifying Bluetooth scanning/connection and data streaming.

### 9.2 iOS

Agent steps:[page:1]

- Switch Build Target to iOS.  
- Build Xcode project.  
- In Xcode:
  - Set signing & capabilities.  
  - Ensure Bluetooth and any required background modes/permissions are configured.  
- Deploy to device and validate connection and streaming.[page:1]

### 9.3 Windows

Agent steps:[page:1]

- Switch Build Target to Windows.  
- Confirm plugin import settings enable the appropriate DLLs for Windows builds.  
- Build player, run on a PC with Bluetooth capability (or dongle) and test scanning/connection/data streaming.[page:1]

---

## 10. Recommended Scene & Script Structure

For a clean integration automated by an AI agent:

- `Scenes/`  
  - `NeeuroBootstrap.unity` – contains `NSB_BLE`, `NSB_EEG`, `NeeuroInitializer`, and `SenzeBandManager`.  
- `Scripts/Neeuro/`  
  - `NeeuroInitializer.cs` – handles Developer Code and SDK initialization + authentication.  
  - `SenzeBandManager.cs` – handles scanning, connection, impedance, calibration, start/stop streams, and dispatches data events to game systems.  
  - Optional UI scripts for device list, connect/disconnect buttons, and metric visualizations.

This structure matches the logical flow described in the NeeuroOS documentation: set up & build SDK → quick‑start sequence → data handling via function and delegate glossaries.[page:1]

---
```
