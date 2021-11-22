using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleFlashlightBtn : ToggleButton
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
    GameObject Flashlight = null;    

    // Start is called before the first frame update
    void Start()
    {
        SetupFlashlightBtn();
    }    

    private void SetupFlashlightBtn()
    {
        button1.onClick.AddListener(() =>
        {
            Tap();
            Flashlight.SetActive(false);
        });
        button2.onClick.AddListener(() =>
        {
            Tap();
            Flashlight.SetActive(true);            
        });
    }
}
