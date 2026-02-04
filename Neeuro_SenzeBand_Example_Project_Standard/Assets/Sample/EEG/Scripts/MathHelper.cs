using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MathHelper
{
	public static float GetPercentageOfValueInRange(float value, float min, float max)
	{
		//clamp the value first to make sure it's within range
		if (value > max)
			value = max;
		else if (value < min)
			value = min;
      
		return (value - min) / (max - min);
	}
    
	//formats int (time) into "hh:mm:ss" format
    public static string FormatTimeElapsed(int t)
	{
		int hours = t / 3600;
        int mins = (t % 3600) / 60;
        int secs = t % 60;
        
		return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, mins, secs);      
	}

	//formats float (time) into "hh:mm:ss.fff" format
    public static string FormatTimeElapsed(float t, int decimalPlaces = 3)
	{

        int millis = (int)(Math.Round(t % 1, decimalPlaces) * (Mathf.Pow(10, decimalPlaces)));
        //make sure millis doesn't go over 10, 100, 1000, etc because 10, 100, 1000 is same as 1 
		if (millis >= Mathf.Pow(10, decimalPlaces))
		{
			t++;
			millis = 0;
		}
		
		int hours = (int)t / 3600;
		int mins = ((int)t % 3600) / 60;
		int secs = (int)t % 60;
      
		return string.Format("{0:D2}:{1:D2}:{2:D2}.{3}", hours, mins, secs, millis);
	}
}
