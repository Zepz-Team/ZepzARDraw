using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleDrawButton : ToggleButton
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
        SetupDrawBtn();
    }    

    private void SetupDrawBtn()
    {
        button1.onClick.AddListener(() =>
        {
            Tap();
            drawManager.ClearLines();
            drawManager.CanDraw = false;
        });
        button2.onClick.AddListener(() =>
        {
            Tap();
            drawManager.CanDraw = true;
        });
    }
}
