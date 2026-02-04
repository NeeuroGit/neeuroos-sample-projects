using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphUpdater : MonoBehaviour
{
    private NSB_Manager nsbm;

    public Image Ch1ConnectionIndicator;
    public Image Ch2ConnectionIndicator;
    public Image Ch3ConnectionIndicator;
    public Image Ch4ConnectionIndicator;
    public SideButtonPanelsController sidePanel;

    private ConnectionColorIndicatorManager colorConnectionManager;
    

    // Start is called before the first frame update
    void Start()
    {
        nsbm = NSB_Manager.instance;
        colorConnectionManager = ConnectionColorIndicatorManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateEEGConnectionColors();
    }
    private int count = 0;

    public void UpdateEEGConnectionColors() {
        if (nsbm.GetReceiveEEGState())
        {
            if (nsbm.GetChannelStatus(0)){
                Ch1ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else{
                Ch1ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }

            if (nsbm.GetChannelStatus(1)){
                Ch2ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else{
                Ch2ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }

            if (nsbm.GetChannelStatus(2)){
                Ch3ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else{
                Ch3ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }

            if (nsbm.GetChannelStatus(3)){
                Ch4ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else{
                Ch4ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }
        }
        else {
            Ch1ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch2ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch3ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch4ConnectionIndicator.color =colorConnectionManager.noConnectionColor;
        }
    }
    public void UpdateGraphs()
    {
        Debug.Log("UpdateGraphs");

     //   grabRawEEG(NSB_Manager.instance.GetFilteredEEG());

     //   grabFrequencyBand(NSB_Manager.instance.GetFrequencyBand());
    }

    public void UpdatePPGGraphs()
    {
        if (SDKStateManager.instance.currentState != SDKStateManager.SDK_State.PPG_PANEL)
        return;
        //Debug.Log("UpdatePPGGraphs");
        
        //grabRawPPG(NSB_Manager.instance.GetRawPPG());
       // grabRawPPG(new int[] { (int)(count % 1000), (int)((count+500) % 1000) });
       // count = count + 100;
    }

    float[] frequencyBandData = new float[5];
    void grabFrequencyBand(float[,] frequencyBand)
    {
        //check if returned data is correct length
        if (frequencyBand.GetLength(1) != frequencyBandData.Length)
            return;

        //if (abdt[0].isActiveAndEnabled == false) return;

        string abdtstring = "";
        for (int i = 0; i < frequencyBandData.Length; ++i)
        {
            frequencyBandData[i] = frequencyBand[0, i];
            {
                float[] d = new float[1];
                for (int j = 0; j < d.Length; ++j)
                {
                    d[j] = frequencyBandData[i];
                }

                abdt[i].SetData(d);
            
            }
        }

        for (int i = 0, length = frequencyBand.GetLength(0); i < length; ++i)
        {
            for (int j = 0, width = frequencyBand.GetLength(1); j < width; ++j)
            {
                abdtstring += frequencyBand[i, j].ToString() + " ";
                //deltaValue.text = frequencyBand[0, j].ToString();
            }
        }
            

        Debug.Log("Received abdt: " + abdtstring);


    }

    float[] rawEEGData = new float[1000];
    public LineCanvas eeg1, eeg2, eeg3, eeg4;
    public LineCanvas ir, red;
    public List<LineCanvas> abdt;
    public TextMesh[] abdtDisplay;

    void grabRawEEG(float[] rawEEG)
    {
        Debug.Log("Unity: Received EEG");

        //Raw EEG received is integer values where 1 unit = 1 * 0.61 microVolt
        if (rawEEG.Length == rawEEGData.Length)
            rawEEGData = (float[])rawEEG.Clone();

        //if (eeg1.isActiveAndEnabled == false) return;
        {
            float[] d = new float[250];
            Array.Copy(rawEEG, 0, d, 0, 250);
            eeg1.SetData(d);
        }

        {
            float[] d = new float[250];
            Array.Copy(rawEEG, 250, d, 0, 250);
            eeg2.SetData(d);
        }

        {
            float[] d = new float[250];
            Array.Copy(rawEEG, 500, d, 0, 250);
            eeg3.SetData(d);
        }

        {
            float[] d = new float[250];
            Array.Copy(rawEEG, 750, d, 0, 250);
            eeg4.SetData(d);
        }
    }

    void Populate<T>(T[] arr, T value)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = value;
        }
    }

    void CopyToArray<T>(T[] arr, T[] source, T defaultValue)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if(i < source.Length)
            {
                arr[i] = source[i];
            }
            else
            {
                arr[i] = defaultValue;
            }
            
        }
    }

    private float processData(float n, float min, float max)
    {
        float result = Mathf.Clamp(n, min, max) - min;

        return result;

    }

    void grabRawPPG(List<int[]> rawPPG)
    {
        Debug.Log("Unity: Received PPG");

        int[] irValue = new int[rawPPG.Count];
        int[] redValue = new int[rawPPG.Count];

        for (int i = 0; i < rawPPG.Count; i++)
        {
            irValue[i] = (int)processData(rawPPG[i][0], 140000, 155000);
            redValue[i] = (int)processData(rawPPG[i][1], 130000, 140000);
        }

        //if (ir.isActiveAndEnabled == false) return;
        {
            int[] d = new int[1];            
            CopyToArray(d, irValue, 0);
            Debug.Log("irValues:" + PrintValues(d));
            ir.SetData(d);
        }

        {
            int[] d = new int[1];
            CopyToArray(d, redValue, 0);
            Debug.Log("redValues:" + PrintValues(d));
            red.SetData(d);
        }
    }

    public static string PrintValues(int[] myArr)
    {
        string result = "";
        foreach (int i in myArr)
        {
            result += " " + i;
        }
        return result;
    }
}
