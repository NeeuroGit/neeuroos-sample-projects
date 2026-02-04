using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using UnityEngine.UI;

public class MentalStatePanel : MonoBehaviour
{
	public MovingLineGraph attentionGraph;
	public MovingLineGraph relaxationGraph;
	public MovingLineGraph mentalWorkloadGraph;
	public MovingLineGraph smoothenedAttentionGraph;
	public MovingLineGraph smoothenedRelaxationGraph;
	public MovingLineGraph smoothenedMentalWorkloadGraph;
    
	private NSB_Manager nsbm;
	private Queue<float> movingAttentionList;   //attention list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing
	private Queue<float> movingRelaxationList;  //relaxation list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing
	private Queue<float> movingMentalWorkloadList;  //mental workload list with MOVINGLIST_MAX_LIMIT count to be averaged for smoothing

	private Vector2 overallAttention;    //(total attention overtime, number of attention samples) to be divided together to get average
	private Vector2 overallRelaxation;    //(total relaxation overtime, number of relaxation samples) to be divided together to get average
    private Vector2 overallMentalWorkload;    //(total mental workload overtime, number of mental workload samples) to be divided together to get average

	private const int MOVINGLIST_MAX_LIMIT = 3;

	public Text attentionValText;
	public Text relaxationValText;
	public Text mentalWorkloadValText;



	private void Start()
	{
		nsbm = NSB_Manager.instance;

		nsbm.rawdataGrabbed.AddListener(UpdateGraphs);
        
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
		attentionGraph.ResetGraph();
		relaxationGraph.ResetGraph();
		mentalWorkloadGraph.ResetGraph();
		smoothenedAttentionGraph.ResetGraph();
		smoothenedRelaxationGraph.ResetGraph();
		smoothenedMentalWorkloadGraph.ResetGraph();

		ResetAveraging();
		ResetMovingLists();
	}

    private void ResetMovingLists()
	{
		movingAttentionList = new Queue<float>();
		movingRelaxationList = new Queue<float>();
		movingMentalWorkloadList = new Queue<float>();
	}

    private void ResetAveraging()
	{
		overallAttention = Vector2.zero;
		overallRelaxation = Vector2.zero;
		overallMentalWorkload = Vector2.zero;

	}

    public void UpdateGraphs()
	{
		if (SDKStateManager.instance.currentState != SDKStateManager.SDK_State.MENTAL_STATES_PANEL)
			return;

#if !UNITY_EDITOR
        if (!nsbm.GetReceiveEEGState()) //don't update graphs if live feed is OFF
            return;
#endif

		StartCoroutine(UpdateGraphs_IEnum());
	}

	private IEnumerator UpdateGraphs_IEnum()
	{
		float attentionVal = nsbm.GetAttention();
        float relaxationVal = nsbm.GetRelaxation();
        float mentalWorkloadVal = nsbm.GetMentalWL();

#if UNITY_EDITOR
        //FOR DEBUGGING PURPOSES IN EDITOR: dummy values that the "senzeband sent" to the system
        attentionVal = Random.Range(0f, 1f);
        relaxationVal = Random.Range(0f, 1f);
        mentalWorkloadVal = Random.Range(0f, 1f);
#endif
		//Debug.Log("uupdate");
		
		//attentionVal = Mathf.Round(nsbm.GetAttention() * 100);
		//relaxationVal = Mathf.Round(nsbm.GetRelaxation() * 100);
		//mentalWorkloadVal = Mathf.Round(nsbm.GetMentalWL() * 100);

		if (attentionVal == 1){
			attentionValText.text = "100";
		}
		else {
			attentionValText.text = Mathf.Round(attentionVal * 100).ToString();
		}

		if (relaxationVal == 1){
			relaxationValText.text = "100";
		}
		else {
			relaxationValText.text = Mathf.Round(relaxationVal * 100).ToString();
		}

		if (mentalWorkloadVal == 1){
			mentalWorkloadValText.text = "100";
		}
		else {
			mentalWorkloadValText.text = Mathf.Round(mentalWorkloadVal * 100).ToString();
		}


		//add values to total for averaging and increment number of samples
		overallAttention[0] += attentionVal;
        overallAttention[1]++;
        overallRelaxation[0] += relaxationVal;
        overallRelaxation[1]++;
        overallMentalWorkload[0] += mentalWorkloadVal;
        overallMentalWorkload[1]++;

        //update raw line graph
        attentionGraph.AddToQueueThenUpdate(attentionVal);
        relaxationGraph.AddToQueueThenUpdate(relaxationVal);
        mentalWorkloadGraph.AddToQueueThenUpdate(mentalWorkloadVal);

		//yield return null;  //separate to next frame

        //update moving list to be averaged for smoothing
        AddToMovingList(ref movingAttentionList, attentionVal);
        AddToMovingList(ref movingRelaxationList, relaxationVal);
        AddToMovingList(ref movingMentalWorkloadList, mentalWorkloadVal);

        //average moving lists for smoothing
        float smoothenedAttention = movingAttentionList.Average();
        float smoothenedRelaxation = movingRelaxationList.Average();
        float smoothenedMentalWorkload = movingMentalWorkloadList.Average();

		//yield return null;  //separate to next frame

        //update smoothened value line graph
        smoothenedAttentionGraph.AddToQueueThenUpdate(smoothenedAttention);
        smoothenedRelaxationGraph.AddToQueueThenUpdate(smoothenedRelaxation);
        smoothenedMentalWorkloadGraph.AddToQueueThenUpdate(smoothenedMentalWorkload);

		yield return null;  //separate to next frame
	}

	private void AddToMovingList(ref Queue<float> movingList, float value)
	{
		//add value to queue
		movingList.Enqueue(value);

        //--remove old excess values
		while (movingList.Count > MOVINGLIST_MAX_LIMIT)
			movingList.Dequeue();
	}

    //getters for average
    public float GetOverallAverageAttention()
	{
		if ((int)overallAttention[1] == 0)
			return 0;
		else
		    return overallAttention[0] / (int)overallAttention[1];
	}
	public float GetOverallAverageRelaxations()
    {
		if ((int)overallRelaxation[1] == 0)
            return 0;
        else
            return overallRelaxation[0] / (int)overallRelaxation[1];
    }
	public float GetOverallAverageMentalWorkload()
    {
		if ((int)overallMentalWorkload[1] == 0)
            return 0;
        else
			return overallMentalWorkload[0] / (int)overallMentalWorkload[1];
    }
}
