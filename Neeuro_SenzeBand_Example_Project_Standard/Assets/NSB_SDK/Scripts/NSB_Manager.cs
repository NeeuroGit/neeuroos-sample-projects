//#define USE_SBv2;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;
using System.Timers;

#if (UNITY_STANDALONE_WIN || UNITY_EDITOR) && NET_4_6 && !UNITY_EDITOR_OSX
using NSB_SDK_WINDOWS;
#elif UNITY_ANDROID
using NSB_SDK_ANDROID;
#elif UNITY_IOS
using NSB_SDK_IOS;
#endif

/// <summary>
/// Receives EEG data via callbacks from Senzeband framework
/// </summary>
public class NSB_Manager : CallbackReceiver
{


	/*
		NSBM Init
			- false
			- true 	- Ready

		Scanning
			- false - Not scanning
			- true 	- Scanning

		Connection States
			- 0 	- Not connected
			- 1 	- Connecting
			- 2		- Connected, connectionStatus is updated in DeviceStatus callback
			- 3		- Connected, SB address is returned in ConnectionSucceed callback
			- 4		- Connected, MCUID is received
			Note:
			iOS devices 0->1->3->4
			Android devices 0->1->2->3->4

		EEG Started
			- false
			- true

		Authenticated
			- false	- Expired, not able to authenticate Developer code with Neeuro Server
			- true	- Valid, not expired

	*/


	//Developer info to update
	public string DEVELOPER_CODE = "1111222233334444";  //Replace this string with your developer code. This is used to authenticate with the NEEURO server.

	//Information stored for SB
	public List<string> listAvailableDevices = new List<string>();  //list of available SB addresses, from scanning

	private bool bIsInitCompleted = false;                          //state, if NSB systems are ready
	private bool bIsScanning = false;                               //state, if scanning
	private bool bEegStarted = false;                               //state, if EEG is being transmitted, received
	private bool bPpgStarted = false;                               //state, if PPG is being transmitted, received
	private int connectionState = 0;                                //state, on the connection
	private int prevConnectionState = 0;

	private string addressConnectingSB = string.Empty;                  //holds address of currently connecting SB
    //private string addressConnectedSB = string.Empty;                   //holds address of currently connected SB
	private List<string> connectedSBAddresses = new List<string>();
	private string mcuid = string.Empty;                                //holds the MCUID when available (of connected SB)
	private string version = string.Empty;
	private string connectionStatus = string.Empty;                 // "Not connected", "Connecting" or "Connected"  from NSB_BLE.getConnectingString(), getConnectedString(), getNotConnectedString
	private bool bluetoothStatus = false;                      //holds the OS's bluetooth status,  enabled or disabled
	private string batteryLevel = string.Empty;                     //holds the battery level

	private float[] mentalStateData = new float[4];
	private float[] accelerometerData = new float[9];
	private bool[] channelStatus = new bool[4];
	private bool goodBTConnection = false;
	private bool signalReady;
	private float[,] frequencyBandData = new float[4, 5];
	private float[] rawEEGData = new float[1000];
	private float[] filteredEEGData = new float[1000];
	//private float[] rawEEGData = new float[1000];
	private float[] eegImpedanceValues = new float[4];
	private List<int[]> rawPPGData = new List<int[]>();
	private bool authenticationResult = false;
	private string authenticationStatus = "";                       //holds the string for authentication status: "200", "No Intenet COnnection" "Invalid"
	private float[] gammaReading = new float[4]; 
	private float[] meanReading = new float[4];
	private float[] fiftysixtyReading = new float[4];   //50 60 Strength

	private string directionData = string.Empty;                  //holds direction of currently connecting SB
	private float[] calibrationParametersData = new float[3];

	public static NSB_Manager instance = null;

	private int SPO2 = 0;
	private int heartRate = 0;
	private bool ppgStatus = false;

	#region Event Callbacks
	/// <summary>
	/// Triggered when SenzeBand connection is successful.
	/// </summary>
	public UnityEvent connectionSuccessfulCallback = new UnityEvent();

	/// <summary>
	/// Triggered when SenzeBand connection is broken or disconnected.
	/// </summary>
	public UnityEvent connectionBrokenCallback = new UnityEvent();

	/// <summary>
	/// Triggered when the app fails to connect successfully to the SenzeBand. 
	/// </summary>
	public UnityEvent connectionFailedCallback = new UnityEvent();


	/// <summary>
	/// Triggered when the app receives EEG data from the SenzeBand. 
	/// </summary>
	public UnityEvent rawdataGrabbed = new UnityEvent();   //will be used to announce if new set of data has been fetched
	public UnityEvent ppgdataGrabbed = new UnityEvent();   //will be used to announce if new set of data has been fetched

	/// <summary>
	/// Triggered when the app receives the authentication update from plugin or every second after authentication is successful
	/// </summary>
	[Header("Triggered when the app receives the authentication update from plugin or every second after authentication is successful")]
	public UnityEvent authenticationUpdated = new UnityEvent();

	[Serializable]
    public class CommandAcknowledgedEvent : UnityEvent<string> { }
    [Header("Triggered if there is an acknowledged command")]
    public CommandAcknowledgedEvent onCommandAcknowledged = new CommandAcknowledgedEvent();     //will be used to an
    #endregion

