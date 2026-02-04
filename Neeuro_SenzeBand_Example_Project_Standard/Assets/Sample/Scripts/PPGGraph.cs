using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

public class PPGGraph : MonoBehaviour
{
	public int PPG_maxClamp_Infared ;
	public int PPG_maxClamp_Red ;

	public int PositiveYLimit ;
	public int NegativeYLimit ;

	public MovingLineGraph ppg_Infared;
	public MovingLineGraph ppg_red;
	private NSB_Manager nsbm;

	private const int MOVINGLIST_MAX_LIMIT = 3;

	bool hasInfaredGraphStarted = false;
	bool hasRedGraphStarted = false;
	bool isDrawingGraph_Infared = false;
	bool isDrawingGraph_red = false;


	public LineRenderer infaredPPGLine;
	public LineRenderer redPPGLine;

	List<float> global_IrValue = new List<float>();
	List<float> global_RedValue = new List<float>();
	string irString = "Overall Infared Values: ";
	string redString = "Overall Red Values: ";
	float startingThousands;
	bool isThousandsModified;

	public Vector3 InfaredTopPos;
	public Vector3 InfaredMidPos;
	public Vector3 InfaredBottomPos;


	public Vector3 redTopPos;
	public Vector3 redMidPos;
	public Vector3 redBottomPos;

	int movingAverageLength = 25;
	float movingAverage;

	int valueCount = 0;
	List<float> listPPG = new List<float>();
	int ppgElement = 0;

	int testcount = 0;


	//float[] irValue = {128000,128021,128060};

	private void Start()
	{
		if(nsbm == null)
			nsbm = NSB_Manager.instance;
		nsbm.ppgdataGrabbed.AddListener(UpdatePPGGraphs);
		global_IrValue.Clear();
		global_RedValue.Clear();
		
	}
	
	private void Update()
	{

#if UNITY_EDITOR
		//FOR DEBUGGING PURPOSES IN EDITOR: Press SPACE to act as SenzeBand "sending" values to system
		/*
		if (Input.GetKeyDown(KeyCode.Space))
		UpdateGraphs();
		if (Input.GetKeyDown(KeyCode.Q)) {
			//ModifyGraphValue(irValue[testcount], ppg_Infared, infaredPPGLine);
			StartCoroutine(UpdateGraph_IEnum(irValue[testcount], ppg_Infared, infaredPPGLine));
			testcount += 1;
		}
		*/	


#endif
	}


    public void ResetGraphs()
	{
		ppg_red.ResetGraph();
		ppg_Infared.ResetGraph();
	}

	public void UpdatePPGGraphs()
	{
		if (SDKStateManager.instance.currentState != SDKStateManager.SDK_State.PPG_PANEL)
			return;
		Debug.Log("PPGTrack: Updating PPG graph");
		//UpdateGraphs();
		UpdateGraphs(nsbm.GetRawPPG());

	}

	public void UpdateGraphs(List<int[]> rawPPG)
	{
		float[] irValue = new float[rawPPG.Count];
		float[] redValue = new float[rawPPG.Count];

		float redVal =0;
		float irVal = 0;

		for (int i = 0; i < rawPPG.Count; i++){
			irValue[i] = rawPPG[i][0];
			redValue[i] = rawPPG[i][1];

			irVal =  rawPPG[i][0];
			redVal = rawPPG[i][1];

			global_IrValue.Add(irValue[i]);
			global_RedValue.Add(redValue[i]);
			Debug.Log("analyzePPGData IRdata: "+ rawPPG[i][0] + " Reddata: " + rawPPG[i][1]);
		}
		
		
		/*
		
		*/

		
		/*
		float[] irValue = {120810,120815,120808,120818,120851,120878,120894,120929,120948,120969,120973,120867,120772,120727,120705,
						   120699,120716,120724,120716,120729,120750,120775,120804,120819,120846,120881,120884,120832,120735,120681,
						   120658,120637,120644,120654,120662,120685,120713,120735,120762,120789,120826,120849,120882,120889,120797,
						   120711,120664,120639,120645,120666,120686,120710,120737,120768,120816,120833,120879,120905,120934,120945,
						   120840,120740,120689,120667,120667,120692,120696,120717,120742,120766,120811,120842,120865,120906,120945,
						   120926,120820,120716,120679,120651,120658,120685,120695,120706,120568,120763,120794,120825,120863,120897,
						   120973,120875,120753,120665,120630,120617,120629,120641,120657,120673,120707,120741,120772,120813,120841,
						   120875,120900,120836,120709,120623,120578,120561,120566,120583,120601,120611,120645,120685,120718,120751,
						   120780,120818,120847,120842,120720,120600,120551,120520,120508,120527,120549,120563,120583,120624,120661,
						   120692,120713,120753,120789,120824,120813,120699,120590,120534,120507,120490,120516,120530,120538,120564,
						   120595,120638,120667,120708,120737,120766,120809,120841,120779,120649,120574,120528,120517,120511,120535,
						   120551,120561,120592,120628,120660,120696,120730,120757,120710,120815,120835,120673,120604,120549,120517,
						   120515,120551,120550,120570,120583,120600,120643,120672,120700,120724,120765,120801,120835,120834,120737,
						   120636,120574,120545,120553,120565};
		
		

		//float[] irValue = {128060};
		
		float[] redValue = {132285,132292,132306,132314,132338,132351,132361,132381,132383,132401,
							132424,132416,132341,132304,132278,132261,132233};
		
		*/

		infaredPPGLine.transform.localScale = new Vector3(infaredPPGLine.transform.localScale.x, 0.3f, 1);
		redPPGLine.transform.localScale = new Vector3(redPPGLine.transform.localScale.x, 0.3f, 1);


		float[] modifiedIrValue = new float[irValue.Length];
		float[] modifiedRedValue = new float[redValue.Length];


		ModifyGraphValue(irVal, ppg_Infared, infaredPPGLine);
		ModifyGraphValue(redVal, ppg_red, redPPGLine);
		Debug.Log("PPGTrack: AnalyzeData");

	}


