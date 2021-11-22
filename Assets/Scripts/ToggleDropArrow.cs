using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleDropArrow : ToggleButton
{
    public Button Button1
    {
     get { return button1; } 
     set { button1 = value;}
    }
    public Button Button2
    {
        get { return button2; }
        set { button2 = value; }
    }

    [SerializeField]
    ARDrawManager drawManager = null;
    
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
            drawManager.ClearArrows();
            drawManager.CanDropArrow = false;
        });
        button2.onClick.AddListener(() =>
        {
            Tap();
            drawManager.CanDropArrow = true;
        });
    }
}
