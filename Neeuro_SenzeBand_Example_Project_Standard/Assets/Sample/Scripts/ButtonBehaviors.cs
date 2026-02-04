using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonBehaviors : MonoBehaviour
{
    public enum buttonType { 
        SMALL,
        MEDIUM,
        LONG
    }

    public buttonType buttonClassification;
    public Sprite clickedSmallButtonGraphic;
    public Sprite unclickedSmallButtonGraphic;

    public Sprite clickedMediumButtonGraphic;
    public Sprite unclickedMediumGraphic;

    public Sprite clickedLongButtonGraphic;
    public Sprite unclickedLongButtonGraphic;


    public void ChangetoClickGraphic(Button currButton)
    {
        Image currImage = currButton.GetComponent<Image>();
        currImage.sprite = clickedMediumButtonGraphic;
       
        switch (buttonClassification)
        {
            case buttonType.SMALL:
                currImage.sprite = clickedSmallButtonGraphic;
                break;

            case buttonType.MEDIUM:
                currImage.sprite = clickedMediumButtonGraphic;
                break;

            case buttonType.LONG:
                currImage.sprite = clickedLongButtonGraphic;
                break;

        }
        
    }

    public void ChangetoUnclickGraphic(Button currButton) {
        Image currImage = currButton.GetComponent<Image>();
        switch (buttonClassification)
        {
            case buttonType.SMALL:
                currImage.sprite = unclickedSmallButtonGraphic;
                break;

            case buttonType.MEDIUM:
                currImage.sprite = unclickedMediumGraphic;
                break;

            case buttonType.LONG:
                currImage.sprite = unclickedLongButtonGraphic;
                break;

        }
    }

  

}
