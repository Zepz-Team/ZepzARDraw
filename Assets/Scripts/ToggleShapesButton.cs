using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EShapes
{
    ARROW,
    SQUARE,
    TRIANGLE,
    CIRCLE
}

public class ToggleShapesButton : ToggleDropArrow
{
    [SerializeField]
    GameObject ShapesPanel;

    void Start()
    {
        SetupShapesBtn();
    }

    private void SetupShapesBtn()
    {
        button1.onClick.AddListener(() =>
        {
            Tap();
            drawManager.ClearShapes();            
            drawManager.CanDropArrow = false;
            ToggleShapesPanel(false);
        });
        button2.onClick.AddListener(() =>
        {
            Tap();
            ToggleShapesPanel(true);            
        });
    }

    public void ToggleShapesPanel(bool show)
    {        
        ShapesPanel.SetActive(show);
    }

    public void OnSquareButtonClick()
    {
        drawManager.SetShapeIndex(EShapes.SQUARE);
        drawManager.CanDropArrow = true;
        //ToggleShapesPanel(false);        
    }

    public void OnCircleButtonClick()
    {
        drawManager.SetShapeIndex(EShapes.CIRCLE);
        drawManager.CanDropArrow = true;
        //ToggleShapesPanel(false);        
    }

    public void OnTriangleButtonClick()
    {
        drawManager.SetShapeIndex(EShapes.TRIANGLE);
        drawManager.CanDropArrow = true;
        //ToggleShapesPanel(false);        
    }
}
