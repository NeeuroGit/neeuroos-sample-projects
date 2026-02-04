//
//  IOSSDK.h
//  IOSBLESBFrame
//
//  Created by Serene Teng on 7/12/16.
//  Copyright © 2016 Serene Teng. All rights reserved.
//

#import <Foundation/Foundation.h>


typedef void(*DelegateSDK) (const char*);
typedef void(*voidDelegateBoolSDK) (bool);

@interface IOSSDK : NSObject

+ (void) Initialized;
+ (void) startScan;
+ (void) stopScan;
+ (void) startStopEEG:(bool)toStart;
+ (void) ClearBLEDeviceList;
+ (void) SendStartCommand:(const char*)deviceAddress :(int)type;
+ (void) SendStopCommand;
+ (void) DisconnectDevice:(const char*)deviceAddress :(int)type;
+ (void) ConnectDevice:(const char*)deviceAddress :(int)type;
+ (void) CancelConnection:(const char*)deviceAddress :(int)type;
+ (NSString*) grabMCUID:(const char*)deviceAddress;

/*!
 Send command to SenzeBand
 
 @param command A command string
    COMMAND_AC_LEADOFF
    COMMAND_DC_LEADOFF
    COMMAND_LIGHT_RED
    COMMAND_LIGHT_GREEN
    COMMAND_LIGHT_BLUE
    COMMAND_LIGHT_CYAN
    COMMAND_LIGHT_MAGENTA
    COMMAND_LIGHT_YELLOW
    COMMAND_STOP_RGB
    COMMAND_CMD_FA - Evoke event
    COMMAND_FW_VER - Get firmware version
    COMMAND_CAL_START - Calibration start
    COMMAND_CAL_STOP - Calibration stop
    COMMAND_START start - Sending eeg data
    COMMAND_STOP stop - Sending eeg data
    COMMAND_PPG_START - Start sending ppg data
    COMMAND_PPG_STOP - Stop sending ppg data
 */
+ (void) grabInputCommand:(const char*)command;

//Calling function in NFL_SenzeBand.m
+ (void)settingStatus;

//Framework calling for Delegate
//Calling Delegate in CBController.m for UnitySendMessage
+ (void) BLE_SetDeviceConnectStatus:(DelegateSDK) callbackNFL;
+ (void) BLE_SetDeviceConnectFailStatus:(DelegateSDK) callbackNFL;
+ (void) BLE_SetDeviceNotConnectStatus:(DelegateSDK) callbackNFL;
+ (void) BLE_SetDeviceList:(DelegateSDK) callbackNFL;
+ (void) BLE_completedBTInit:(DelegateSDK) callbackNFL;
+ (void) BLE_SetUpdateRSSI:(DelegateSDK) callbackNFL;
+ (void) BLE_bluetoothSwitchStatus:(DelegateSDK) callbackNFL;


//Calling Delegate in NFL_SenzeBand.m for UnitySendMessage
+ (void) FNFL_EEG_GetMentalStateData:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetBatteryLevelUpdate:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetAccXYZ:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetChannelStatus:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetGoodConnectionCheck:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetSignalReadyStatus:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetFreqTypePower:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetFreqTypePowerRaw:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetFreqTypePowerNorm:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetData:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetFilteredData:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetDirection:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetImpedance:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetSPO2AndHeartRate:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetCalibrationParameters:(DelegateSDK) callbackNFL;
+ (void) FNFL_PPG_GetRawData:(DelegateSDK) callbackNFL;
+ (void) FNFL_EEG_GetEvoke:(DelegateSDK) callbackNFL;

+ (void) SetEEGData:(int[]) eeg;
+ (void) SetAttentionModel
    :(double[])svData :(int)svCount
    :(double[])alphaData :(int)alCount
    :(double[])biasData :(int)bCount;
+ (NSString*) GetEnvironmentData:(int) index;
+ (bool) SetEnvironmentThreshold:(int) index :(NSString*) ret;

//End of Framework calling for Delegate


@end
