using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandHandler : MonoBehaviour
{
    public static CommandHandler instance;
    private NSB_Manager nsbm;

    bool isACLeadoffMode = false;
    bool isEegON = false;
    bool isPpgON = false;
    bool receivingData = false;
    bool startToggleImpedance = false;

    public bool isEEGON { get => isEegON; set => isEegON = value; }
    public bool isPPGON { get => isPpgON; set => isPpgON = value; }
    public bool isImpedanceCheckOn { get => isACLeadoffMode; set => isACLeadoffMode = value; }
    public bool isReceivingData { get => receivingData; set => receivingData = value; }

    string acknowledgedCommand = "";

    
    [Space]

    [Header("Butttons")]
    public Button ppgButtonStart;
    public Button ppgButtonStop;
    public Button eegButtonStart;
    public Button eegButtonStop;
    public Button impedanceCheckBtn;
    [Space]
    public Sprite activatedButtonSprite;
    public Sprite deActivatedButtonSprite;

    void Start()
    {
        if (instance == null)
            instance = this;

        nsbm = NSB_Manager.instance;
        nsbm.connectionSuccessfulCallback.AddListener(() => isACLeadoffMode = false);
        nsbm.connectionBrokenCallback.AddListener(() => isACLeadoffMode = false);

        nsbm.onCommandAcknowledged.AddListener((x) =>
        {
            var tmpString = x;
            acknowledgedCommand = tmpString.Trim();
        });

        ppgButtonStart.onClick.AddListener(StartReceivingPPG);
        ppgButtonStop.onClick.AddListener(StopReceivingPPG);
        eegButtonStart.onClick.AddListener(StartEEG);
        eegButtonStop.onClick.AddListener(StopEEG);

        startToggleImpedance = false;
    }

    #region Controls

    public void StartEEG()
    {
        if (!isEEGON)
        {
            Debug.Log("PPGTrack: EEG has started");

            string command = "COMMAND_START";
            nsbm.sendCommand(command);
            isEEGON = true;
            
        }
        else
        {
            Debug.Log("PPGTrack: EEG has already started");
        }
        eegButtonStart.gameObject.SetActive(false);
        eegButtonStop.gameObject.SetActive(true);
    }

    public void StopEEG()
    {

        if (isEEGON)
        {
            Debug.Log("PPGTrack: EEG has stopped");
            string command = "COMMAND_STOP";
            nsbm.sendCommand(command);

            isEEGON = false;
        }
        eegButtonStart.gameObject.SetActive(true);
        eegButtonStop.gameObject.SetActive(false);
    }


    public void StartCalibration()
    {
        // StartCoroutine(StartCalibration_IE());
        SendCalStart();
    }
    public IEnumerator StartCalibration_IE()
    {

        StopEEG();
        yield return new WaitForSeconds(1);
        SendCalStart();
    }

    public void StopCalibration()
    {
        // StartCoroutine(StopCalibration_IE());
        SendCalStop();
    }
    public IEnumerator StopCalibration_IE()
    {
        SendCalStop();
        yield return new WaitForSeconds(1);
        StartEEG();
    }
    public void StartReceivingPPG()
    {
        isPPGON = true;
        // StartCoroutine(StarReceivingPPG_IE());
        SendPPGStart();
        ButtonBehavior(ppgButtonStart, ppgButtonStop);

        if (isEEGON)
        {
            SendStop();
        }
    }
    public IEnumerator StarReceivingPPG_IE()
    {
        SendStop();
        yield return new WaitForSeconds(1);
        SendPPGStart();
    }

    public void StopReceivingPPG()
    {
        isPPGON = false;
        SendPPGStop();
        ButtonBehavior(ppgButtonStop, ppgButtonStart);


    }
    public IEnumerator StopReceivingPPG_IE()
    {
        SendPPGStop();
        yield return new WaitForSeconds(1);
        SendStart();
    }
    public void SendStart()
    {
        ButtonBehavior(eegButtonStart, eegButtonStop);

        if (!isEEGON)
        {
            Debug.Log("PPGTrack: EEG has started");

            string command = "COMMAND_START";
            nsbm.sendCommand(command);
            isEEGON = true;

            if (isPPGON)
            {
                StopReceivingPPG();
                isPPGON = false;
            }
        }
        else
        {
            Debug.Log("PPGTrack: EEG has already started");
        }
        eegButtonStart.gameObject.SetActive(false);
        eegButtonStop.gameObject.SetActive(true);
    }

    public void SendStop()
    {
        ButtonBehavior(eegButtonStop, eegButtonStart);

        if (isEEGON)
        {
            Debug.Log("PPGTrack: EEG has stopped");
            string command = "COMMAND_STOP";
            nsbm.sendCommand(command);

            isEEGON = false;
        }
        eegButtonStart.gameObject.SetActive(true);
        eegButtonStop.gameObject.SetActive(false);
    }
    public void ButtonBehavior(Button activatedButton, Button deActivatedButton)
    {

        activatedButton.GetComponent<Image>().sprite = activatedButtonSprite;
        deActivatedButton.GetComponent<Image>().sprite = deActivatedButtonSprite;
    }




    #endregion


    #region commands

  
    
    public void ToggleImpedance()
    {
        if (startToggleImpedance) return;
        StartCoroutine(SendImpedanceCommandThenVerify());
    }

    public IEnumerator SendImpedanceCommandThenVerify()
    {
        startToggleImpedance = true;
        if (!isEEGON)
        {
            Debug.LogError("No EEG");
            yield break;
        }

        acknowledgedCommand = "-";

        if (isACLeadoffMode)
        {
            Debug.LogError("Sending DC");
            SendDCLeadoff();
        }
        else
        {
            Debug.LogError("Sending AC");
            SendACLeadoff();
        }

        //wait for verification
        yield return new WaitUntil(() => acknowledgedCommand != "-");

        if (acknowledgedCommand.Contains("DC_LEAD_OFF")) isACLeadoffMode = false;
        if (acknowledgedCommand.Contains("AC_LEAD_OFF")) isACLeadoffMode = true;

        if (isACLeadoffMode)
        {
            impedanceCheckBtn.GetComponent<Image>().sprite = deActivatedButtonSprite;
        }
        else
        {
            impedanceCheckBtn.GetComponent<Image>().sprite = activatedButtonSprite;
        }

        startToggleImpedance = false;

        yield return null;
    }


    public void SendDCLeadoff()
    {
        string command = "COMMAND_DC_LEADOFF";
        nsbm.sendCommand(command);
    }
    public void SendACLeadoff()
    {
        string command = "COMMAND_AC_LEADOFF";
        nsbm.sendCommand(command);
    }

    public void SendRed()
    {
        string command = "COMMAND_LIGHT_RED";
        nsbm.sendCommand(command);
    }
    public void SendGreen()
    {
        string command = "COMMAND_LIGHT_GREEN";
        nsbm.sendCommand(command);
    }
    public void SendBlue()
    {
        string command = "COMMAND_LIGHT_BLUE";
        nsbm.sendCommand(command);
    }
    public void SendCyan()
    {
        string command = "COMMAND_LIGHT_CYAN";
        nsbm.sendCommand(command);
    }
    public void SendMagenta()
    {
        string command = "COMMAND_LIGHT_MAGENTA";
        nsbm.sendCommand(command);
    }
    public void SendYellow()
    {
        string command = "COMMAND_LIGHT_YELLOW";
        nsbm.sendCommand(command);
    }
    public void SendStopRGB()
    {
        string command = "COMMAND_STOP_RGB";
        nsbm.sendCommand(command);
    }
    public void SendFWVER()
    {
        string command = "COMMAND_FW_VER";
        nsbm.sendCommand(command);
    }
    public void SendCalStart()
    {
        string command = "COMMAND_CAL_START";
        nsbm.sendCommand(command);
    }
    public void SendCalStop()
    {
        string command = "COMMAND_CAL_STOP";
        nsbm.sendCommand(command);
    }
    public void SendPPGStart()
    {
        string command = "COMMAND_PPG_START";
        nsbm.sendCommand(command);
        nsbm.SetReceivePPG(true);
        Debug.Log("PPGTrack: PPG has started");
    }
    public void SendPPGStop()
    {
        string command = "COMMAND_PPG_STOP";
        nsbm.sendCommand(command);
        nsbm.SetReceivePPG(false);

        Debug.Log("PPGTrack: PPG has stopped");
    }


    #endregion
}