	public void ModifyGraphValue(float originalValue, MovingLineGraph currGraph, LineRenderer currLineRenderer)
	{
			float currHundreds = originalValue % 1000;
			if (currHundreds == 000) {
				currHundreds = 100;
			}
			Debug.Log("h "+currHundreds);
			float valuetoGraph = 0;
			float currThousands = Mathf.Floor(originalValue * 0.001f);

			if (!isThousandsModified)
			{
				startingThousands = Mathf.Floor(originalValue * 0.001f);
			}


			if (currThousands == startingThousands)
			{
				valuetoGraph = currHundreds;
			}
			else
			{
				float thousandsDifference = currThousands - startingThousands;
				valuetoGraph = currHundreds + thousandsDifference * 1000;

				if (currThousands > startingThousands + 10000)
				{
					Debug.Log("start " + startingThousands + " g " + currGraph);
					if (currThousands > startingThousands * startingThousands)
					{
						currLineRenderer.transform.localScale = new Vector3(currLineRenderer.transform.localScale.x, 0.5f, 1);
						startingThousands = currThousands * 0.001f;
						Debug.Log("stt " + startingThousands);
						isThousandsModified = true;
					}
				}

			}

			valuetoGraph /= 5;


			if (valuetoGraph > PositiveYLimit)
			{
				valuetoGraph = PositiveYLimit;


			}

			if (valuetoGraph < NegativeYLimit && valuetoGraph < 0)
			{
				valuetoGraph = NegativeYLimit;
			}

			StartCoroutine(UpdateGraph_IEnum(valuetoGraph,currGraph, currLineRenderer));
	}
	private IEnumerator UpdateGraph_IEnum(float ppgValue, MovingLineGraph graphToUpdate, LineRenderer currLineRenderer)
	{
		//yield return new WaitForEndOfFrame();
		if (graphToUpdate == ppg_Infared && isDrawingGraph_Infared)
		{
			yield break;
		}

		if (graphToUpdate == ppg_red && isDrawingGraph_red)
		{
			yield break;
		}

		if (currLineRenderer == infaredPPGLine)
		{
			currLineRenderer.GetComponent<RectTransform>().anchoredPosition = InfaredMidPos;
		}
		if (currLineRenderer == redPPGLine)
		{
			currLineRenderer.GetComponent<RectTransform>().anchoredPosition = redMidPos;
		}

		//yield return new WaitForSeconds(0.6f);
		if (graphToUpdate == ppg_Infared)
		{
			isDrawingGraph_Infared = true;
		}

		if (graphToUpdate == ppg_red)
		{
			isDrawingGraph_red = true;
		}


		float currVal = 0;
		listPPG.Add(ppgValue);

	

		Debug.Log("count " + listPPG.Count);

		for (int z = 0; z < listPPG.Count; z++) {
			Debug.Log("ppg c " + listPPG[z]);
		}
		if (listPPG.Count <= movingAverageLength)
		{
			if (listPPG.Count == 1)
			{
				currVal = ppgValue;
			}
			else
			{
				Debug.Log(" test: before subtract " + ppgValue);
				float meanValue = Mean(listPPG.ToArray());

				Debug.Log(" test: mean value " + meanValue);
				currVal = ppgValue - meanValue;
				Debug.Log(" test: after subtract " + currVal);
			}
		}
		else
		{
			ppgElement += 1;
			List<float> listValuesToMean = new List<float>();
			Debug.Log(" test: before subtract " + ppgValue);
			for (int y = 0; y < listPPG.Count; y++)
			{
				if (y < ppgElement)
				{
						listValuesToMean.Add(listPPG[y]);
				}
			}


			float meanValue = Mean(listValuesToMean.ToArray());
			Debug.Log(" test: mean value " + meanValue);

			currVal = ppgValue - meanValue;
			Debug.Log(" test: after subtract " + currVal);

		}



		Debug.Log("value that will be drawn " + currVal);
		graphToUpdate.AddToQueueThenUpdate(currVal);
		yield return new WaitForSeconds(0.03f);

		if (graphToUpdate == ppg_Infared)
		{
			if (!hasInfaredGraphStarted)
			{
				ChangeGraphLine(graphToUpdate, listPPG[0]);
				hasInfaredGraphStarted = true;

			}
		}

		if (graphToUpdate == ppg_red)
		{
			if (!hasRedGraphStarted)
			{
				ChangeGraphLine(graphToUpdate, listPPG[0]);
				hasRedGraphStarted = true;
			}

		}


		if (graphToUpdate == ppg_Infared)
		{
			isDrawingGraph_Infared = false;
		}

		if (graphToUpdate == ppg_red)
		{
			isDrawingGraph_red = false;
		}
		if (currLineRenderer == infaredPPGLine)
		{
			StartCoroutine(changeLine(currLineRenderer.GetComponent<RectTransform>(), InfaredMidPos));
		}
		if (currLineRenderer == redPPGLine)
		{
			StartCoroutine(changeLine(currLineRenderer.GetComponent<RectTransform>(), redMidPos));
		}
	}


