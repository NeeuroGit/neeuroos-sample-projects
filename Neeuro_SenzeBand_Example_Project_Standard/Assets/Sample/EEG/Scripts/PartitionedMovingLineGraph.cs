using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartitionedMovingLineGraph : MonoBehaviour
{
	public MovingLineGraph[] movingLineGraphs;  //first element should be right-most

	private void Awake()
	{
		Debug.Log(-(GetComponent<RectTransform>().rect.width / 1000f));
		GetComponent<HorizontalLayoutGroup>().spacing = -(GetComponent<RectTransform>().rect.width / 1000f);
	}

	//resets graph to new and straight horizontal line graph
	public void ResetGraph()
    {
		for (int i = 0; i < movingLineGraphs.Length; i++)
		{
			movingLineGraphs[i].ResetGraph();
		}
    }
    
	public void AddToQueue(float percentage)
    {
		float transportingValue = movingLineGraphs[0].AddToQueue(percentage,true);   //will handle the value that gets removed from one queue and will be enqueued to next line graph
		for (int i = 1; i < movingLineGraphs.Length; i++)
		{
			if (transportingValue >= 0) //if transporting value is positive (means that there is value to be transported, else, it's -1)
				transportingValue = movingLineGraphs[i].AddToQueue(transportingValue, true);
			else
				break;
		}
    }

    public void InsertToQueue(float[] arr)
	{
		float[] arr1 = new float[50];
		System.Array.Copy(arr, 0, arr1, 0, 50);

		movingLineGraphs[2].AssignArr(arr1);
		float[] arr2 = new float[101];
		arr2[0] = arr1[49];
		System.Array.Copy(arr, 50, arr2, 1, 100);
		movingLineGraphs[1].AssignArr(arr2);

		float[] arr3 = new float[101];
		arr3[0] = arr2[100];
		System.Array.Copy(arr, 150, arr3, 1, 100);
		movingLineGraphs[0].AssignArr(arr3);
	}

    public void UpdateGraph()
	{
		for (int i = 0; i < movingLineGraphs.Length; i++)
		{
			if (movingLineGraphs[i].ThereIsValueToBeUpdatedToGraph)
				movingLineGraphs[i].UpdateGraph();
		}
	}
}
