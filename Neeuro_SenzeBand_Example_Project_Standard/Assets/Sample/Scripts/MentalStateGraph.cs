using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MentalStateGraph : MonoBehaviour
{
    private NSB_Manager nsbm;

    public Text attentionValText;
    public Text relaxationValText;
    public Text mentalWorkloadValText;

    public LineCanvas attentionGraph;
    public LineCanvas relaxationGraph;
    public LineCanvas mentalWorkloadGraph;
     
    float attentionval;
    float relaxationVal;
    float mentalWorkloadVal;

    int datagrabbed = 0;

    float[] attentionValues = new float[5];
    float[] relaxationValues = new float[5];
    float[] mentalWorkloadValues = new float[5];

    void Start()
    {
        nsbm = NSB_Manager.instance;
    }

    void Update()
    {


        // attentionValues[(int)Mathf.Round(Time.time)] = nsbm.GetAttention();
        // relaxationValues[(int)Mathf.Round(Time.time)] = nsbm.GetRelaxation();
        // mentalWorkloadValues[(int)Mathf.Round(Time.time)] = nsbm.GetMentalWL();
        //UpdateMentalStateGraph();

    }


    private void LateUpdate()
    {

    }

    public void UpdateMentalStateGraph() {
        if (datagrabbed == attentionValues.Length) {
            datagrabbed = 0;
        }

        attentionval = Mathf.Round(nsbm.GetAttention() * 100);
        relaxationVal = Mathf.Round(nsbm.GetRelaxation() * 100);
        mentalWorkloadVal = Mathf.Round(nsbm.GetMentalWL() * 100);

        if (attentionval == 1) {
            attentionval = 100;
        }

        if (relaxationVal == 1)
        {
            relaxationVal = 100;
        }

        if (mentalWorkloadVal == 1)
        {
            mentalWorkloadVal = 100;
        }

        attentionValText.text = attentionval.ToString();
        relaxationValText.text = relaxationVal.ToString();
        mentalWorkloadValText.text = mentalWorkloadVal.ToString();


       // attentionValues[datagrabbed] = nsbm.GetAttention();
      //  relaxationValues[datagrabbed] = nsbm.GetRelaxation();
      //  mentalWorkloadValues[datagrabbed] = nsbm.GetMentalWL();

      //  ConfigureGraph(attentionValues, attentionGraph);
       // ConfigureGraph(relaxationValues, relaxationGraph);
       // ConfigureGraph(mentalWorkloadValues, mentalWorkloadGraph);
      //  datagrabbed += 1;

    }

    public void ConfigureGraph(float[]graphValues,LineCanvas graph) {
        for (int i = 0; i < datagrabbed; ++i)
        {
            float[] d = new float[1];
            for (int j = 0; j < d.Length; ++j){
                d[j] = graphValues[i];
            }

            Debug.Log("length: " + d.Length + "d " + d[d.Length-1]) ;
            graph.SetData(d);   
        }
    }


}