	//original
	public void ModifyGraphValues(float[] originalValues, float[] modifiedValues, MovingLineGraph currGraph, LineRenderer currLineRenderer) {
		for (int x = 0; x < originalValues.Length; x++)
		{
			modifiedValues[x] = originalValues[x];
		}

		for (int x = 0; x < originalValues.Length; x++)
		{
			float currHundreds = originalValues[x] % 1000;
			float valuetoGraph = 0;
			float currThousands = Mathf.Floor(originalValues[x] * 0.001f);

			if (!isThousandsModified) {
				startingThousands = Mathf.Floor(originalValues[0] * 0.001f);
			}
			

			if (currThousands == startingThousands){
				valuetoGraph = currHundreds;
			}
			else
			{
			
				float thousandsDifference = currThousands - startingThousands;
				valuetoGraph = currHundreds + thousandsDifference * 1000;

				if (currThousands > startingThousands +10000)
				{
					Debug.Log("start " + startingThousands + " g " + currGraph);

					if (currThousands > startingThousands * startingThousands)
					{
						currLineRenderer.transform.localScale = new Vector3(currLineRenderer.transform.localScale.x, 0.5f, 1);
						startingThousands = currThousands * 0.001f;
						Debug.Log("stt " + startingThousands);
						isThousandsModified = true;
					}
				}
			}

			valuetoGraph /= 5;


			if (valuetoGraph > PositiveYLimit)
			{
				valuetoGraph = PositiveYLimit;
			}

			if (valuetoGraph < NegativeYLimit && valuetoGraph < 0)
			{
					valuetoGraph = NegativeYLimit;
	
			}
			
			modifiedValues[x] = valuetoGraph;
		}
		StartCoroutine(UpdateGraphs_IEnum(modifiedValues, currGraph, currLineRenderer));
	}

