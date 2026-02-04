using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIObject : MonoBehaviour 
{
    public enum Transition
    {
        FADE,
        APPEARFROMLEFT,
        APPEARFROMRIGHT,
        APPEARFROMTOP,
        APPEARFROMBOTTOM,
        SCALEFROMBIG,
        SCALEFROMSMALL,
        CUSTOM
    }

    public float onEnableTransitionDuration = 0f;
    public Transition[] onEnableTransition;
    public float onDisableTransitionDuration = 0f;
    public Transition[] onDisableTransition;
    public AnimationCurve transitionAnimCurve;

    public UnityEvent functionToCallOnEnableTransitionFinish;
    public UnityEvent functionToCallOnDisableTransitionFinish;

    private Coroutine transitionCoroutine;
    private CanvasGroup canvasGroup;
    private RectTransform rt;
	private Animator animator;
    private Vector3 origPos;
    private Vector2 origSizeDelta;
    private float origScale;
    private float origAlpha;
    private bool hasStartedPlayingCustomAnim;
    //[HideInInspector]
    public bool activatedThroughOwnFunction;   //false if object is activated through gameObject.SetActive instead of uiObject.SetActive
    private bool isTransitioning;
	private bool delayEnableStarted = false;

    private const string CUSTOM_ENABLETRANSITION_ANIMKEY = "EnableTransition";
    private const string CUSTOM_DISABLETRANSITION_ANIMKEY = "DisableTransition";

    public Coroutine TransitionCoroutine{ get { return transitionCoroutine; }}
    
    void Awake()
    {
        rt = GetComponent<RectTransform>();
        
		if (GetComponent<Animator>())
		{
			animator = GetComponent<Animator>();
			if(onEnableTransition.Length > 0 && onEnableTransition[0] == Transition.CUSTOM)
			    animator.enabled = false;
		}

        origPos = rt.anchoredPosition3D;
        origSizeDelta = rt.sizeDelta;
        origScale = rt.localScale.x;

        if (canvasGroup)
            origAlpha = canvasGroup.alpha;

		if (onEnableTransition.Length == 0)
            onEnableTransitionDuration = 0f;
        if (onDisableTransition.Length == 0)
            onDisableTransitionDuration = 0f;

        functionToCallOnDisableTransitionFinish.AddListener(() => { DisableObject(); });
        activatedThroughOwnFunction = false;
    }

    void Start()
    {
    }

    void OnEnable()
    {
        if(!activatedThroughOwnFunction)
        {
            hasStartedPlayingCustomAnim = false;
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(AnimateTransition(true));

        }
        activatedThroughOwnFunction = false;    //reset to false for next time
    }

    public void SetActive(bool active)
	{
		if (active && gameObject.activeInHierarchy ||
            !active && isTransitioning)
            return;
		
        bool continueTransition = false;
        if(active)
        {
            continueTransition = !gameObject.activeInHierarchy && !isTransitioning;
            activatedThroughOwnFunction = true;
            gameObject.SetActive(true);

        }
        else
        {
            continueTransition = gameObject.activeInHierarchy && !isTransitioning;
        }

        if (continueTransition)
        {
            isTransitioning = true;
            hasStartedPlayingCustomAnim = false;
            if (transitionCoroutine != null)
                StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(AnimateTransition(active));
        }
    }
    public void DelayEnable(float duration)
	{
		if (!delayEnableStarted)
		{
			delayEnableStarted = true;
			Invoke("SetActiveToTrue", duration);
		}
	}
    private void SetActiveToTrue()
	{
		delayEnableStarted = false;
		SetActive(true);
	}

    
    private IEnumerator AnimateTransition(bool toAppear)
    {
        float t = 0;

		if (animator)
			animator.enabled = true;

        if(toAppear)
        {
            while(t<1)
            {
                if (1 - t <= 0.05f)
                    t = 1;
                //on enable animation
                for (int i = 0; i < onEnableTransition.Length; i++)
                {
                    switch(onEnableTransition[i])
                    {
                        case Transition.FADE:
                            if (!canvasGroup)
                            {
                                canvasGroup = GetComponent<CanvasGroup>();
                                if (!canvasGroup)
                                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                                origAlpha = canvasGroup.alpha;
                            }

                            canvasGroup.alpha = transitionAnimCurve.Evaluate(t);
                            if (1 - canvasGroup.alpha <= 0.05f)
                                canvasGroup.alpha = 1;

                            break;
                        case Transition.APPEARFROMLEFT:
                            rt.anchoredPosition3D = new Vector3(Mathf.Lerp(origPos.x - origSizeDelta.x, origPos.x, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.y, rt.anchoredPosition3D.z);
                            break;
                        case Transition.APPEARFROMRIGHT:
                            Debug.Log(rt);
                            rt.anchoredPosition3D = new Vector3(Mathf.Lerp(origPos.x + origSizeDelta.x, origPos.x, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.y, rt.anchoredPosition3D.z);
                            break;
                        case Transition.APPEARFROMTOP:
                            rt.anchoredPosition3D = new Vector3(rt.anchoredPosition3D.x,
                                                                Mathf.Lerp(origPos.y + origSizeDelta.y, origPos.y, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.z);
                            break;
                        case Transition.APPEARFROMBOTTOM:
                            rt.anchoredPosition3D = new Vector3(rt.anchoredPosition3D.x,
                                                                Mathf.Lerp(origPos.y - origSizeDelta.y, origPos.y, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.z);
                            break;
                        case Transition.SCALEFROMBIG:
                            rt.localScale = Vector3.one * Mathf.Lerp(origScale * 2, origScale, transitionAnimCurve.Evaluate(t));
                            break;
                        case Transition.SCALEFROMSMALL:
                            rt.localScale = Vector3.one * Mathf.Lerp(0, origScale, transitionAnimCurve.Evaluate(t));
                            break;
                        case Transition.CUSTOM:
                            if(!hasStartedPlayingCustomAnim)
                            {
                                animator.Play(CUSTOM_ENABLETRANSITION_ANIMKEY);
                                hasStartedPlayingCustomAnim = true;
                            }
                            break;
                    }
                }
                //-------------------

                t += Time.deltaTime / onEnableTransitionDuration;
                yield return null;
            }
            for (int i = 0; i < onEnableTransition.Length; i++)
            {
                switch (onEnableTransition[i])
                {
                    case Transition.FADE:
                        canvasGroup.alpha = 1;
                        break;
                    case Transition.APPEARFROMLEFT:
                    case Transition.APPEARFROMRIGHT:
                        rt.anchoredPosition3D = new Vector3(origPos.x,
                                                            rt.anchoredPosition3D.y, rt.anchoredPosition3D.z);
                        break;
                    case Transition.APPEARFROMTOP:
                    case Transition.APPEARFROMBOTTOM:
                        rt.anchoredPosition3D = new Vector3(rt.anchoredPosition3D.x,
                                                            origPos.y,
                                                            rt.anchoredPosition3D.z);
                        break;
                    case Transition.SCALEFROMBIG:
                    case Transition.SCALEFROMSMALL:
                        rt.localScale = Vector3.one * origScale;
                        break;
                }
            }

            functionToCallOnEnableTransitionFinish.Invoke();
        }
        else
        {
            t = 1;

            while(t>0)
            {
                if (t <= 0.05f)
                    t = 0;
                //on disable animation
                for (int i = 0; i < onDisableTransition.Length; i++)
                {
                    switch (onDisableTransition[i])
                    {
                        case Transition.FADE:
                            if (!canvasGroup)
                            {
                                canvasGroup = GetComponent<CanvasGroup>();
                                if (!canvasGroup)
                                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                                origAlpha = canvasGroup.alpha;
                            }

                            canvasGroup.alpha = transitionAnimCurve.Evaluate(t);
                            if (canvasGroup.alpha <= 0.05f)
                                canvasGroup.alpha = 0;
                            break;
                        case Transition.APPEARFROMLEFT:
                            rt.anchoredPosition3D = new Vector3(Mathf.Lerp(origPos.x - origSizeDelta.x, origPos.x, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.y, rt.anchoredPosition3D.z);
                            break;
                        case Transition.APPEARFROMRIGHT:
                            rt.anchoredPosition3D = new Vector3(Mathf.Lerp(origPos.x + origSizeDelta.x, origPos.x, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.y, rt.anchoredPosition3D.z);
                            break;
                        case Transition.APPEARFROMTOP:
                            rt.anchoredPosition3D = new Vector3(rt.anchoredPosition3D.x,
                                                                Mathf.Lerp(origPos.y + origSizeDelta.y, origPos.y, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.z);
                            break;
                        case Transition.APPEARFROMBOTTOM:
                            rt.anchoredPosition3D = new Vector3(rt.anchoredPosition3D.x,
                                                                Mathf.Lerp(origPos.y - origSizeDelta.y, origPos.y, transitionAnimCurve.Evaluate(t)),
                                                                rt.anchoredPosition3D.z);
                            break;
                        case Transition.SCALEFROMBIG:
                            rt.localScale = Vector3.one * Mathf.Lerp(origScale * 2, origScale, transitionAnimCurve.Evaluate(t));
                            break;
                        case Transition.SCALEFROMSMALL:
                            rt.localScale = Vector3.one * Mathf.Lerp(0, origScale, transitionAnimCurve.Evaluate(t));
                            break;
                        case Transition.CUSTOM:
                            if (!hasStartedPlayingCustomAnim)
                            {
                                animator.Play(CUSTOM_DISABLETRANSITION_ANIMKEY);
                                hasStartedPlayingCustomAnim = true;
                            }
                            break;
                    }
                }
                //-------------------

                t -= Time.deltaTime / onDisableTransitionDuration;
                yield return null;
            }

            for (int i = 0; i < onDisableTransition.Length; i++)
            {
                switch (onDisableTransition[i])
                {
                    case Transition.FADE:
                        canvasGroup.alpha = 0;
                        break;
                    case Transition.APPEARFROMLEFT:
                        rt.anchoredPosition3D = new Vector3(origPos.x - origSizeDelta.x,
                                                            rt.anchoredPosition3D.y, rt.anchoredPosition3D.z);
                        break;
                    case Transition.APPEARFROMRIGHT:
                        rt.anchoredPosition3D = new Vector3(origPos.x + origSizeDelta.x,
                                                            rt.anchoredPosition3D.y, rt.anchoredPosition3D.z);
                        break;
                    case Transition.APPEARFROMTOP:
                        rt.anchoredPosition3D = new Vector3(rt.anchoredPosition3D.x,
                                                            origPos.y + origSizeDelta.y,
                                                            rt.anchoredPosition3D.z);
                        break;
                    case Transition.APPEARFROMBOTTOM:
                        rt.anchoredPosition3D = new Vector3(rt.anchoredPosition3D.x,
                                                            origPos.y - origSizeDelta.y,
                                                            rt.anchoredPosition3D.z);
                        break;
                    case Transition.SCALEFROMBIG:
                        rt.localScale = Vector3.one * origScale * 2;
                        break;
                    case Transition.SCALEFROMSMALL:
                        rt.localScale = Vector3.one * 0;
                        break;
                }
            }

            functionToCallOnDisableTransitionFinish.Invoke();
        }
        isTransitioning = false;
        
		//rt.anchoredPosition3D = origPos;
		//rt.localScale = Vector3.one * origScale;
		//if(canvasGroup)
		//canvasGroup.alpha = origAlpha;
		if(animator && (onEnableTransition.Length > 0 && onEnableTransition[0] == Transition.CUSTOM))
		    animator.enabled = false;
    }

    void DisableObject()
    {
        gameObject.SetActive(false);
    }
}
