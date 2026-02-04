using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SenzebandControlPanelUI : MonoBehaviour
{
    private NSB_Manager nsbm;

    [Space]
    public Text AccXValue;
    public Text AccYValue;
    public Text AccZValue;
    public Text DirectionValue;
    public Text RXValue;
    public Text RYValue;
    public Text RZValue;


    [Space]
    public Image Ch1ConnectionIndicator;
    public Image Ch2ConnectionIndicator;
    public Image Ch3ConnectionIndicator;
    public Image Ch4ConnectionIndicator;


    [Space]
    public Text Ch1Value;
    public Text Ch2Value;
    public Text Ch3Value;
    public Text Ch4Value;
    public Text SignalReadyValue;

    private ConnectionColorIndicatorManager colorConnectionManager;

    // Use this for initialization
    void Start()
    {
        nsbm = NSB_Manager.instance;
        colorConnectionManager = ConnectionColorIndicatorManager.instance;

    }

    void Update()
    {

        if (nsbm.GetReceiveEEGState())
        {

            DirectionValue.text = nsbm.GetDirection();

            RXValue.text = nsbm.GetAccel(3).ToString();
            RYValue.text = nsbm.GetAccel(4).ToString();
            RZValue.text = nsbm.GetAccel(5).ToString();

            AccXValue.text = nsbm.GetAccel(0).ToString();
            AccYValue.text = nsbm.GetAccel(1).ToString();
            AccZValue.text = nsbm.GetAccel(2).ToString();

            Ch1Value.text = nsbm.GetChannelStatus(0).ToString();
            Ch2Value.text = nsbm.GetChannelStatus(1).ToString();
            Ch3Value.text = nsbm.GetChannelStatus(2).ToString();
            Ch4Value.text = nsbm.GetChannelStatus(3).ToString();

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

            SignalReadyValue.text = nsbm.GetSignalReady().ToString() ;
        


        }
        else
        {

            AccXValue.text = "-";
            AccYValue.text = "-";
            AccZValue.text = "-";

            Ch1Value.text = "-";
            Ch2Value.text = "-";
            Ch3Value.text = "-";
            Ch4Value.text = "-";         
                      

            RXValue.text = "-";
            RYValue.text = "-";
            RZValue.text = "-";
          

            Ch1ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch2ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch3ConnectionIndicator.color = colorConnectionManager.noConnectionColor;
            Ch4ConnectionIndicator.color = colorConnectionManager.noConnectionColor;


        }


    }


}
