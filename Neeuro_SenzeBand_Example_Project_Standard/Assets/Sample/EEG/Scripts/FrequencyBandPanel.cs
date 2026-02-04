using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class FrequencyBandPanel : MonoBehaviour
{
	public UIObject[] channelParents;   //indexes: 0-right 1-centerright 2-centerleft 3-left
	public Toggle[] frequencyFilters;   //indexes: 0-delta 1-theta 2-alpha 3-beta 4-gamma
	public MovingLineGraph[] alphaGraph;
	public MovingLineGraph[] betaGraph;
	public MovingLineGraph[] deltaGraph;
	public MovingLineGraph[] thetaGraph;
	public MovingLineGraph[] gammaGraph;
	public MovingLineGraph[] smoothenedAlphaGraph;
	public MovingLineGraph[] smoothenedBetaGraph;
	public MovingLineGraph[] smoothenedDeltaGraph;
	public MovingLineGraph[] smoothenedThetaGraph;
	public MovingLineGraph[] smoothenedGammaGraph;

	public GameObject uiBlocker;

    private NSB_Manager nsbm;
	private Queue<float>[] movingAlphaList;   //alpha list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing
	private Queue<float>[] movingBetaList; //beta list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing
	private Queue<float>[] movingDeltaList;  //delta list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing
	private Queue<float>[] movingThetaList;   //theta list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing
	private Queue<float>[] movingGammaList;  //gamma list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing
    
	private Vector2 overallAlpha;    //(total alpha overtime, number of alpha samples) to be divided together to get average
    private Vector2 overallBeta;    //(total beta overtime, number of beta samples) to be divided together to get average
    private Vector2 overallDelta;    //(total delta overtime, number of delta samples) to be divided together to get average
	private Vector2 overallTheta;    //(total theta overtime, number of theta samples) to be divided together to get average
    private Vector2 overallGamma;    //(total gamma overtime, number of gamma samples) to be divided together to get average

	private int shownChannel = -1;    //-1 means shown channel wasn't set yet and is currently showing all channels

    private const int MOVINGLIST_MAX_LIMIT = 3;
	private Vector2 ALPHA_RANGE = new Vector2(0f, 0.25f);  //min and max values of Alpha frequency
    //private Vector2 BETA_RANGE = new Vector2(0f, 0.25f);  //min and max values of Beta frequency
    private Vector2 BETA_RANGE = new Vector2(0f, 0.35f);  //min and max values of Beta frequency
    private Vector2 DELTA_RANGE = new Vector2(0f, 1f);  //min and max values of Delta frequency
	private Vector2 THETA_RANGE = new Vector2(0f, 0.8f);  //min and max values of Theta frequency
	private Vector2 GAMMA_RANGE = new Vector2(0f, 0.2f);  //min and max values of Gamma frequency
    private const int FREQ_CHANNELS = 4;


    public Text deltaValueText;
    public Text thetaValueText;
    public Text alphaValueText;
    public Text betaValueText;
    public Text gammaValueText;

    private void Start()
    {
        nsbm = NSB_Manager.instance;
       nsbm.rawdataGrabbed.AddListener(UpdateGraphs);

		ShowChannel(0,false);      
        ResetMovingLists();
    }

    private void Update()
    {
#if UNITY_EDITOR
        //FOR DEBUGGING PURPOSES IN EDITOR: Press SPACE to act as SenzeBand "sending" values to system
        if (Input.GetKeyDown(KeyCode.Space))
            UpdateGraphs();
#endif
    }

    public void ResetGraphs()
    {
		for (int i = 0; i < FREQ_CHANNELS; i++)
		{
			alphaGraph[i].ResetGraph();
			betaGraph[i].ResetGraph();
			deltaGraph[i].ResetGraph();
			thetaGraph[i].ResetGraph();
			gammaGraph[i].ResetGraph();
			smoothenedAlphaGraph[i].ResetGraph();
			smoothenedBetaGraph[i].ResetGraph();
			smoothenedDeltaGraph[i].ResetGraph();
			smoothenedThetaGraph[i].ResetGraph();
			smoothenedGammaGraph[i].ResetGraph();
		}

		ResetAveraging();
        ResetMovingLists();
    }

    private void ResetMovingLists()
    {
		movingAlphaList = new Queue<float>[FREQ_CHANNELS];
        movingBetaList = new Queue<float>[FREQ_CHANNELS];
        movingDeltaList = new Queue<float>[FREQ_CHANNELS];
        movingThetaList = new Queue<float>[FREQ_CHANNELS];
        movingGammaList = new Queue<float>[FREQ_CHANNELS];
		for (int i = 0; i < FREQ_CHANNELS; i++)
		{
			movingAlphaList[i] = new Queue<float>();
			movingBetaList[i] = new Queue<float>();
			movingDeltaList[i] = new Queue<float>();
			movingThetaList[i] = new Queue<float>();
			movingGammaList[i] = new Queue<float>();
		}
    }

	private void ResetAveraging()
    {
        overallAlpha = Vector2.zero;
        overallBeta = Vector2.zero;
        overallDelta = Vector2.zero;
		overallTheta = Vector2.zero;
        overallGamma = Vector2.zero;
    }

	public void UpdateGraphs()
	{
        if (SDKStateManager.instance.currentState != SDKStateManager.SDK_State.ABDT_PANEL)
            return;
#if !UNITY_EDITOR
        if (!nsbm.GetReceiveEEGState()) //don't update graphs if live feed is OFF
            return;
#endif

        StartCoroutine(UpdateGraphs_IEnum());
	}

    private IEnumerator UpdateGraphs_IEnum()
	{
		int priorityChannel = shownChannel; //first loop on channel that is shown
		bool firstLoop = true;
		for (int i = 0; i < FREQ_CHANNELS; i++)
        {
			if(firstLoop)
			{
				i = priorityChannel;
			}
			else
			{
				//if i is equal to priority channel, skip because it was already updated on first loop
				if (i == priorityChannel)   
					continue;
			}
            /*
             * FREQUENCY BAND INDEXES
                0-delta
                1-theta
                2-alpha
                3-beta
                4-gamma
             */

            float deltaVal = nsbm.GetFrequencyBand(i, 0);
            float thetaVal = nsbm.GetFrequencyBand(i, 1);
            float alphaVal = nsbm.GetFrequencyBand(i, 2);
            float betaVal = nsbm.GetFrequencyBand(i, 3);
            float gammaVal = nsbm.GetFrequencyBand(i, 4);

#if UNITY_EDITOR
            //FOR DEBUGGING PURPOSES IN EDITOR: dummy values that the "senzeband sent" to the system
            alphaVal = Random.Range(ALPHA_RANGE.x, ALPHA_RANGE.y);
            betaVal =  Random.Range(BETA_RANGE.x, BETA_RANGE.y);
            deltaVal = Random.Range(DELTA_RANGE.x, DELTA_RANGE.y);
            thetaVal = Random.Range(THETA_RANGE.x, THETA_RANGE.y);
            gammaVal = Random.Range(GAMMA_RANGE.x, GAMMA_RANGE.y);
#endif
        
            
            //add values to total for averaging and increment number of samples
            overallAlpha[0] += alphaVal;
            overallAlpha[1]++;
            overallBeta[0] += betaVal;
            overallBeta[1]++;
            overallDelta[0] += deltaVal;
            overallDelta[1]++;
            overallTheta[0] += thetaVal;
            overallTheta[1]++;
            overallGamma[0] += gammaVal;
            overallGamma[1]++;

            //update raw line graph
            alphaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(alphaVal, ALPHA_RANGE.x, ALPHA_RANGE.y));
            betaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(betaVal, BETA_RANGE.x, BETA_RANGE.y));
            deltaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(deltaVal, DELTA_RANGE.x, DELTA_RANGE.y));
            thetaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(thetaVal, THETA_RANGE.x, THETA_RANGE.y));
            gammaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(gammaVal, GAMMA_RANGE.x, GAMMA_RANGE.y));

			//yield return null;  //separate to next frame

            //update moving list to be averaged for smoothing
            AddToMovingList(ref movingAlphaList[i], alphaVal);
            AddToMovingList(ref movingBetaList[i], betaVal);
            AddToMovingList(ref movingDeltaList[i], deltaVal);
            AddToMovingList(ref movingThetaList[i], thetaVal);
            AddToMovingList(ref movingGammaList[i], gammaVal);


            //average moving lists for smoothing
            float smoothenedAlpha = movingAlphaList[i].Average();
            float smoothenedBeta = movingBetaList[i].Average();
            float smoothenedDelta = movingDeltaList[i].Average();
            float smoothenedTheta = movingThetaList[i].Average();
            float smoothenedGamma = movingGammaList[i].Average();

			//yield return null;  //separate to next frame

            //update smoothened value line graph
            smoothenedAlphaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(smoothenedAlpha, ALPHA_RANGE.x, ALPHA_RANGE.y));
            smoothenedBetaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(smoothenedBeta, BETA_RANGE.x, BETA_RANGE.y));
            smoothenedDeltaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(smoothenedDelta, DELTA_RANGE.x, DELTA_RANGE.y));
            smoothenedThetaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(smoothenedTheta, THETA_RANGE.x, THETA_RANGE.y));
            smoothenedGammaGraph[i].AddToQueueThenUpdate(MathHelper.GetPercentageOfValueInRange(smoothenedGamma, GAMMA_RANGE.x, GAMMA_RANGE.y));
        
			if(firstLoop)
			{
                if (deltaVal == 1)
                {
                    deltaValueText.text = "100";
                }
                else
                {
                    deltaValueText.text = Mathf.Round(deltaVal * 100).ToString();
                    //deltaValueText.text = deltaVal.ToString();
                }

                if (thetaVal == 1)
                {
                    thetaValueText.text = "100";
                }
                else
                {
                    thetaValueText.text = Mathf.Round(thetaVal * 100).ToString();
                    //thetaValueText.text = thetaVal.ToString();
                }

                if (alphaVal == 1)
                {
                    alphaValueText.text = "100";
                }
                else
                {
                    alphaValueText.text = Mathf.Round(alphaVal * 100).ToString();
                    //alphaValueText.text = alphaVal.ToString();
                }

                if (betaVal == 1)
                {
                    betaValueText.text = "100";
                }
                else
                {
                    betaValueText.text = Mathf.Round(betaVal * 100).ToString();
                    //betaValueText.text = betaVal.ToString();
                }


                if (gammaVal == 1)
                {
                    gammaValueText.text = "100";
                }
                else
                {
                    gammaValueText.text = Mathf.Round(gammaVal * 100).ToString();
                    //gammaValueText.text = gammaVal.ToString();
                }


                firstLoop = false;
				i = -1;
			}

			yield return null;  //separate to next frame
		}
	}
   
    private void AddToMovingList(ref Queue<float> movingList, float value)
    {
        //add value to queue
        movingList.Enqueue(value);

        //--remove old excess values
        while (movingList.Count > MOVINGLIST_MAX_LIMIT)
            movingList.Dequeue();
    }

    public void ChannelButtonBehavior(int channnel)
    {
        ShowChannel(channnel, true);
    }

    public void ShowChannel(int channel, bool updateGraph = true)
	{
		if (shownChannel == channel)
			return;
        if (updateGraph) {
            UpdateGraphs();
        }
        //uiBlocker.SetActive(true);
        for (int i = 0; i < FREQ_CHANNELS; i++)
		{
			channelParents[i].SetActive(i == channel);


		}

		shownChannel = channel;
	}
    public void ToggleDisplayFrequency(int frequencyIndex)
	{
		bool display = frequencyFilters[frequencyIndex].isOn;

		switch(frequencyIndex)
		{
			case 0: //delta
				for (int i = 0; i < FREQ_CHANNELS; i++)
				{
					deltaGraph[i].gameObject.SetActive(display);
					smoothenedDeltaGraph[i].gameObject.SetActive(display);
				}
				break;
			case 1: //theta
				for (int i = 0; i < FREQ_CHANNELS; i++)
                {
                    thetaGraph[i].gameObject.SetActive(display);
                    smoothenedThetaGraph[i].gameObject.SetActive(display);
                }
                break;
			case 2: //alpha
				for (int i = 0; i < FREQ_CHANNELS; i++)
                {
                    alphaGraph[i].gameObject.SetActive(display);
                    smoothenedAlphaGraph[i].gameObject.SetActive(display);
                }
                break;
			case 3: //beta
				for (int i = 0; i < FREQ_CHANNELS; i++)
                {
                    betaGraph[i].gameObject.SetActive(display);
                    smoothenedBetaGraph[i].gameObject.SetActive(display);
                }
                break;
			case 4: //gamma
				for (int i = 0; i < FREQ_CHANNELS; i++)
                {
                    gammaGraph[i].gameObject.SetActive(display);
                    smoothenedGammaGraph[i].gameObject.SetActive(display);
                }
                break;

		}
	}

    //getters for average
	public float GetOverallAverageAlpha()
    {
		if ((int)overallAlpha[1] == 0)
            return 0;
        else
		    return overallAlpha[0] / (int)overallAlpha[1];
    }
    public float GetOverallAverageBeta()
    {
		if ((int)overallBeta[1] == 0)
            return 0;
        else
            return overallBeta[0] / (int)overallBeta[1];
    }
    public float GetOverallAverageDelta()
    {
		if ((int)overallDelta[1] == 0)
            return 0;
        else
            return overallDelta[0] / (int)overallDelta[1];
    }
	public float GetOverallAverageTheta()
    {
		if ((int)overallTheta[1] == 0)
            return 0;
        else
            return overallTheta[0] / (int)overallTheta[1];
    }
    public float GetOverallAverageGamma()
    {
		if ((int)overallGamma[1] == 0)
            return 0;
        else
            return overallGamma[0] / (int)overallGamma[1];
    }
}
