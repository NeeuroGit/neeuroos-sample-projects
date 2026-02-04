using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SDKStateManager : MonoBehaviour
{
    public static SDKStateManager instance = null;
    
    public enum SDK_State
    {
        SENZEBAND_CONTROL_PANEL,
        EEG_PANEL,
        ABDT_PANEL,
        PPG_PANEL,
        MENTAL_STATES_PANEL,
        MOTION_SENSOR_CALIBRATION_PANEL

    }

    public SDK_State currentState;

    private void Start()
    {
        instance = this;
        currentState = SDK_State.SENZEBAND_CONTROL_PANEL;
        GetComponent<SideButtonPanelsController>().ChangeToSBControlPanel();
       
    }
}
