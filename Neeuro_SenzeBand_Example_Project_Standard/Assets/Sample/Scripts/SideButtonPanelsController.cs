using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SideButtonPanelsController : MonoBehaviour
{
    //Functions for actions that happen when the side button panels are pressed can be found here

    public CanvasGroup SBControlPanel;

    //EEG
    public CanvasGroup EEGDataPanelCanvas;
    public GameObject EEGButtons;
    public List<LineRenderer> eegGraphs = new List<LineRenderer>();

    //PPG
    public CanvasGroup PPGCanvas;
    public List<LineRenderer> ppgGraphs = new List<LineRenderer>();
    public GameObject PPGButtons;

    public GameObject StatusPanel;

    //ABDT
    public CanvasGroup ABDTDataPanelCanvas;
    public GameObject ABDTButtons;
    public List<LineRenderer> abdtGraphs = new List<LineRenderer>();

    //MENTAL STATES
    public CanvasGroup MentalStatesDataPanelCanvas;
    public List<LineRenderer> mentalStatesGraphs = new List<LineRenderer>();

    public CanvasGroup MotionSensorCalibratorPanel;

    public bool isReceivingData;


    public List<GameObject> sidePanelButtons = new List<GameObject>();

    public Color selectedColor;
    public Color deSelectedColor;

    public Sprite selectedSprite;
    public Sprite deSelectedSprite;  

    public GameObject panelResizeEEG;
    public GameObject panelResizeABDT;
    public GameObject panelResizePPG;
    public GameObject panelResizeMentalStates;
    public GameObject panelResizeMotionControl;

    public GameObject impedanceCheckPopup;

    public void Start()
    {

    }

    public void ResizePanels()
    {
        //Resize for EEG panel and EEG button
        panelResizeEEG.transform.localScale = new Vector3(0.0573658f, 0.129992f, 0.06709447f);
        EEGButtons.transform.localScale = new Vector3(0.86343f, 0.86343f, 0.86343f);
        EEGButtons.GetComponent<RectTransform>().anchoredPosition = new Vector3(793, 180, -320);

        //Resize for ABDT panel and ABDT button
        panelResizeABDT.transform.localPosition = new Vector3(-55.6f, -72, 44);
        panelResizeABDT.transform.localScale = new Vector3(0.07364871f, 0.0736487f, 0.09206086f);
        ABDTButtons.GetComponent<RectTransform>().anchoredPosition = new Vector3(594, 50, 0);

        //Resize for PPG panel
        panelResizePPG.transform.localScale = new Vector3(0.05376023f, 0.1218217f, 0.06287744f);

        //Resize for Mental States Panel
        panelResizeMentalStates.transform.localScale = new Vector3(0.05712662f, 0.12945f, 0.06681473f);

        //Resize for Motion Sensor States Panel
        panelResizeMotionControl.transform.localScale = new Vector3(0.87586f, 0.87586f, 0.87586f);
        panelResizeMotionControl.GetComponent<RectTransform>().anchoredPosition = new Vector3(-300, -76, 0);

    }

    public void ChangeToSBControlPanel()
    {
        if(CommandHandler.instance.isImpedanceCheckOn)
        {
            impedanceCheckPopup.SetActive(true);
            return;
        }
        ChangeButtonColor(sidePanelButtons[0]);

        SDKStateManager.instance.currentState = SDKStateManager.SDK_State.SENZEBAND_CONTROL_PANEL;
        SBControlPanel.alpha = 1;
        SBControlPanel.transform.SetAsLastSibling();

        TogglePanel(EEGDataPanelCanvas, eegGraphs, false);
        EEGButtons.gameObject.SetActive(false);

        TogglePanel(ABDTDataPanelCanvas, abdtGraphs, false);
        ABDTButtons.gameObject.SetActive(false);

        TogglePanel(PPGCanvas, ppgGraphs, false);
        PPGButtons.gameObject.SetActive(false);


        TogglePanel(MentalStatesDataPanelCanvas, mentalStatesGraphs, false);
        MotionSensorCalibratorPanel.alpha = 0;
        StatusPanel.gameObject.SetActive(false);
    }

    public void ChangeToEEGDataPanel()
    {
        ChangeButtonColor(sidePanelButtons[1]);
        SDKStateManager.instance.currentState = SDKStateManager.SDK_State.EEG_PANEL;
        TogglePanel(EEGDataPanelCanvas, eegGraphs, true);
        EEGButtons.gameObject.SetActive(true);
        EEGButtons.transform.SetAsLastSibling();


        if (CommandHandler.instance.isEEGON)
        {
            CommandHandler.instance.SendStart();
        }
       

        TogglePanel(PPGCanvas, ppgGraphs, false);
        PPGButtons.gameObject.SetActive(false);

        StatusPanel.gameObject.SetActive(true);

        SBControlPanel.alpha = 0;

        TogglePanel(ABDTDataPanelCanvas, abdtGraphs, false);
        ABDTButtons.gameObject.SetActive(false);

        TogglePanel(MentalStatesDataPanelCanvas, mentalStatesGraphs, false);
        MotionSensorCalibratorPanel.alpha = 0;
    }

    public void ChangeToABDTDataPanel()
    {
        if (CommandHandler.instance.isImpedanceCheckOn)
        {
            impedanceCheckPopup.SetActive(true);
            return;
        }
        ChangeButtonColor(sidePanelButtons[2]);
        SDKStateManager.instance.currentState = SDKStateManager.SDK_State.ABDT_PANEL;
        TogglePanel(ABDTDataPanelCanvas, abdtGraphs, true);
        ABDTButtons.gameObject.SetActive(true);
        ABDTButtons.transform.SetAsLastSibling();

        SBControlPanel.alpha = 0;

        TogglePanel(EEGDataPanelCanvas, eegGraphs, false);
        EEGButtons.gameObject.SetActive(false);

        //PPGDataPanel.gameObject.SetActive(false);
        TogglePanel(PPGCanvas, ppgGraphs, false);
        PPGButtons.gameObject.SetActive(false);

        TogglePanel(MentalStatesDataPanelCanvas, mentalStatesGraphs, false);
        MotionSensorCalibratorPanel.alpha = 0;
        StatusPanel.gameObject.SetActive(false);
    }

    public void ChangeToPPGDataPanel()
    {
        if (CommandHandler.instance.isImpedanceCheckOn)
        {
            impedanceCheckPopup.SetActive(true);
            return;
        }
        ChangeButtonColor(sidePanelButtons[3]);
        SDKStateManager.instance.currentState = SDKStateManager.SDK_State.PPG_PANEL;
        Debug.Log("PPGTrack: PPG panel active");
        if (CommandHandler.instance.isPPGON)
        {
            Debug.Log("PPGTrack: PPG should resume");
            CommandHandler.instance.StartReceivingPPG();
        }

        //PPGDataPanel.gameObject.SetActive(true);
        TogglePanel(PPGCanvas, ppgGraphs, true);
        PPGButtons.gameObject.SetActive(true);
        PPGButtons.transform.SetAsLastSibling();
        StatusPanel.gameObject.SetActive(true);

        SBControlPanel.alpha = 0;

        TogglePanel(EEGDataPanelCanvas, eegGraphs, false);
        EEGButtons.gameObject.SetActive(false);

        TogglePanel(ABDTDataPanelCanvas, abdtGraphs, false);
        ABDTButtons.gameObject.SetActive(false);

        TogglePanel(MentalStatesDataPanelCanvas, mentalStatesGraphs, false);
        MotionSensorCalibratorPanel.alpha = 0;
    }

    public void ChangeToMentalStateDataPanel()
    {
        if (CommandHandler.instance.isImpedanceCheckOn)
        {
            impedanceCheckPopup.SetActive(true);
            return;
        }
        ChangeButtonColor(sidePanelButtons[4]);
        SDKStateManager.instance.currentState = SDKStateManager.SDK_State.MENTAL_STATES_PANEL;
        TogglePanel(MentalStatesDataPanelCanvas, mentalStatesGraphs, true);

        SBControlPanel.alpha = 0;

        TogglePanel(EEGDataPanelCanvas, eegGraphs, false);
        EEGButtons.gameObject.SetActive(false);

        TogglePanel(ABDTDataPanelCanvas, abdtGraphs, false);
        ABDTButtons.gameObject.SetActive(false);

        //PPGDataPanel.gameObject.SetActive(false);
        TogglePanel(PPGCanvas, ppgGraphs, false);
        PPGButtons.gameObject.SetActive(false);

        MotionSensorCalibratorPanel.alpha = 0;
        StatusPanel.gameObject.SetActive(false);
    }

    public void ChangeToMotionSensorCalibratorPanel()
    {
        if (CommandHandler.instance.isImpedanceCheckOn)
        {
            impedanceCheckPopup.SetActive(true);
            return;
        }
        ChangeButtonColor(sidePanelButtons[5]);
        SDKStateManager.instance.currentState = SDKStateManager.SDK_State.MOTION_SENSOR_CALIBRATION_PANEL;
        MotionSensorCalibratorPanel.alpha = 1;
        MotionSensorCalibratorPanel.transform.SetAsLastSibling();

        SBControlPanel.alpha = 0;

        TogglePanel(EEGDataPanelCanvas, eegGraphs, false);
        EEGButtons.gameObject.SetActive(false);

        TogglePanel(ABDTDataPanelCanvas, abdtGraphs, false);
        ABDTButtons.gameObject.SetActive(false);

        //PPGDataPanel.gameObject.SetActive(false);
        TogglePanel(PPGCanvas, ppgGraphs, false);
        PPGButtons.gameObject.SetActive(false);

        TogglePanel(MentalStatesDataPanelCanvas, mentalStatesGraphs, false);
        StatusPanel.gameObject.SetActive(false);
    }


    public void TogglePanel(CanvasGroup panel, List<LineRenderer> graphs, bool isonPanel)
    {
        if (isonPanel)
        {
            panel.alpha = 1;
        }
        else
        {
            panel.alpha = 0;
        }

        for (int x = 0; x < graphs.Count; x++)
        {
            graphs[x].forceRenderingOff = !isonPanel;

            DisableSpriteRenderer(isonPanel, graphs[x].transform.root);

        }

    }


    public void DisableSpriteRenderer(bool condition, Transform curParent)
    {
        Component[] spriteRenderers;
        spriteRenderers = curParent.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sprites in spriteRenderers)
            sprites.enabled = condition;
    }


    public void ChangeButtonColor(GameObject buttonToChange)
    {
        buttonToChange.GetComponent<Image>().color = selectedColor;
        buttonToChange.GetComponent<Image>().sprite = selectedSprite;

        for (int x = 0; x < sidePanelButtons.Count; x++)
        {
            if (buttonToChange != sidePanelButtons[x])
            {
                sidePanelButtons[x].GetComponent<Image>().color = deSelectedColor;
                sidePanelButtons[x].GetComponent<Image>().sprite = deSelectedSprite;
            }
        }
    }
}