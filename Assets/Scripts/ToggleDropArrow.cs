using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleDropArrow : ToggleButton
{    
    public ARDrawManager drawManager = null;
    
    // Start is called before the first frame update
    void Start()
    {
        SetupDropArrowBtn();
    }    

    private void SetupDropArrowBtn()
    {
        button1.onClick.AddListener(() =>
        {
            Tap();
            drawManager.ClearShapes();
            drawManager.CanDropArrow = false;
        });
        button2.onClick.AddListener(() =>
        {
            Tap();
            drawManager.CanDropArrow = true;
            drawManager.SetShapeIndex(EShapes.ARROW);
        });
    }
}