    private void Awake()
	{
		if (instance != null)
		{
			Destroy(this.gameObject);
			if (transform.parent != null)
				Destroy(transform.parent.gameObject); //if this object is parented to something, destroy the parent
		}

		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(this.gameObject); //We need this object to persist throughout the lifetime of the program
			if (transform.parent != null)
				DontDestroyOnLoad(transform.parent.gameObject); //if this object is parented to something, don't destroy that gameobject too
		}
	}
	//UNITY mono functions
	// Use this for initialization
	new void Start()
	{
		base.Start();

		bIsInitCompleted = false;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		Init();
	}

	// Update is called once per frame
	System.DateTime lastClear = System.DateTime.Now;
	System.TimeSpan period = new System.TimeSpan(0, 0, 6);
	new void Update()
	{
		base.Update();

		if (bIsScanning)
		{
			if (System.DateTime.Now - lastClear > period)
			{
				lastClear = System.DateTime.Now;
				ClearList();
			}
		}

		//Tracking connection state;
		if (connectionState != prevConnectionState)
		{
			Debug.Log("NSB Connection state, " + prevConnectionState + " -> " + connectionState);
			prevConnectionState = connectionState;

			if (connectionState == 4)
			{
				connectionSuccessfulCallback.Invoke();
			}
		}
	}

	private void OnDestroy()
	{
		Shutdown();
	}


	/// <summary>
	/// Initialises Bluetooth, NSB library system, and also sets the callback functions when data is received or calculated.
	/// </summary>
	public void Init()
	{
		//Initialises Bluetooth, NSB libraries, and also sets the callback functions when hardwares' actions are completed.
		if (bIsInitCompleted == false)
		{
			//BLE - Bluetooth system controls
			NSB_BLE.instance.assignErrorLogDelegate(Log);
			NSB_BLE.instance.initializeBT(InitComplete, GetDeviceStatus, GetBTStatus, DEVELOPER_CODE);

			NSB_BLE.instance.assignAuthenticationStatusDelegate(grabAuthenticationStatus);
			NSB_BLE.instance.assignAuthenticationResultDelegate(grabAuthenticationResult);

			NSB_EEG.instance.assignBatteryStatus(GetBattery);
			NSB_BLE.instance.assignScanCallBack(FoundAvailableDevice);

			//EEG - EEG signal processing controls 
			NSB_EEG.instance.assignAttentionDelegate(grabAttention);
			NSB_EEG.instance.assignRelaxationDelegate(grabRelaxation);
			NSB_EEG.instance.assignMentalWorkloadDelegate(grabMentalWorkload);
			NSB_EEG.instance.assignAccDelegate(grabAccelerometer);
			NSB_EEG.instance.assignChannelDelegate(grabChannelStatus);
			NSB_EEG.instance.assignGoodConnectionCheckDelegate(grabGoodConnection);
			NSB_EEG.instance.assignSignalReadyStatusDelegate(grabSignalReady);
			NSB_EEG.instance.assignMCUIDDelegate(grabMCUID);
			NSB_EEG.instance.assignABDTDelegate(grabFrequencyBand);
			NSB_EEG.instance.assignRawDataDelegateFloat(grabRawEEG);
            NSB_EEG.instance.assignRawDataDelegate200ms(grabRawEEG200ms);
            NSB_EEG.instance.assignEnvironmentDataDelegate(grabEnvironmentData);

			NSB_EEG.instance.assignDirectionDelegate(grabDirection);
			NSB_EEG.instance.assignCalibrationParametersDelegate(grabCalibrationParameters);
			NSB_EEG.instance.assignSPO2AndHeartRateDelegate(grabSPO2AndHeartRate);

			NSB_EEG.instance.assignPPGDataDelegate(grabRawPPG);
			NSB_EEG.instance.assignEEGImpedanceDelegate(grabEEGImpedance);		

			NSB_EEG.instance.assignSenzeBandVersionDelegate(grabSenzeBandVersion);
			NSB_EEG.instance.assignFilteredDataDelegate(grabFilteredEEG);

            NSB_EEG.instance.assignCommandACKDelegate(grabCommandACK);

            SetScanning(true);
		}
	}

	/// <summary>
	/// Releases resources under NSB library system
	/// </summary>
	public void Shutdown()
	{
		NSB_BLE.instance.shutdownBT();
	}

	/// <summary>
	///  
	/// </summary>
	/// <returns>Boolean value stating whether the initialisation of NSB library system is completed</returns>
	public bool IsInitCompleted()
	{

		return bIsInitCompleted;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>Boolean value stating whether Bluetooth is enabled or not</returns>
	public bool IsBluetoothEnabled()
	{
		return bluetoothStatus;
	}

	/// <summary>
	///  
	/// </summary>
	/// <returns>Boolean value stating whether the app is scanning for available SB devices or not</returns>
	public bool IsScanning()
	{
		return bIsScanning;
	}

	/// <summary>
	/// Turns on or off scanning for SB device.
	/// </summary>
	/// <param name="state">Scanning state ON or OFF</param>
	public void SetScanning(bool state)
	{
		if (bIsScanning)
		{
			ClearList();
		}
		bIsScanning = state;

		Debug.Log("NSB SetScanning " + state);
		NSB_BLE.instance.startStopScanning(state);
	}

	/// <summary>
	///  
	/// </summary>
	/// <returns>List of available SB devices found from scanning </returns>
	public List<string> GetScannedSenzeBandList()
	{
		return listAvailableDevices.Distinct().ToList();
	}

	/// <summary>
	/// Starts connection process to SB device
	/// </summary>
	/// <param name="address">Address of device to connect to</param>
	/// <returns>Success or failure of connection attempt</returns>
	public bool ConnectSB(string address)
	{
		//Starts connection process to SB device
		Debug.Log("NSB ConnectSB " + address);
		if (NSB_BLE.instance.connectBT(address, ConnectionSucceed, ConnectionBroken, ConnectionFailed) == false)
		{
			//Unable to process this function
			Debug.Log("NSB Can't start connect");
			return false;
		}

		addressConnectingSB = address;  //TEMP
		Debug.Log("ConnectSB addressConnectingSB=" + addressConnectingSB);
		ClearList();
		return true;
	}

	/// <summary>
	/// sends specific commands to plugin
	/// possible string commands:
	/// "COMMAND_START" - for starting EEG
	/// "COMMAND_STOP" - for stopping EEG
	/// "COMMAND_AC_LEADOFF" - for turning ON impedance check mode
	/// "COMMAND_DC_LEADOFF" - for turning OFF impedance check mode
	/// "COMMAND_LIGHT_RED" - sets SenzeBand device light to red color
	/// "COMMAND_LIGHT_GREEN" - sets SenzeBand device light to green color
	/// "COMMAND_LIGHT_BLUE" - sets SenzeBand device light to blue color
	/// "COMMAND_LIGHT_CYAN" - sets SenzeBand device light to cyan color
	/// "COMMAND_LIGHT_MAGENTA" - sets SenzeBand device light to magenta color
	/// "COMMAND_LIGHT_YELLOW" - sets SenzeBand device light to yellow color
	/// "COMMAND_STOP_RGB" - stops light commands 
	/// "COMMAND_FW_VER" - command for getting the firmware version of SenzeBand device
	/// "COMMAND_CAL_START" - start calibration for acceleration and orientation to improve accuracy
	/// "COMMAND_CAL_STOP" - stop calibration
	/// </summary>
	public void sendCommand(string command)
	{
		NSB_BLE.instance.sendCommand(command);
	}

	/// <summary>
	/// Disconnects a connected SB device
	/// </summary>
	public void DisconnectSB()
	{

		//Disconnects a connected SB device
		Debug.Log("NSB DisconnectSB");
		if (connectedSBAddresses.Count > 0)
		{
			NSB_BLE.instance.disconnectBT(connectedSBAddresses[0]);
			connectedSBAddresses.RemoveAt(0);
			//connectionBrokenCallback.Invoke();

		}
		else if (addressConnectingSB != string.Empty)
		{
			NSB_BLE.instance.disconnectBT(addressConnectingSB);
			connectionFailedCallback.Invoke();
		}
		
	}

	/// <summary>
	/// Tries to authenticate the SDK. Requires an internet connection
	/// </summary>
	public void AuthenticateUser()
	{
		BLEController.getInstance().authenticateUser();
	}

	/// <summary>
	/// Determines the state of the NSB connection handling.
	/// </summary>
	/// <returns>Connection state: 0 - Not connected, 1 - Connecting, 2 - Connected (connectionStatus is updated in DeviceStatus callback), 3 - Connected, SB address is returned in ConnectionSucceed callback, 4 - Connected (MCUID is received)</returns>
	public int GetConnectionState()
	{
		connectionState = 0;

		if (connectionStatus == NSB_BLE.instance.getNotConnectedString())
			connectionState = 0;
		else if (connectionStatus == NSB_BLE.instance.getConnectingString())
			connectionState = 1;
		else if (connectionStatus == NSB_BLE.instance.getConnectedString())
		{
			if (connectedSBAddresses.Count < 1)
				connectionState = 2;
			else
			{
				if (mcuid == string.Empty)
					connectionState = 3;
				else
					connectionState = 4;
			}
		}
		return connectionState;
	}

	/// <summary>
	/// Determines address of connected SenzeBand device
	/// </summary>
	/// <returns>Bluetooth identifier address of the connected SenzeBand device</returns>
	public string GetConnectedSBAddress()
	{
		return connectedSBAddresses.Count > 0 ? connectedSBAddresses[0] : "";
	}

	/// <summary>
	/// MCUID is a 32 character string unique to every SB device. 
	/// This can be retrieved only after connection
	/// </summary>
	/// <returns>MCUID of the SB device</returns>
	public string GetConnectedSBMCUID()
	{
		return mcuid;
	}

	/// <summary>
	/// Determines whether system is receiving EEG data from SenzeBand
	/// </summary>
	/// <returns>TRUE means that the NSB library system is receiving EEG and other data from the SB device, FALSE if data transfer is switched OFF.</returns>
	public bool GetReceiveEEGState()
	{
		return bEegStarted;
	}

	/// <summary>
	/// Sets the NSB library system to enable or disable receiving EEG and other data from the SB device
	/// </summary>
	/// <param name="send">Sending state ON or OFF</param>
	public void SetReceiveEEG(bool send)
	{
		Debug.Log("NSB Set receive EEG " + send);
		if (send)
		{
			NSB_EEG.instance.EEG_Start();
			bEegStarted = true;
		}
		else
		{
			NSB_EEG.instance.EEG_Stop();
			bEegStarted = false;
		}
	}

	/// <summary>
	/// Determines whether system is receiving PPG data from SenzeBand
	/// </summary>
	/// <returns>TRUE means that the system is receiving PPG data from SB device, FALSE if PPG data transfer is switched OFF.</returns>
	public bool GetReceivePPGState()
	{
		return bPpgStarted;
	}

	/// <summary>
	/// Sets the NSB library system to enable or disable receiving PPG and other data from the SB device
	/// </summary>
	/// <param name="send">Sending state ON or OFF</param>
	public void SetReceivePPG(bool send)
	{
		Debug.Log("NSB Set receive PPG " + send);
		if (send)
		{
			bPpgStarted = true;
		}
		else
		{
			bPpgStarted = false;
		}
	}

	float GT = 0.1f, FST = 0.08f;
	/// <summary>
	/// Only for SenzeBand 1, not applicable for SenzeBand 2
	/// Sets gamma threshold for filtering useful data
	/// </summary>
	/// <param name="threshold">Set threshold limit for what power is considered high interference</param>
	public void SetGammaThreshold(float threshold)
	{
		GT = threshold;
		EEGController.getInstance().SetEnvironmentThreshold(GT.ToString() + "," + FST.ToString());
	}

	/// <summary>
	/// Only for SenzeBand 1, not applicable for SenzeBand 2
	/// Sets 50-60hz threshold for filtering useful data
	/// </summary>
	/// <param name="threshold">Set threshold limit for what power is considered high interference</param>
	public void Set5060Threshold(float threshold)
	{
		FST = threshold;
		EEGController.getInstance().SetEnvironmentThreshold(GT.ToString() + "," + FST.ToString());
	}

	/// <summary>
	/// Only for SenzeBand 1, not applicable for SenzeBand 2
	/// Determines the 50-60Hz noise signal - This comes from power sources.
	/// </summary>
	/// <param name="channel">The channel to read 50-60hz power from</param>
	/// <returns>Current value of 50-60hz power from selected channel</returns>
	public float GetFiftySixtyReading(int channel)
	{
		if ((channel >= 0 && channel < 4))
			return fiftysixtyReading[channel];
		else
			return 0f;
	}

	/// <summary>
	/// Only for SenzeBand 1, not applicable for SenzeBand 2
	/// Determines frequency signal power - This is present in naturally-occuring ambient noise
	/// </summary>
	/// <param name="channel">The channel to read gamma power from</param>
	/// <returns>Current value of gamma power from selected channel</returns>
	public float GetGammaReading(int channel)
	{
		if ((channel >= 0 && channel < 4))
			return gammaReading[channel];
		else
			return 0f;
	}

	/// <summary>
	/// Only for SenzeBand 1, not applicable for SenzeBand 2
	/// Determines average signal power
	/// </summary>
	/// <param name="channel">The channel to read mean power from</param>
	/// <returns>Current value of mean power from selected channel</returns>
	public float GetMeanReading(int channel)
	{
		if ((channel >= 0 && channel < 4))
			return meanReading[channel];
		else
			return 0f;
	}


	/// <summary>
	/// Returns the accelerometer values.
	/// </summary>
	/// <param name="dimension">0 - X accel, 1 - Y accel, 2 - Z accel, 3 - X rotation, 4 - Y rotation, 5 - Z rotation, 6 - X magnitude, 7 - Y magnitude, 8 - Z magnitude</param>
	/// <returns>Value of accel/rotation/magnitude depending on dimension index on parameter</returns>
	public float GetAccel(int dimension)
	{
		if (dimension < 9)
			return accelerometerData[dimension];
		else
			return 999; //Error; dimension cover X, Y, Z only.
	}

	/// <summary>
	/// This is calculated from the latest set of EEG data received from the SB device.
	/// </summary>
	/// <returns>Attention level value, range of 0 - 1.</returns>
	public float GetAttention()
	{
		return mentalStateData[1];
	}

	/// <summary>
	/// This is calculated from the latest set of EEG data received from the SB device.
	/// </summary>
	/// <returns>Relaxation level value, range of 0 - 1.</returns>
	public float GetRelaxation()
	{
		return mentalStateData[0];
	}

	/// <summary>
	/// This is calculated from the latest set of EEG data received from the SB device.
	/// </summary>
	/// <returns>Mental Workload value, range of 0 - 1.</returns>
	public float GetMentalWL()
	{
		return Mathf.Clamp(mentalStateData[2], 0f, 1f);
	}

	/// <summary>
	/// Determines if the signal from each sensor on SB device is receiving EEG signal
	/// </summary>
	/// <param name="channel">Index of SenzeBand channel: 0 = center-left, 1 = center-right, 2 = right, 3 = left</param>
	/// <returns>TRUE if the sensor channel in parameter is receiving EEG signal</returns>
	public bool GetChannelStatus(int channel)
	{
		if (channel < 4)
			return channelStatus[channel];
		else
			return false;
	}

	/// <summary>
	/// Determines validity of data received from SenzeBand.
	/// Signal noise from body movement, or insufficient skin contact at the sensor electrode give poor quality signal. 
	/// Under this condition, the results from signal processing may not be accurate.
	/// </summary>
	/// <returns>TRUE if the EEG signal received is acceptable/valid </returns>
	public bool GetSignalReady()
	{
		return signalReady;
	}

	/// <summary>
	/// Determines if bluetooth signal is valid.
	/// Interference from other Bluetooth signal emitters can lead to data loss.
	/// </summary>
	/// <returns>TRUE if the Bluetooth connection is good.</returns>
	public bool GetGoodBTConnection()
	{
		return goodBTConnection;
	}

	/// <summary>
	/// Determines the battery level of connected SenzeBand device
	/// </summary>
	/// <returns>Value of battery level, range decimal number from 0 to 1</returns>
	public string GetConnectedSBBattery()
	{
		return batteryLevel;
	}

	/// <summary>
	/// This is calculated from the latest set of EEG data received from the SB device.
	/// </summary>
	/// <param name="channel">Index of SenzeBand channel: 0 = center-left, 1 = center-right, 2 = right, 3 = left</param>
	/// <param name="band">Index of frequency band: 0 = delta, 1 = theta, 2 = alpha, 3 = beta, 4 = gamma</param>
	/// <returns>Value of frequency band on channel</returns>
	public float GetFrequencyBand(int channel, int band)
	{
		//returns a float of the power spectral density(PSD). ie. 0.3.
		//the sum of the PSD for all the 5 bands should be 1.
		if ((band >= 0 && band < 5) &&
		   (channel >= 0 && channel < 4))
			return frequencyBandData[channel, band];
		else
			return 0f;
	}

	/// <summary>
	/// Returns received raw unprocessed EEG data
	/// </summary>
	/// <returns>Array of Raw unprocessed EEG data for all channels (0 to 249 = center-left, 250 to 499 = center-right, 500 to 749 = right, 750 to 999 = left</returns>
	public float[] GetRawEEG()
	{
		return rawEEGData;
	}


	/// <summary>
	/// Returns received cleaned up processed EEG data
	/// </summary>
	/// <returns>Array of cleaned up processed EEG data for all channels (0 to 249 = center-left, 250 to 499 = center-right, 500 to 749 = right, 750 to 999 = left</returns>
	public float[] GetFilteredEEG()
	{
		return filteredEEGData;
	}

	/// <summary>
	/// EEG impedance helps to verify if the signal quality receiving from the 4 contact points/electrodes are good.
	/// Example, if there is makeup or some other impurities on the skin, this impedance values will be much higher,
	/// due to the contribution of more resistance between the electrodes and skin surface, and the EEG signals will not be good/accurate.
	/// Good impedance range is below 800kohms
	/// </summary>
	/// <returns>Received EEG impedance</returns>
	public float[] GetEEGImpedance()
	{
		return eegImpedanceValues;
	}

	/// <summary>
	/// For determining authentication validity
	/// </summary>
	/// <returns>Validity of authentication. If true, authentication is successful, then EEG data can be received from the Senzeband</returns>
	public bool GetAuthenticationResult()
	{
		return authenticationResult;
	}

	/// <summary>
	/// Determines authentication status description
	/// </summary>
	/// <returns>Human readable string describing auth status. i.e No Internet or Devcode not specified</returns>
	public string GetAuthenticationStatus()
	{
		return authenticationStatus;
	}

	//CALLBACKS to be sent to NSB_BLE and NSB_EEG
	void Log(string error)
	{
		Debug.Log("NSB SDK LOG: " + error);
	}

	/// <summary>
	/// </summary>
	/// <param name="authenStatus">Authentication status from Senzeband plugin</param>
	void grabAuthenticationStatus(string authenStatus)
	{
		//"200" or "Successful" - authentication is valid
		//"No internet connection	- no connection to server, authentication status will fail
		//"Unsuccessful" or others - authentication has expired 
		Debug.Log("NSB Authentication status is " + authenStatus);
		authenticationStatus = authenStatus;
		authenticationUpdated?.Invoke();
	}


	/// <summary>
	/// </summary>
	/// <param name="authenResult">Authentication result from Senzeband plugin</param>
	Timer authLogic = null;
	void grabAuthenticationResult(bool authenResult)
	{
		//TRUE - Within valid authentication period
		//FALSE - Not within valid authentication period
		Debug.Log("NSB Authentication result is " + authenResult);
		authenticationResult = authenResult;

        if (authenResult)
        {
			if (authLogic != null) authLogic.Stop();

			authLogic = new Timer();
			authLogic.Interval = 1000;
			authLogic.Elapsed += (object source, ElapsedEventArgs e) =>
			{
				UpdateAuthPeriod();
			};
			authLogic.Start();
        }

		authenticationUpdated?.Invoke();
	}

	public long AuthPeriod { set; get; }
	/// <summary>
	/// Tracks how many seconds left until authentication validity expires
	/// </summary>
	void UpdateAuthPeriod()
	{
		if (SeparateThread)
		{
			System.Action func = UpdateAuthPeriod;
			QueueInvoke(func);
			return;
		}

		AuthPeriod = BLEController.getInstance().GetAuthenticationValidityPeriod();
		authenticationUpdated?.Invoke();
		Debug.Log("I am on UpdateAuthPeriod");
	}

	/// <summary>
	/// Callback when NSB SDK Library initialisation is complete.
	/// </summary>
	void InitComplete()
	{
		Debug.Log("InitComplete addressConnectingSB=" + addressConnectingSB);

		//reset all data
		bIsScanning = true;
		addressConnectingSB = string.Empty;
		connectedSBAddresses.Clear();
		mcuid = string.Empty;
		connectionStatus = string.Empty;
		bEegStarted = false;
		bPpgStarted = false;
		connectionState = 0;

		ClearList();
		bIsInitCompleted = true;

	}

	/// <summary>
	/// Updates the status of the connection of the SenzeBand device
	/// </summary>
	/// <param name="status"></param>
	void GetDeviceStatus(string status)
	{
		//Updates the status of the connection of the SenzeBand device
		Debug.Log("NSB SB connection status changed: " + status + " for address: " + addressConnectingSB);
		connectionStatus = status;
		if (status == NSB_BLE.instance.getConnectedString())
		{
			
			ConnectionSucceed(addressConnectingSB);
			//connected!		also see ConnectionSucceed()
		}
		else if (status == NSB_BLE.instance.getNotConnectedString())
		{
			//disconnected!		also see ConnectionFailed()
		}
		else if (status == NSB_BLE.instance.getConnectingString())
		{
			//connecting!
		}
	}

	/// <summary>
	/// Updates the status of Bluetooth settings
	/// </summary>
	/// <param name="status"></param>
	public bool BTStatusFetched { set; get; }
	void GetBTStatus(bool status)
	{
		BTStatusFetched = true;
		//Updates the status of Bluetooth settings
		Debug.Log("NSB BT setting changed: " + status);
		bluetoothStatus = status;

		//To reset the available device list when BT state is changed
		ClearList();
		//If BT is turned off, disconnect existing connections - reset.
		if (!status)
		{
			DisconnectSB();
		}

	}

	/// <summary>
	/// Updates the battery level
	/// </summary>
	/// <param name="battery"></param>
	void GetBattery(string battery)
	{
		//Updates the battery level
		batteryLevel = battery;

		Debug.Log("Battery level: " + battery);
	}

	/// <summary>
	/// During scanning, an available SenzeBand is found
	/// </summary>
	/// <param name="address"></param>
	/// <param name="name"></param>
	void FoundAvailableDevice(string address, string name)
	{
		//During scanning, an available SenzeBand is found
		Debug.Log("NSB found available device : " + address + " ; list of " + listAvailableDevices.Count);
		string tempString = string.Copy(address);
		foreach (string t in listAvailableDevices)
		{
			if (string.Equals(t, tempString))
			{
				//It is a repeated SB identity
				Debug.Log("NSB repeated device ID.  Not adding to list");
				return;
			}
		}
		listAvailableDevices.Add(tempString);

	}

	/// <summary>
	/// Gets from NSB to store the MCU ID of the SB device
	/// </summary>
	/// <returns></returns>
	IEnumerator co_PullMCUID()
    {
        yield return new WaitForSeconds(1.0f);
		Debug.Log("co_PullMCUID connectionStatus=" + connectionStatus + " addressConnectingSB=" + addressConnectingSB + " mcuid=" + mcuid);
		if (connectionStatus == NSB_BLE.instance.getConnectedString() && addressConnectingSB != string.Empty)
		{

			while (mcuid == string.Empty)
			{
				yield return new WaitForSeconds(0.5f);

				//Gets from NSB to store the MCU ID of the SB device
				mcuid = NSB_BLE.instance.getMCUID(addressConnectingSB);
				if (mcuid.Length < 8)
					mcuid = string.Empty;
				else
					addressConnectingSB = string.Empty;

				Debug.Log("NSB Pull mcuid: " + mcuid);

				if (connectionStatus != NSB_BLE.instance.getConnectedString() || addressConnectingSB == string.Empty)
					break;
			}
		}
	}

	/// <summary>
	/// Callback for when the connection process is successful - SenzeBand is connected.
	/// </summary>
	/// <param name="address"></param>
	void ConnectionSucceed(string address)
    {
		if (SeparateThread)
		{
			System.Action<string> func = ConnectionSucceed;
			QueueInvoke(func, address);
			return;
		}

		//Callback for when the connection process is successful - SenzeBand is connected.
		Debug.Log("NSB connection succeed : " + address);
		Debug.Log("ConnectionSucceed addressConnectingSB=" + addressConnectingSB);
		connectedSBAddresses.Add(address);
		
		StartCoroutine(co_PullMCUID());
        
        connectionSuccessfulCallback.Invoke();
    }

	/// <summary>
	/// Callback after a successful connection, when the connection is broken
	/// </summary>
	/// <param name="address"></param>
	void ConnectionBroken(string address)
    {
		if (SeparateThread)
		{
			System.Action<string> func = ConnectionBroken;
			QueueInvoke(func, address);
			return;
		}

		//Callback for when the connection process fails - SenzeBand is NOT connected.
		//OR after a successful connection, when the connection is broken
		Debug.Log("NSB connection broken : " + address);
		Debug.Log("ConnectionBroken addressConnectingSB=" + addressConnectingSB);
		connectedSBAddresses.Remove(address);
		addressConnectingSB = string.Empty;
        mcuid = string.Empty;
        bEegStarted = false;
		bPpgStarted = false;
		ClearList();

        connectionBrokenCallback.Invoke();
    }

	/// <summary>
	/// Callback for when the connection process fails - SenzeBand is NOT connected
	/// </summary>
	/// <param name="address"></param>
	void ConnectionFailed(string address)
    {
		if (SeparateThread)
		{
			System.Action<string> func = ConnectionFailed;
			QueueInvoke(func, address);
			return;
		}

		//Callback for when the connection process fails - SenzeBand is NOT connected.
		//OR after a successful connection, when the connection is broken
		Debug.Log("NSB connection failed : " + address);
		Debug.Log("ConnectionFailed addressConnectingSB=" + addressConnectingSB);
		addressConnectingSB = string.Empty;
        mcuid = string.Empty;
        bEegStarted = false;
		bPpgStarted = false;
		ClearList();
        
        connectionFailedCallback.Invoke();
    }

	/// <summary>
	/// Receives attention data from plugin
	/// </summary>
	/// <param name="attention"></param>
	void grabAttention(float attention)
	{
		mentalStateData[1] = attention;
	}

	/// <summary>
	/// Receives relaxation data from plugin
	/// </summary>
	/// <param name="relaxation"></param>
	void grabRelaxation(float relaxation)
	{
		mentalStateData[0] = relaxation;
	}

	/// <summary>
	/// Receives mental workload data from plugin
	/// </summary>
	/// <param name="mentalWorkload"></param>
	void grabMentalWorkload(float mentalWorkload)
	{
		mentalStateData[2] = mentalWorkload;
	}

	/// <summary>
	/// Receives accelerometer data from plugin
	/// </summary>
	/// <param name="acc"></param>
	void grabAccelerometer(float[] acc)
	{
		if (acc.Length == accelerometerData.Length)
		{
			accelerometerData = acc;
		}
		else
			Debug.Log("NSB accelerometer has incorrect data");
	}

	/// <summary>
	/// Receives channel status data from plugin
	/// </summary>
	/// <param name="chnStatus"></param>
	void grabChannelStatus(bool[] chnStatus)
	{
		if (chnStatus.Length == 4)
		{
			channelStatus[0] = chnStatus[0];
			channelStatus[1] = chnStatus[1];
			channelStatus[2] = chnStatus[2];
			channelStatus[3] = chnStatus[3];

			Debug.LogFormat("Received channel status: {0} {1} {2} {3}", channelStatus[0], channelStatus[1], channelStatus[2], channelStatus[3]);
		}
		else
			Debug.Log("NSB channelStatus has incorrect data");

		var data = NSB_EEG.instance.GetEnvironmentData();

		var parameters = data.Split(',');
		if (parameters.Length >= 15)
		{
			float[] gamma = new float[4];
			float[] mean = new float[4];
			float[] fiftysixty = new float[4];

			gamma[0] = gamma[1] = gamma[2] = gamma[3] = 0;
			try
			{
				gamma[0] = float.Parse(parameters[1]);
				gamma[1] = float.Parse(parameters[2]);
				gamma[2] = float.Parse(parameters[3]);
				gamma[3] = float.Parse(parameters[4]);
			}
			catch(Exception ex)
			{
				Debug.LogException(ex);
			}

			mean[0] = mean[1] = mean[2] = mean[3] = 0;
			try
			{
				mean[0] = float.Parse(parameters[6]);
				mean[1] = float.Parse(parameters[7]);
				mean[2] = float.Parse(parameters[8]);
				mean[3] = float.Parse(parameters[9]);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			fiftysixty[0] = fiftysixty[1] = fiftysixty[2] = fiftysixty[3] = 0;
			try
			{
				fiftysixty[0] = float.Parse(parameters[11]);
				fiftysixty[1] = float.Parse(parameters[12]);
				fiftysixty[2] = float.Parse(parameters[13]);
				fiftysixty[3] = float.Parse(parameters[14]);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}

			gammaReading = gamma;
			meanReading = mean;
			fiftysixtyReading = fiftysixty;
		}
	}

	/// <summary>
	/// Receives updated connection check from plugin.
	/// Returns if the Bluetooth connection is good.
	/// Interference from other Bluetooth signal emitters can lead to data loss 
	/// </summary>
	/// <param name="goodConnection"></param>
	void grabGoodConnection(bool goodConnection)
	{
		goodBTConnection = goodConnection;
	}

	/// <summary>
	/// Receives signal ready data from plugin
	/// </summary>
	/// <param name="_signalReady"></param>
	void grabSignalReady(bool _signalReady)
	{
		signalReady = _signalReady;
	}

	/// <summary>
	/// Receives MCUID data from plugin
	/// </summary>
	/// <param name="data"></param>
	void grabMCUID(string data)
	{
		mcuid = data;
	}

	/// <summary>
	/// Receives Frequency band data from plugin
	/// </summary>
	/// <param name="frequencyBand"></param>
	void grabFrequencyBand(float[,] frequencyBand)
	{
		//check if returned data is correct length
		if (frequencyBand.Length != frequencyBandData.Length)
			return;

		for (int j = 0; j < frequencyBandData.GetLength(0); ++j)
		{
			for (int i = 0; i < frequencyBandData.GetLength(1); ++i)
				frequencyBandData[j, i] = frequencyBand[j, i];
		}
	}

	/// <summary>
	/// Receives EEG Impedance data from plugin
	/// This helps to verify if the signal quality receiving from the 4 contact points/electrodes are good.
	/// Example, if there is makeup or some other impurities on the skin, this impedance values will be much higher,
	/// due to the contribution of more resistance between the electrodes and skin surface, and the EEG signals will not be good/accurate.
	/// Good impedance range is below 800kohms
	/// </summary>
	/// <param name="impedance"></param>
	void grabEEGImpedance(float[] impedance)
	{
		Debug.Log("grabEEGImpedance " + impedance[0] + " " + impedance[1] + " " + impedance[2] + " " + impedance[3]);
		for (int i = 0; i < eegImpedanceValues.Length; i++)
		{
			eegImpedanceValues[i] = impedance[i];

		}
	}

	/// <summary>
	/// Receives PPG data from plugin
	/// </summary>
	/// <param name="rawPPG"></param>
	void grabRawPPG(int[] rawPPG)
	{
		if (SeparateThread)
		{
			System.Action<int[]> func = grabRawPPG;
			QueueInvoke(func, rawPPG);
			return;
		}

		Debug.Log("Received PPG data: rawPPG[0]=" + rawPPG[0] + " rawPPG[1]=" + rawPPG[1]);

		int PPG_DATA_PER_SECOND = 1;
		rawPPGData.Add(rawPPG);
		if (rawPPGData.Count >= PPG_DATA_PER_SECOND)
		{
			rawPPGData.Skip(rawPPGData.Count - PPG_DATA_PER_SECOND);
		}

		if (bPpgStarted && ppgdataGrabbed != null) //only trigger event if eeg receiving is ON
			ppgdataGrabbed?.Invoke();    //announce that new set of data has been fetched			 

		if (ppgdataGrabbed == null)
			Debug.Log("ppgdataGrabbed not assigned");
	}


	/// <summary>
	/// Receives raw unprocessed EEG data from plugin
	/// Raw EEG received is integer values where 1 unit = 1 * 0.61 microVolt
	/// </summary>
	/// <param name="rawEEG"></param>
	void grabRawEEG(float[] rawEEG)
	{
		if (SeparateThread)
		{
			System.Action<float[]> func = grabRawEEG;
			QueueInvoke(func, rawEEG);
			return;
		}

		Debug.Log("Received EEG data");
		Debug.Log("rawEEG: " + string.Join(" ", rawEEG));

		//Raw EEG received is integer values where 1 unit = 1 * 0.61 microVolt
		if (rawEEG.Length == rawEEGData.Length)
			rawEEGData = (float[])rawEEG.Clone();
	}

    /// <summary>
    /// Receives EEG data from plugin every 200ms
    /// </summary>
    /// <param name="rawEEG"></param>
    void grabRawEEG200ms(float[] rawEEG)
    {
        if (SeparateThread)
        {
            System.Action<float[]> func = grabRawEEG200ms;
            QueueInvoke(func, rawEEG);
            return;
        }

        Debug.Log("Received EEG data 200ms");
        Debug.Log("rawEEG: " + string.Join(" ", rawEEG));
        
    }

    /// <summary>
    /// Receives EnvironmentData (gamma noise and 50-60Hz noise) from plugin
    /// </summary>
    /// <param name="data"></param>
    void grabEnvironmentData(string data)
	{
		var parameters = data.Split(',');
		if (parameters.Length >= 15)
		{
			float[] gamma = new float[4];
			float[] mean = new float[4];
			float[] fiftysixty = new float[4];

			gamma[0] = float.Parse(parameters[1]);
			gamma[1] = float.Parse(parameters[2]);
			gamma[2] = float.Parse(parameters[3]);
			gamma[3] = float.Parse(parameters[4]);

			mean[0] = float.Parse(parameters[6]);
			mean[1] = float.Parse(parameters[7]);
			mean[2] = float.Parse(parameters[8]);
			mean[3] = float.Parse(parameters[9]);

			fiftysixty[0] = float.Parse(parameters[11]);
			fiftysixty[1] = float.Parse(parameters[12]);
			fiftysixty[2] = float.Parse(parameters[13]);
			fiftysixty[3] = float.Parse(parameters[14]);

			gammaReading = gamma;
			meanReading = mean;
			fiftysixtyReading = fiftysixty;

			Debug.LogFormat("Received environment data\n gamma: {0} {1} {2} {3}\n mean: {4} {5} {6} {7}\n 5060: {8} {9} {10} {11}",
			gamma[0], gamma[1], gamma[2], gamma[3],
			mean[0], mean[1], mean[2], mean[3],
			fiftysixty[0], fiftysixty[1], fiftysixty[2], fiftysixty[3]);
		}
	}

	//Other Internal Support functions

	/// <summary>
	/// Clearing the list of SenzeBands available for connection. Not if it is in Connecting state.
	/// </summary>
	public void ClearList()
	{
		//Clearing the list of SenzeBands available for connection. Not if it is in Connecting state.
		if (connectionState != 1)
		{
			Debug.Log("NSB Clearlist ");
			listAvailableDevices.Clear();
			NSB_BLE.instance.clearList();
		}
	}

	public List<int[]> GetRawPPG()
	{
		List<int[]> result = new List<int[]>();
		for (int i = 0; i < rawPPGData.Count; i++)
		{
			result.Add(rawPPGData[i]);
		}
		rawPPGData.Clear();
		Debug.Log("GetRawPPG result.Count = " + result.Count);
		return result;
	}

	/// <summary>
	/// Returns latest PPG data
	/// </summary>
	/// <returns></returns>
	public int[] GetLatestPPG()
	{
		int[] result = new int[2];
		if (rawPPGData.Count > 0)
		{

		}
		Debug.Log("GetLatestPPG result = " + result[0] + " " + result[1]);
		return result;
	}


	/// <summary>
	/// </summary>
	/// <returns>Received direction data</returns>
	public string GetDirection(){
		return directionData;
	}

	/// <summary>
	/// Receives Direction string from plugin
	/// </summary>
	/// <param name="direction"></param>
	void grabDirection(string direction){
		Debug.Log("grabDirection direction=" + direction);
		directionData = direction;
	}



	/// <summary>
	/// Receives Calibration Parameters from plugin
	/// </summary>
	/// <param name="calibrationParameters"></param>
	void grabCalibrationParameters(float[] calibrationParameters){
		if (calibrationParameters.Length == calibrationParametersData.Length)
			calibrationParametersData = (float[])calibrationParameters.Clone();
		Debug.Log("grabCalibrationParameters xgain=" + calibrationParametersData[0] + " ygain=" + calibrationParametersData[1] + " zgain=" + calibrationParametersData[2]);
	}

	/// <summary>
	/// For calibrating acceleration and orientation to improve accuracy
	/// </summary>
	/// <returns>Received calibration parameters</returns>
	public float[] GetCalibrationParameters(){
		return calibrationParametersData;
	}


	/// <summary>
	/// Receives SPO2 and Heart Rate from plugin
	/// </summary>
	/// <param name="data">Contains SPO2 and heartrate data</param>
	void grabSPO2AndHeartRate(int[] data)
    {
		ppgStatus = data[2] != 0;
		Debug.Log("grabSPO2AndHeartRate raw SPO2=" + data[0] + " raw Heartrate=" + data[1] + " HeartRateDetected=" + data[2]);
		if (ppgStatus)
		{
			if (data.Length == 5)
			{
				SPO2 = data[3];
				heartRate = data[4];
				Debug.Log("grabSPO2AndHeartRate SPO2=" + data[3] + " HeartRate" + data[4] + " HeartRateDetected=" + data[2]);
			}
			else
			{
				SPO2 = data[0];
				heartRate = data[1];
			}
		}
		else //if ppg status is FALSE, then it is invalid
		{
			//make SPO2 and heartrate to 0 since ppg status is invalid
			SPO2 = 0;
			heartRate = 0;
		}
	}


	/// <summary>
	/// SPO2 is the Oxygen Level in bloodstream.
	/// </summary>
	/// <returns>Received SPO2 data, ranges from 0 to 100</returns>
	public int GetSPO2()
	{
		return SPO2;
	}

	/// <summary>
	///
	/// </summary>
	/// <returns>Received heart rate data in BPM</returns>
	public int GetHeartRate()
	{
		return heartRate;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns>TRUE if heartrate and spo2 is VALID/detected properly</returns>
	public bool GetPPGStatus()
	{
		return ppgStatus;
	}

	/// <summary>
	/// Receives SenzeBand version data from plugin
	/// </summary>
	/// <param name="data"></param>
	void grabSenzeBandVersion(string data)
	{
		Debug.Log("grabSenzeBandVersion version=" + data);
		version = data;
	}

	/// <summary>
	/// Receives Filtered (cleaned up and processed) EEG data from plugin
	/// </summary>
	/// <param name="filteredEEG"></param>
	void grabFilteredEEG(float[] filteredEEG)
	{
		if (SeparateThread)
		{
			System.Action<float[]> func = grabFilteredEEG;
			QueueInvoke(func, filteredEEG);
			return;
		}

		Debug.Log("Received Filtered EEG data");

		Debug.Log("filteredEEG: " + string.Join(" ", filteredEEG));


		//Raw EEG received is integer values where 1 unit = 1 * 0.61 microVolt
		if (filteredEEG.Length == filteredEEGData.Length)
			filteredEEGData = (float[])filteredEEG.Clone();

		if (bEegStarted && rawdataGrabbed != null) //only trigger event if eeg receiving is ON
			rawdataGrabbed.Invoke();    //announce that new set of data has been fetched			 

		if (rawdataGrabbed == null)
			Debug.Log("rawdataGrabbed not assigned");

	}


    /// <summary>
    /// Receives verification from plugin if SenzeBand is AC mode or DC mode
    /// </summary>
    /// <param name="ack">can be either "COMMAND_ACK_DC_LEAD_OFF" or "COMMAND_ACK_AC_LEAD_OFF"</param>
    void grabCommandACK(string ack)
    {
        Debug.Log("grabCommandACK ack=" + ack);
        onCommandAcknowledged.Invoke(ack);
    }

	/// <summary>
	/// Determines SenzeBand version
	/// </summary>
	/// <returns>Version of the connected SenzeBand device</returns>
	public string GetConnectedSBVersion()
	{
		return version;
	}
}



