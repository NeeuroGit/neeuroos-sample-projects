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
    
    //Framework calling for Delegate
    //Calling Delegate for UnitySendMessage
    
    void BLE_SetDeviceConnectStatus(DelegateCBFUnity callbackUnity); //connected status
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
    void FNFL_EEG_GetFilteredData(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFreqTypePower(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFreqTypePowerRaw(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetFreqTypePowerNorm(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetDirection(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetImpedance(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetSPO2AndHeartRate(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetCalibrationParameters(DelegateCBFUnity callbackUnity);
    void FNFL_PPG_GetRawData(DelegateCBFUnity callbackUnity);
    void FNFL_EEG_GetEvoke(DelegateCBFUnity callbackUnity);

    //End of Framework calling for Delegate
    
    //Framework Functions
    void Initialized ();
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
    char* getVersion();
    //End of Framework Functions

    void SetEEGData(int data[]);
    void ChangeAttentionModel(double svData[], int svCount, double alphaData[], int alCount, double biasData[], int bCount);
    char* GetEnvironmentData(int index);
    bool SetEnvironmentThreshold(int index, const char *ret);
    
#ifdef __cplusplus
}
#endif
