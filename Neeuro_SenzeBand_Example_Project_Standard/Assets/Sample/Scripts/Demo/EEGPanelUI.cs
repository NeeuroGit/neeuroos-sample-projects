using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sample usage file. Displays data collected from the Senzeband
/// </summary>
public class EEGPanelUI : MonoBehaviour {

	private NSB_Manager nsbm;

   
	public Text DeltaValue;
	public Text ThetaValue;
	public Text AlphaValue;
	public Text BetaValue;
	public Text GammaValue;

    [Space]
    public Text EEGImpedanceValueCh1;
    public Text EEGImpedanceValueCh2;
    public Text EEGImpedanceValueCh3;
    public Text EEGImpedanceValueCh4;
    public GameObject ChannelValueHolder;
    public GameObject EEGImpedanceLabel;
	
	private ConnectionColorIndicatorManager colorConnectionManager;

	// Use this for initialization
	void Start () {
        nsbm = NSB_Manager.instance;
		colorConnectionManager = ConnectionColorIndicatorManager.instance;
    }

	// Update is called once per frame
	void Update()
	{

		if (nsbm.GetReceiveEEGState())
		{

			DeltaValue.text = Mathf.Round(nsbm.GetFrequencyBand(0, 0) * 100).ToString();
			ThetaValue.text = Mathf.Round(nsbm.GetFrequencyBand(0, 1) * 100).ToString();
			AlphaValue.text = Mathf.Round(nsbm.GetFrequencyBand(0, 2) * 100).ToString();
			BetaValue.text = Mathf.Round(nsbm.GetFrequencyBand(0, 3) * 100).ToString();
			GammaValue.text = Mathf.Round(nsbm.GetFrequencyBand(0, 4) * 100).ToString();
			UpdateImpedanceStatus();

        }
		else
		{		
		

			DeltaValue.text = "-"; 

			ThetaValue.text = "-";
			AlphaValue.text = "-";
			BetaValue.text = "-";
			GammaValue.text = "-";

            EEGImpedanceLabel.SetActive(false);
            ChannelValueHolder.SetActive(true);

            EEGImpedanceValueCh1.text = "";
            EEGImpedanceValueCh2.text = "";
            EEGImpedanceValueCh3.text = "";
            EEGImpedanceValueCh4.text = "";

        }


	}


    private void UpdateImpedanceStatus()
    {


        if (CommandHandler.instance.isImpedanceCheckOn)
        {
            float[] impedance = nsbm.GetEEGImpedance();
            EEGImpedanceValueCh1.text = formatOhms(impedance[0]) + " kΩ";
            EEGImpedanceValueCh2.text = formatOhms(impedance[1]) + " kΩ";
            EEGImpedanceValueCh3.text = formatOhms(impedance[2]) + " kΩ";
            EEGImpedanceValueCh4.text = formatOhms(impedance[3]) + " kΩ";
            ChannelValueHolder.SetActive(false);
            EEGImpedanceLabel.SetActive(true);


        }
        else
        {
            ChannelValueHolder.SetActive(true);
            EEGImpedanceLabel.SetActive(false);
            EEGImpedanceValueCh1.text = "";
            EEGImpedanceValueCh2.text = "";
            EEGImpedanceValueCh3.text = "";
            EEGImpedanceValueCh4.text = "";
           
        }



    }

    private string formatOhms(float value)
    {
        return (value / 1000).ToString("#0.0");
    }
}
