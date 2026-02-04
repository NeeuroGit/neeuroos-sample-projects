//
//  UnityBridge.h
//  IOSBLESBFrame
//
//  Created by Serene Teng on 13/12/16.
//  Copyright © 2016 Serene Teng. All rights reserved.
//

//#import <Foundation/Foundation.h>
//
//@interface UnityBridge : NSObject
//
//@end


#ifdef __cplusplus

extern "C" {
#endif
    
    //To create Delegate to replace "UnitySendMessage" in CBController
    typedef void (*DelegateCBFUnity)(const char*);//delegate
    typedef void (*voidDelegateBoolUnity)(bool);//delegate
    
    //Framework calling for Delegate
    //Calling Delegate for UnitySendMessage
    
    void BLE_SetDeviceConnectStatus(DelegateCBFUnity callbackUnity); //connected status
    void BLE_SetSenzeBandVersion(DelegateCBFUnity callbackUnity);
    void BLE_SetDeviceList(DelegateCBFUnity callbackUnity);
    void BLE_completedBTInit(DelegateCBFUnity callbackUnity);
    void BLE_bluetoothSwitchStatus(DelegateCBFUnity callbackUnity);
    void BLE_SetDeviceNotConnectStatus(DelegateCBFUnity callbackUnity); //disconnect status
    void BLE_SetDeviceConnectFailStatus(DelegateCBFUnity callbackUnity); //connect fail status
    void FNFL_EEG_GetMentalStateData(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetBatteryLevelUpdate(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetAccXYZ(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetChannelStatus(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetGoodConnectionCheck(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetSignalReadyStatus(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetData(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetData200ms(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFilteredData(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFreqTypePower(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFreqTypePowerRaw(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFreqTypePowerNorm(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFreqTypePowerNormReref(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetAuthenticationStatus(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetAuthenticationResult(voidDelegateBoolUnity callbackUnity);
    void FNFL_EEG_GetDirection(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetImpedance(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetSPO2AndHeartRate(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetCalibrationParameters(DelegateCBFUnity callbackUnity);
    void FNFL_PPG_GetRawData(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetEvoke(DelegateCBFUnity callbackUnity);
    void FNFL_commandACK(DelegateCBFUnity callbackUnity);
    //End of Framework calling for Delegate
    
    //Framework Functions
    void Initialized ();
    char* getBluetoothVersion();
    bool isBluetoothPermissionGranted();
    void StartScanDevice();
    void StopScanDevice();
    void startStopEEG (bool toStart);
    void ClearDeviceList ();
    void StartCommand ();
    void StopCommand ();
    void DisconnectDevice (const char *deviceAddress, int type);
    void grabInputCommand (const char *command);
    void ConnectDevice (const char *deviceAddress, int type);
//    void CancelConnection (const char *deviceAddress, int type);
    char* getMCUID (const char *deviceAddress);
    void nfl_developerCode (char *developerCode);
    void nfl_Authenticate ();/// Returns the  authentication validity period remaining
    long getAuthenticationValidityPeriod();
    char* getVersion();
    //End of Framework Functions

    char* GetEnvironmentData(int index);
    bool SetEnvironmentThreshold(int index, const char *ret);
    
#ifdef __cplusplus
}
#endif
