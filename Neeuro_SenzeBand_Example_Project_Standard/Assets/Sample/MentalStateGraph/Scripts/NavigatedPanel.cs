using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NavigatedPanel : MonoBehaviour 
{
	public UnityEvent OnFocusUnfocus;

	public bool Focused { set; get; }

	public Toggle toggle;
    
	private bool bypassCheck;

	private void OnEnable()
	{
		bypassCheck = true;
		FocusPanel(toggle.isOn);
		bypassCheck = false;
	}


	public void FocusPanel(bool focus)
	{
		if(focus)
		{
			if (!bypassCheck)
			{
				if (Focused)
					return;
			}

			Debug.LogError(gameObject.name + " FOCUS");
			transform.SetAsLastSibling();
			GetComponent<RectTransform>().anchoredPosition3D = new Vector3(GetComponent<RectTransform>().anchoredPosition3D.x,
																		   GetComponent<RectTransform>().anchoredPosition3D.y,
																		   -50);

		}
		else
		{
			if (!bypassCheck)
			{
				if (!Focused)
					return;
			}

			Debug.LogError(gameObject.name + " UNFOCUS");
			GetComponent<RectTransform>().anchoredPosition3D = new Vector3(GetComponent<RectTransform>().anchoredPosition3D.x,
                                                                           GetComponent<RectTransform>().anchoredPosition3D.y,
                                                                           100);
		}

		Focused = focus;
		if (OnFocusUnfocus != null)
			OnFocusUnfocus.Invoke();
	}

}
