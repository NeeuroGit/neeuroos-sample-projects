using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sample usage file. To control the demo Scan panel
/// </summary>
public class ScanPanelController : MonoBehaviour {

	private NSB_Manager nsbm;
	public Text btStatusText;
	public Text scanStatusText;
	public Button scanButton;
	public GameObject availSBButton;
    public GameObject availSBButtonPlaceHolder;

    private List<GameObject> availSBButtonList;
	private bool bIsReady;

	// Use this for initialization
	void Start () {
		nsbm = NSB_Manager.instance;
		bIsReady = false;
		availSBButtonList = new List<GameObject>();
        //hide this button reference for instantiating
        if (availSBButton!=null)
			availSBButton.SetActive (false);
	}
		

	// Update is called once per frame
	void Update () {

		//Performs one-time processes when NSB init is completed.
		if (!bIsReady) {
			if (nsbm.IsInitCompleted()) {
				//Just Ready!
				bIsReady = true;
				scanButton.onClick.AddListener (ToggleScan);
			} else {
				//NSB init is not ready yet, so skip all following processes
				return;
			}
		}

		if (nsbm.IsBluetoothEnabled ()) {

			if (nsbm.IsScanning())
				scanStatusText.text = "Scanning";
			else
				scanStatusText.text = "Not scanning";


			if (btStatusText.text != "BT is ON") {
				//BT is switched from OFF to ON
				btStatusText.text = "BT is ON";
			}
		} else {
			if (btStatusText.text != "BT is OFF") {
				//BT is switched from ON to OFF
				btStatusText.text = "BT is OFF";
				//update of scanning status
				if (nsbm.IsScanning ()) {
					ToggleScan ();
				}
			}
		}
		

		//To manage, create and destroy list of buttons of available SenzeBands detected.
		if ( availSBButtonList.Count != nsbm.listAvailableDevices.Count ) 
		{
			//Different number of SB avail
			//Debug.Log("NSB  Button List has "+availSBButtonList.Count + " ; NSBm List has "+nsbm.listAvailableDevices.Count);

			if (nsbm.listAvailableDevices.Count > availSBButtonList.Count) 
			{
                //To add 1 SB button
                //Debug.Log("NSB  add 1 button to current "+availSBButtonList.Count);
                Debug.Log("Instantiate Button");

                GameObject buttonObj;
                buttonObj = Instantiate(availSBButton, this.transform);
                buttonObj.transform.SetParent(availSBButtonPlaceHolder.transform);
                buttonObj.GetComponentInChildren<Text>().text = nsbm.listAvailableDevices[availSBButtonList.Count];
                buttonObj.GetComponent<Button>().onClick.AddListener(() => { nsbm.ConnectSB(buttonObj.GetComponentInChildren<Text>().text); });
                buttonObj.SetActive(true);
                availSBButtonList.Add(buttonObj);
              
			}
			if (nsbm.listAvailableDevices.Count < availSBButtonList.Count && availSBButtonList.Count > 0) {
				//clear all availSBButtonList
				//Debug.Log("NSB  remove all "+availSBButtonList.Count+" buttons");
				for (int i = availSBButtonList.Count-1; i >= 0; --i) {
					Destroy (availSBButtonList [i]);
				}
				availSBButtonList.Clear ();
					
			}

		}
			
	}

	public void ToggleScan()
	{
        Debug.Log("Button Scan?");
		if (nsbm.IsScanning ()) {
			nsbm.SetScanning (false);
			scanStatusText.text = "Not scanning";
		} else {
			if (nsbm.IsBluetoothEnabled ()) {
				nsbm.SetScanning (true);
				scanStatusText.text = "Scanning";
			}
		}
	}
}
