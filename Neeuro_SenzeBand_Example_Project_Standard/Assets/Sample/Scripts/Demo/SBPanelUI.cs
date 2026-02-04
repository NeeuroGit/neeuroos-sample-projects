using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sample usage file. To control the demo Senzeband UI
/// </summary>
public class SBPanelUI : MonoBehaviour
{

    private NSB_Manager nsbm;

    public Text ConnectionStatusText;
    public Text SBAddressText;
    public Text SBMcuidText;
    public Text SBBattery;
    public Text ReceiveEEGText;
    public Text AuthenticationText;
    public Button DisconnectButton;
    public Toggle ToggleEEGButton;
    public Button CancelConnectButton;
    public Button AuthenticateButton;

    private bool bIsReady;
    private bool bIsConnected;

    private ConnectionColorIndicatorManager connectionColorManager;

    const string defaultMCUIDtext = "MCUID";
    const string defaultSBAddresstext = "SB Address";
    const string defaultBatterytext = "-";

    public Image connectionIndicator;
  
    // Use this for initialization
    void Start()
    {
        nsbm = NSB_Manager.instance;
        connectionColorManager = ConnectionColorIndicatorManager.instance;

        bIsReady = false;
        if (SBAddressText != null)
            SBAddressText.text = defaultSBAddresstext;

        if (DisconnectButton != null)
            DisconnectButton.gameObject.SetActive(false);

        if (SBMcuidText != null)
            SBMcuidText.text = defaultMCUIDtext;

        if (SBBattery != null)
            SBBattery.text = defaultBatterytext;

        if (ToggleEEGButton != null)
            ToggleEEGButton.gameObject.SetActive(false);

        if (ReceiveEEGText != null)
            ReceiveEEGText.gameObject.SetActive(false);

        if (CancelConnectButton != null)
            CancelConnectButton.gameObject.SetActive(false);

        if (AuthenticateButton != null)
        {
            AuthenticateButton.gameObject.SetActive(true);
            AuthenticateButton.onClick.AddListener(() =>
            {
                nsbm.AuthenticateUser();
            });
        }

        if (ConnectionStatusText != null)
            ConnectionStatusText.text = "-";

        nsbm.connectionBrokenCallback.AddListener(() =>
        {
            ToggleEEGButton.isOn = false;
            ToggleEEG(false);
        });

        nsbm.connectionFailedCallback.AddListener(() =>
        {
            ToggleEEGButton.isOn = false;
            ToggleEEG(false);
        });

        nsbm.authenticationUpdated.AddListener(UpdateAuthenStatusText);
    }

    void UpdateAuthenStatusText()
    {
        if (nsbm.GetAuthenticationResult())
        {
            AuthenticationText.text = "Authentication period: " + nsbm.AuthPeriod / 1000;
        }
        else
        {
            AuthenticationText.text = nsbm.GetAuthenticationStatus();
        }
    }