	private IEnumerator UpdateGraphs_IEnum(float[] ppgValues, MovingLineGraph graphToUpdate,LineRenderer currLineRenderer)
	{
		yield return new WaitForEndOfFrame();
		if (graphToUpdate == ppg_Infared && isDrawingGraph_Infared)
		{
			yield break;
		}

		if (graphToUpdate == ppg_red && isDrawingGraph_red)
		{
			yield break;
		}
		
	
		if (currLineRenderer == infaredPPGLine)
		{
			currLineRenderer.GetComponent<RectTransform>().anchoredPosition = InfaredMidPos;
			Debug.Log("should show me");
		}
		if (currLineRenderer == redPPGLine)
		{
			currLineRenderer.GetComponent<RectTransform>().anchoredPosition = redMidPos;
		}

		if (graphToUpdate == ppg_Infared) {
			isDrawingGraph_Infared = true;
		}

		if (graphToUpdate == ppg_red){
			isDrawingGraph_red = true;
		}
		if (graphToUpdate == ppg_Infared)
		{
			if (!hasInfaredGraphStarted)
			{
				ChangeGraphLine(graphToUpdate, ppgValues[0]);
				hasInfaredGraphStarted = true;

			}
		}

		if (graphToUpdate == ppg_red)
		{
			if (!hasRedGraphStarted)
			{
				ChangeGraphLine(graphToUpdate, ppgValues[0]);
				hasRedGraphStarted = true;
			}
		}
		float[] ppgValuesModified = new float[ppgValues.Length];

		for (int x = 0; x < ppgValues.Length; x++)
		{
			ppgValuesModified[x] = ppgValues[x];
		}

		for (int x = 0; x < ppgValues.Length; x++){
			Debug.Log("Value to graph before Mean Logic " + ppgValues[x]);

			if (x <= movingAverageLength - 1)
			{
				Debug.Log("Value from mean less than 25 before subtraction " + ppgValuesModified[x]);
				float meanValue = Mean(ppgValues);
				Debug.Log("Value of Mean " + meanValue);

				ppgValuesModified[x] = ppgValuesModified[x] - meanValue;

				Debug.Log("Value from mean less than 25 after subtraction " + ppgValuesModified[x]);
			}
			else
			{
				ppgElement += 1;
				List<float> listValuesToMean = new List<float>(); 
				for (int y = 0; y < ppgValues.Length; y++)
				{
					if (y < ppgElement)
					{
						listValuesToMean.Add(ppgValues[y]);
					}
				}
				Debug.Log("Value from mean more than 25 before subtraction" + ppgValues[x]);

				float meanValue = Mean(listValuesToMean.ToArray());
				Debug.Log("Value of Mean " + meanValue);

				ppgValuesModified[x] = ppgValuesModified[x]-meanValue;
				
				Debug.Log("Value from mean more than 25 after subtraction" + ppgValuesModified[x]);
			}

			float currVal = ppgValuesModified[x];

			Debug.Log("value that will be drawn " + currVal);

			graphToUpdate.AddToQueueThenUpdate(currVal);
			Debug.Log("PPGTrack: Draw PPG graph");
			yield return new WaitForSeconds(0.03f);

		}

	

		if (graphToUpdate == ppg_Infared){
			isDrawingGraph_Infared = false;
		}

		if (graphToUpdate == ppg_red){
			isDrawingGraph_red = false;
		}
		if (currLineRenderer == infaredPPGLine){
			StartCoroutine(changeLine(currLineRenderer.GetComponent<RectTransform>(), InfaredMidPos));
		}
		if (currLineRenderer == redPPGLine){
			StartCoroutine(changeLine(currLineRenderer.GetComponent<RectTransform>(), redMidPos));
		}
	}

	public float Mean(float[] valuesToCompute) {
		
		float sum = 0;
		for (int x = 0; x < valuesToCompute.Length; x++) {
			sum += valuesToCompute[x];
			Debug.Log("test in mean sum " + sum);
		}

		float average = sum / valuesToCompute.Length;
		Debug.Log("test in mean ave " + average);

		return average;
	}



	IEnumerator changeLine(RectTransform rt, Vector3 newpos)
	{
		rt.anchoredPosition = newpos;
		yield return new WaitForSeconds(1);

	}

	public void ChangeGraphLine(MovingLineGraph graphToUpdate, float startingValue) {
		graphToUpdate.startingYPercentageValue = startingValue;
		graphToUpdate.ResetGraph();
		infaredPPGLine.transform.localScale = new Vector3(infaredPPGLine.transform.localScale.x, 0.3f, 1);
		redPPGLine.transform.localScale = new Vector3(redPPGLine.transform.localScale.x, 0.3f, 1);

	}



	private void AddToMovingList(ref Queue<float> movingList, float value)
	{
		Debug.Log("Value " + value);
		//add value to queue
		movingList.Enqueue(value);

        //--remove old excess values
		while (movingList.Count > MOVINGLIST_MAX_LIMIT)
			movingList.Dequeue();
	}
}
