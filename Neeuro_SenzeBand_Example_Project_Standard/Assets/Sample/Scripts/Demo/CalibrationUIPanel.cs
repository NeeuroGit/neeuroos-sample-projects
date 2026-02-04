using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationUIPanel : MonoBehaviour
{
    private NSB_Manager nsbm;
    public Text CalibrationXGainValue;
    public Text CalibrationYGainValue;
    public Text CalibrationZGainValue;

    private void Start()
    {
        if (nsbm == null)
            nsbm = NSB_Manager.instance;
    }
    // Update is called once per frame
    void Update()
    {
        if (nsbm.GetReceiveEEGState())
        {
          

            float[] calibrationParameters = nsbm.GetCalibrationParameters();
            CalibrationXGainValue.text = calibrationParameters[0].ToString("#0.000");
            CalibrationYGainValue.text = calibrationParameters[1].ToString("#0.000");
            CalibrationZGainValue.text = calibrationParameters[2].ToString("#0.000");


        }
        else
        {            

            float[] calibrationParameters = nsbm.GetCalibrationParameters();
            CalibrationXGainValue.text = calibrationParameters[0].ToString("#0.000");
            CalibrationYGainValue.text = calibrationParameters[1].ToString("#0.000");
            CalibrationZGainValue.text = calibrationParameters[2].ToString("#0.000");

        }
    }
}