    // Update is called once per frame
    void Update()
    {

        //Performs one-time processes when NSB init is completed.
        if (!bIsReady)
        {
            if (nsbm.IsInitCompleted())
            {
                //Just Ready!
                bIsReady = true;
            }
            else
            {
                //NSB init is not ready yet, so skip all following processes
                return;
            }
        }

        /*
		Connection States
		- 0 	- Not connected
		- 1 	- Connecting
		- 2		- Connected, connectionStatus is updated in DeviceStatus callback
		- 3		- Connected, SB address is returned in ConnectionSucceed callback
		- 4		- Connected, MCUID is received
		*/
        //Track the state of connection
        if (!bIsConnected)
        {
            int subState = nsbm.GetConnectionState();
            switch (subState)
            {
                case 0:
                    ConnectionStatusText.text = "Not connected";
                    connectionIndicator.color = connectionColorManager.noConnectionColor;
                    if (CancelConnectButton.gameObject.activeSelf == true)
                    {
                        CancelConnectButton.gameObject.SetActive(false);
                        CancelConnectButton.onClick.RemoveAllListeners();
                    }
                    break;
                case 1:
                    ConnectionStatusText.text = "Connecting...";
                    if (CancelConnectButton.gameObject.activeSelf == false)
                    {
                        CancelConnectButton.gameObject.SetActive(true);
                        CancelConnectButton.onClick.AddListener(() =>
                        {
                            nsbm.DisconnectSB();
                        });     //TODO: for Android, to add a cancel connecting process function. For iOS, this disconnect function works
                    }
                    break;
                case 2:
                    ConnectionStatusText.text = "Connected, awaiting info";
                    break;
                case 3:
                    ConnectionStatusText.text = "Connected, awaiting more info";
                    break;
                case 4:
                    ConnectionStatusText.text = "Connected. Ready!!";
                    connectionIndicator.color = connectionColorManager.connectedColor;
                    break;
            }
        }

        //Transitions from Not Connected to Connected; add in and remove buttons
        if (!bIsConnected && nsbm.GetConnectionState() == 4)
        {
            //Just connected
            if (CancelConnectButton.gameObject.activeSelf == true)
            {
                CancelConnectButton.gameObject.SetActive(false);
                CancelConnectButton.onClick.RemoveAllListeners();
            }
            DisconnectButton.gameObject.SetActive(true);
            DisconnectButton.onClick.AddListener(() => { nsbm.DisconnectSB(); });

            ToggleEEGButton.gameObject.SetActive(true);
            //ToggleEEGButton.onClick.AddListener ( ToggleEEG );

            ReceiveEEGText.gameObject.SetActive(true);
            if (!nsbm.GetReceiveEEGState())
                ReceiveEEGText.text = "Not receiving EEG";

            //Debug.Log ("NSB SB Panel Connected - "+nsbm.NSBm_GetConnectedSBAddress ());
            SBAddressText.text = nsbm.GetConnectedSBAddress();
            SBMcuidText.text = nsbm.GetConnectedSBMCUID();
            SBBattery.text = nsbm.GetConnectedSBBattery();
            bIsConnected = true;


        }
        else if (bIsConnected && nsbm.GetConnectionState() == 0)
        {
            //Just disconnected
            connectionIndicator.color = connectionColorManager.noConnectionColor;
            DisconnectButton.gameObject.SetActive(false);
            DisconnectButton.onClick.RemoveAllListeners();

            ToggleEEGButton.gameObject.SetActive(false);
            //ToggleEEGButton.onClick.RemoveAllListeners ();

            ReceiveEEGText.gameObject.SetActive(false);

            //Debug.Log ("NSB SB Panel Disconnected");
            SBAddressText.text = defaultSBAddresstext;
            SBMcuidText.text = defaultMCUIDtext;
            SBBattery.text = defaultBatterytext;
            bIsConnected = false;
        }

        if (nsbm.GetReceiveEEGState())
        {
            SBBattery.text = nsbm.GetConnectedSBBattery() + "% battery";
        }
        else
            SBBattery.text = defaultBatterytext;
    }
    public void ButtonEnableEEG()
    {
        ToggleEEG();
    }

    private void ToggleEEG()
    {
        //To toggle to receive EEG data
        if (!nsbm.GetReceiveEEGState())
        {
            nsbm.SetReceiveEEG(true);
            ReceiveEEGText.text = "Receiving EEG";
            CommandHandler.instance.isEEGON = true;            
            CommandHandler.instance.isReceivingData = true;
        }
        else
        {
            nsbm.SetReceiveEEG(false);
            ReceiveEEGText.text = "Not receiving EEG";
            CommandHandler.instance.isEEGON = false;
            CommandHandler.instance.isReceivingData = false;
        }
    }

    private void ToggleEEG(bool status)
    {
        //To toggle to receive EEG data
        if (status)
        {
            nsbm.SetReceiveEEG(true);
            ReceiveEEGText.text = "Receiving EEG";
            CommandHandler.instance.isEEGON = true;
            CommandHandler.instance.isReceivingData = true;
        }
        else
        {

            nsbm.SetReceiveEEG(false);
            ReceiveEEGText.text = "Not receiving EEG";
            CommandHandler.instance.isEEGON = false;
            CommandHandler.instance.isReceivingData = false;
        }
    }



 

}