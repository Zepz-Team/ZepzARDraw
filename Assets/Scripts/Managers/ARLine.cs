using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARLine 
{
    private int positionCount = 0;

    private Vector3 prevPointDistance = Vector3.zero;
    
    private LineRenderer LineRenderer { get; set; }

    private LineSettings settings;

    public ARLine(LineSettings settings)
    {
        this.settings = settings;
    }

    public void AddPoint(Vector3 position)
    {
        if(prevPointDistance == null)
            prevPointDistance = position;

        if(prevPointDistance != null && Mathf.Abs(Vector3.Distance(prevPointDistance, position)) >= settings.minDistanceBeforeNewPoint)
        {
            prevPointDistance = position;
            positionCount++;

            LineRenderer.positionCount = positionCount;

            // index 0 positionCount must be - 1
            LineRenderer.SetPosition(positionCount - 1, position);

            // applies simplification if reminder is 0
            if(LineRenderer.positionCount % settings.applySimplifyAfterPoints == 0 && settings.allowSimplification)
            {
                LineRenderer.Simplify(settings.tolerance);
            }
        }   
    }

    public void AddNewLineRenderer(Transform parent1, ARAnchor anchor, Vector3 position)
    {
        Transform parent2 = anchor?.transform;
        AddNewLineRenderer(parent1, parent2, position);
    }

    public void AddNewLineRenderer(Transform parent1, Transform parent2, Vector3 position)
    {
        positionCount = 2;
        GameObject go = new GameObject($"LineRenderer");
        
        go.transform.parent = parent2 ?? parent1;
        go.transform.position = position;
        go.tag = settings.lineTagName;
        
        LineRenderer goLineRenderer = go.AddComponent<LineRenderer>();

        //We are going with the convention that line width is 100th of distance from Camera
        goLineRenderer.startWidth = settings.startWidth; // settings.distanceFromCamera / 100;
        goLineRenderer.endWidth = settings.endWidth; // settings.distanceFromCamera / 100;

        goLineRenderer.startColor = settings.startColor;
        goLineRenderer.endColor = settings.endColor;

        goLineRenderer.material = settings.defaultMaterial;
        goLineRenderer.useWorldSpace = true;
        goLineRenderer.positionCount = positionCount;

        goLineRenderer.numCornerVertices = settings.cornerVertices;
        goLineRenderer.numCapVertices = settings.endCapVertices;

        goLineRenderer.SetPosition(0, position);
        goLineRenderer.SetPosition(1, position);

        LineRenderer = goLineRenderer;

        //ARDebugManager.Instance.LogInfo($"New line renderer created");
    }
}