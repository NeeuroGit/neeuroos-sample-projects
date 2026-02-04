using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusPanelUI : MonoBehaviour
{
    private NSB_Manager nsbm;
    public Text SPO2Value;
    public Text HeartRateValue;

    void Start()
    {
        if (nsbm == null)
            nsbm = NSB_Manager.instance;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (nsbm.GetReceivePPGState() || nsbm.GetReceiveEEGState())
        {
            SPO2Value.text = nsbm.GetSPO2().ToString();
            HeartRateValue.text = nsbm.GetHeartRate().ToString();
        }
        else
        {
            SPO2Value.text = "-";
            HeartRateValue.text = "-";
        }

    }
}
