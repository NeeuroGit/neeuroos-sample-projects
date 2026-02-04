using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MovingLineGraph : MonoBehaviour
{
	public int maxNumberOfElements = 100;
	public float startingYPercentageValue = 0;  //when x percentage/value has no y percentage yet, this is default y percentage
	public bool useListInsteadQueue;    //if TRUE, use list "yPercentages_List"... else, use queue "yPercentages_Queue"

	private Queue<float> yPercentages_Queue;   //queue that holds positions of points from bottom to top of graph (value range: 0.0 to 1.0)
	private List<float> yPercentages_List;      //list that holds positions of points from bottom to top of graph
   
    //UI
	public LineRenderer lr;
	public RectTransform rt;

	public bool ThereIsValueToBeUpdatedToGraph { set; get; }    //flag to tell if the graph is necessary to be updated
	private bool UpdatingGraph { set; get; }    //flag to lock queue from being modified while graph is updating



	private void Awake()
	{      
		//ResetGraph();
	
	}

    private void Start()
    {
        ResetGraph();
    }
    //resets graph to new and straight horizontal line graph
    public void ResetGraph()
	{
		//setup line renderer scaling
		lr.transform.localScale = new Vector3(rt.rect.width, rt.rect.height, 1);
        Debug.Log(rt.rect.width + " HERE " + rt.rect.height);
        Debug.Log(rt.sizeDelta + " 2HERE " + rt.sizeDelta.y);
        //setup line renderer points
        lr.positionCount = maxNumberOfElements;

		if (useListInsteadQueue)
			yPercentages_List = new List<float>();
		else
		    yPercentages_Queue = new Queue<float>();
        
		Vector3[] points = new Vector3[maxNumberOfElements];
        
		for (int i = 0; i < maxNumberOfElements; i++)
		{
			points[i] = (Vector3)PercentageToGraphVector(i, startingYPercentageValue);
		}
		lr.SetPositions(points);

		ThereIsValueToBeUpdatedToGraph = false;
		UpdatingGraph = false;
	}

	public float AddToQueue(float percentage, bool hasExcessOverlap = false, bool blankValue = false)
	{
		if (UpdatingGraph)
			return -1;

		//add value to queue/list
		if (useListInsteadQueue)
			yPercentages_List.Add(percentage);
		else
		    yPercentages_Queue.Enqueue(percentage);

        

		ThereIsValueToBeUpdatedToGraph = true;
        
		//--remove old excess values
		float removedValue = -1;
		if (hasExcessOverlap)
		{
			if (useListInsteadQueue)
			{
				while (yPercentages_List.Count > maxNumberOfElements)
				{
					yPercentages_List.RemoveAt(0);
				}
				
				if (yPercentages_List.Count > maxNumberOfElements - 1)
					removedValue = yPercentages_List[0];
			}
			else
			{
				while (yPercentages_Queue.Count > maxNumberOfElements)
				{
					yPercentages_Queue.Dequeue();
				}

				if (yPercentages_Queue.Count > maxNumberOfElements - 1)
					removedValue = yPercentages_Queue.Peek();
			}
			
		}
		else
		{
			if (useListInsteadQueue)
			{
				while (yPercentages_List.Count > maxNumberOfElements)
				{
					removedValue = yPercentages_List[0];
					yPercentages_List.RemoveAt(0);
				}
			}
			else
			{
				while (yPercentages_Queue.Count > maxNumberOfElements)
				{
					removedValue = yPercentages_Queue.Dequeue();
				}
			}
		}

		return removedValue;
	}
   
    public void AddToQueueThenUpdate(float percentage)
	{
		//add value to queue
		AddToQueue(percentage);

		//call UpdateGraph() to reflect the queue's values to UI Line Renderer
		UpdateGraph();
	}   

	//updates the graph (UI Line Renderer points) according to the list
    //method 0 = per point method
    //method 1 = burst method
    public void UpdateGraph(int method= 0)
	{
		if (UpdatingGraph)
			return;
		
		UpdatingGraph = true;
        
		//i starts at index where y value count reaches from rightmost of graph UI
		float[] yPercentagesArr;
		if (useListInsteadQueue)
			yPercentagesArr = yPercentages_List.ToArray();
		else
			yPercentagesArr = yPercentages_Queue.ToArray();
		int ctr = 0;

		if (method == 0)
		{
			//PER POINT METHOD: (Assign to graphic as loop iterates)
			for (int i = maxNumberOfElements - yPercentagesArr.Length; i < maxNumberOfElements; i++)
			{
				lr.SetPosition(i, PercentageToGraphVector(i, yPercentagesArr[ctr]));    //assign position to graphic
				ctr++;
			}
		}
		else if(method == 1)
		{
			
			//BURST METHOD: (Assign to graphic by the end of loop iterations)
			Vector3[] points = new Vector3[maxNumberOfElements];
			lr.GetPositions(points);   //make copy of current points
			for (int i = maxNumberOfElements - yPercentagesArr.Length; i < maxNumberOfElements; i++)
			{
				points[i] = PercentageToGraphVector(i, yPercentagesArr[ctr]);  
				ctr++;
			}
			lr.SetPositions(points);    //reassign modified copy of points to line renderer

		}
		ThereIsValueToBeUpdatedToGraph = false; //reset flag to false since graph has just been updated

		UpdatingGraph = false;
	}
    
    private Vector2 PercentageToGraphVector(float x, float y)
	{
		//using line renderer:
		return new Vector2(x / (maxNumberOfElements - 1), y);
        
		//using UI line renderer:
		//return new Vector2(rt.rect.width * (x / (maxNumberOfElements - 1)), rt.rect.height * y);
	}   

    public void AssignArr(float[] arr)
	{
		if(useListInsteadQueue)
		{
			if(arr.Length == maxNumberOfElements)
			    yPercentages_List = new List<float>(arr);
			else
			{
				if (yPercentages_List.Count + arr.Length <= maxNumberOfElements)
					yPercentages_List.AddRange(arr);
				else
				{
					int offset = yPercentages_List.Count + arr.Length - maxNumberOfElements;
					int ctr = 0;
					for (int i = yPercentages_List.Count - offset; i < yPercentages_List.Count; i++)
					{
						yPercentages_List[i] = arr[ctr];
						ctr++;
					}
					for (int i = ctr; i < arr.Length; i++)
					{
						yPercentages_List.Add(arr[i]);
					}
				}
			}
		}

		UpdateGraph(1);
	}


}
