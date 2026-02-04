using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class ApplicationManager : MonoBehaviour
{
	public static ApplicationManager instance;

	public enum SENZEBANDTYPE
	{
		SENZEBAND_V1,
		SENZEBAND_V2
	}
	public SENZEBANDTYPE senzebandTypeUsed;
    
	public enum SPECIALVERSION
	{
		BASELINE = 0,
		EEG_NEEURO,
		EEG_NSDE
	}
	private SPECIALVERSION specialVersion;
	public bool enableDebuggerUI;
	public bool enableSessionTimeLimit;
	public float sessionTimeLimit = 3600;
	public bool showExhibitBanner;


    public void Awake()
    {
        if (Application.isPlaying)
        {
            if (instance != null)
                Destroy(this.gameObject);
        }


		if (instance == null)
		{
			instance = this;
            if (Application.isPlaying)
            {
        
                DontDestroyOnLoad(this.gameObject); //We need this object to persist throughout the lifetime of the program
            }
		}

		if (PlayerPrefs.HasKey("SPECIAL_VERSION_INDEX"))
			specialVersion = (SPECIALVERSION)PlayerPrefs.GetInt("SPECIAL_VERSION_INDEX");
	}

	public bool CanRecordRawEEG()
	{
		return specialVersion.ToString().Contains("EEG");
	}

	//calculate physical inches with pythagoras theorem
	public static float DeviceDiagonalSizeInInches()
	{
		float screenWidth = Screen.width / Screen.dpi;
		float screenHeight = Screen.height / Screen.dpi;
		float diagonalInches = Mathf.Sqrt(Mathf.Pow(screenWidth, 2) + Mathf.Pow(screenHeight, 2));

		Debug.Log("Getting device inches: " + diagonalInches);

		return diagonalInches;
	}

	public static bool IsMobilePhone()
	{
		bool isMobilePhone = false;

		var aspectRatio = Mathf.Max(Screen.width, Screen.height) / Mathf.Min(Screen.width, Screen.height);

		isMobilePhone = !(DeviceDiagonalSizeInInches() > 6.5f && aspectRatio < 2f);

		return isMobilePhone;
	}
}
