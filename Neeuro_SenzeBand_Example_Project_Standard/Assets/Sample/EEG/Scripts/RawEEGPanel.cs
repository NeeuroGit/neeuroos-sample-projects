using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RawEEGPanel : MonoBehaviour
{
    public PartitionedMovingLineGraph rightChannelGraph;
    public PartitionedMovingLineGraph centerRightChannelGraph;
    public PartitionedMovingLineGraph centerLeftChannelGraph;
    public PartitionedMovingLineGraph leftChannelGraph;

    public const float CEILING_EEG_VALUE = 80;
    public const float FLOOR_EEG_VALUE = -80;

    private bool startDisplaying = false;
    private int rightChannelDataDisplayed = 0; //tracks number of data displayed from right channel on current second
    private int centerRightChannelDataDisplayed = 0;    //tracks number of data displayed from center-right channel on current second
    private int centerLeftChannelDataDisplayed = 0; //tracks number of data displayed from center-left channel on current second
    private int leftChannelDataDisplayed = 0;   //tracks number of data displayed from left channel on current second

    private bool newRawDataArrived = false; //flag to determine if new full-batch of raw data has been fetched

    private NSB_Manager nsbm;

    private Coroutine gradualEnqueueCoroutine;

    private const int CHANNEL_DATASIZE = 250;
    private const int BUFFER_SIZE = 500;    //2 seconds worth of buffer

    private ConnectionColorIndicatorManager colorConnectionManager;

    public Image Ch1ConnectionIndicator;
    public Image Ch2ConnectionIndicator;
    public Image Ch3ConnectionIndicator;
    public Image Ch4ConnectionIndicator;

    private void Start()
    {
        colorConnectionManager = ConnectionColorIndicatorManager.instance;
        nsbm = NSB_Manager.instance;
        nsbm.rawdataGrabbed.AddListener(FetchNewDataSet);
        nsbm.rawdataGrabbed.AddListener(UpdateEEGConnectionColors);

        StartCoroutine(ResetGraphsByEndOfFrame());
    }

    private void Update()
    {
        UpdateEEGConnectionColors();
#if UNITY_EDITOR
        //FOR DEBUGGING PURPOSES IN EDITOR: Press SPACE to act as SenzeBand "sending" values to system
        if (Input.GetKeyDown(KeyCode.Space))
            FetchNewDataSet();
#endif
    }

    private void FixedUpdate()
    {
        
		int BATCH_SIZE = Mathf.RoundToInt(CHANNEL_DATASIZE * Time.fixedDeltaTime);
              
		float blankValuePercentage = 0.5f;  //middle of graph

		for(int i = 0; i<BATCH_SIZE; i++)
		{
			rightChannelGraph.AddToQueue(blankValuePercentage);
			rightChannelGraph.UpdateGraph();

			centerRightChannelGraph.AddToQueue(blankValuePercentage);
			centerRightChannelGraph.UpdateGraph();

			centerLeftChannelGraph.AddToQueue(blankValuePercentage);
			centerLeftChannelGraph.UpdateGraph();

			leftChannelGraph.AddToQueue(blankValuePercentage);
			leftChannelGraph.UpdateGraph();
        }
    }

    public void ResetGraphs()
    {      
        startDisplaying = false;

        rightChannelDataDisplayed = 0;
        centerRightChannelDataDisplayed = 0;
        centerLeftChannelDataDisplayed = 0;
        leftChannelDataDisplayed = 0;

        rightChannelGraph.ResetGraph();
        centerRightChannelGraph.ResetGraph();
        centerLeftChannelGraph.ResetGraph();
        leftChannelGraph.ResetGraph();
    }

    public void FetchNewDataSet()
    {
        Debug.Log("FetchNewDataSet SDKStateManager.instance.currentState=" + SDKStateManager.instance.currentState);
        if (SDKStateManager.instance.currentState != SDKStateManager.SDK_State.EEG_PANEL)
            return;
#if !UNITY_EDITOR
        Debug.Log("FetchNewDataSet nsbm.GetReceiveEEGState()=" + nsbm.GetReceiveEEGState());
        if (!nsbm.GetReceiveEEGState()) //don't update graphs if live feed is OFF
            return;
#endif

        float[] rawEEG = nsbm.GetFilteredEEG();

    #if UNITY_EDITOR
        //FOR DEBUGGING PURPOSES IN EDITOR: dummy values that the "senzeband sent" to the system
        for (int i = 0; i < 1000; i++)
        {
            rawEEG[i] = (i%250);
        }
    #endif

		float[] rightChannel = new float[CHANNEL_DATASIZE];
		float[] centerRightChannel = new float[CHANNEL_DATASIZE];
		float[] centerLeftChannel = new float[CHANNEL_DATASIZE];
		float[] leftChannel = new float[CHANNEL_DATASIZE];
		//separate 1000 raw eeg data into each channels with 250 eeg data each
        for (int i = 0; i < CHANNEL_DATASIZE; i++)
        {            
			rightChannel[i] = MathHelper.GetPercentageOfValueInRange(rawEEG[i], FLOOR_EEG_VALUE, CEILING_EEG_VALUE);
			centerRightChannel[i] = MathHelper.GetPercentageOfValueInRange(rawEEG[i + CHANNEL_DATASIZE], FLOOR_EEG_VALUE, CEILING_EEG_VALUE);
			centerLeftChannel[i] = MathHelper.GetPercentageOfValueInRange(rawEEG[i + (CHANNEL_DATASIZE * 2)], FLOOR_EEG_VALUE, CEILING_EEG_VALUE);
			leftChannel[i] = MathHelper.GetPercentageOfValueInRange(rawEEG[i + (CHANNEL_DATASIZE * 3)], FLOOR_EEG_VALUE, CEILING_EEG_VALUE);         
        }

		rightChannelGraph.InsertToQueue(rightChannel);
		centerRightChannelGraph.InsertToQueue(centerRightChannel);
		centerLeftChannelGraph.InsertToQueue(centerLeftChannel);
		leftChannelGraph.InsertToQueue(leftChannel);
    }
   

    IEnumerator ResetGraphsByEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        ResetGraphs();
    }



    public void UpdateEEGConnectionColors()
    {
        Debug.Log("UpdateEEGConnectionColors nsbm.GetReceiveEEGState()=" + nsbm.GetReceiveEEGState());
        if (nsbm.GetReceiveEEGState())
        {
            if (nsbm.GetChannelStatus(0))
            {
                Ch1ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else
            {
                Ch1ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }

            if (nsbm.GetChannelStatus(1))
            {
                Ch2ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else
            {
                Ch2ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }

            if (nsbm.GetChannelStatus(2))
            {
                Ch3ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else
            {
                Ch3ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }

            if (nsbm.GetChannelStatus(3))
            {
                Ch4ConnectionIndicator.color = colorConnectionManager.connectedColor;
            }
            else
            {
                Ch4ConnectionIndicator.color = colorConnectionManager.disConnectedColor;
            }
        }
        else
        {
            Ch1ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch2ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch3ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch4ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
        }
    }

}
