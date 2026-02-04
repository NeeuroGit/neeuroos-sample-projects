using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;

public class ConnectionColorIndicatorManager : MonoBehaviour
{
    public Color noConnectionColor;
    public Color connectedColor;
    public Color disConnectedColor;

    [Header("othercolors")]
    public Color DefaultOutline;
    public Color MaroonOutline;
    public static ConnectionColorIndicatorManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }
}
