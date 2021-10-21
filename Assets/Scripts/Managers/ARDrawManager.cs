using System;
using System.Collections.Generic;
using System.Linq;
using DilmerGames.Core.Singletons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

//[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    [SerializeField]
    private LineSettings lineSettings = null;

    [SerializeField]
    private UnityEvent OnDraw = null;

    [SerializeField]
    private ARAnchorManager anchorManager = null;

    Camera arCamera;     // the AR Camera
    Camera renderCam; // the Renderer Camera, space of 3D objects
    Camera viewCam; // the viewer of the projected quad, acting camera sin

    [SerializeField]
    private ToggleButton toggleDraw = null;

    private List<ARAnchor> anchors = new List<ARAnchor>();

    //private Dictionary<int, ARLine> Lines = new Dictionary<int, ARLine>();
    private List<ARLine> Lines = new List<ARLine>();

    [SerializeField] Transform referenceObject = null;

    [SerializeField] GraphicRaycaster graphicRaycaster = null;

    //private GameObject anchorGO;

    private bool CanDraw { get; set; }
    
    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        //AllowDraw(false);

        SetupDrawBtn();

        CamStart();        
    }

    void Update ()
    {
        #if !UNITY_EDITOR    
        DrawOnTouch();
        #else
        DrawOnMouse();
        #endif
	}    

    /// <summary>
    /// THis is called when local user on mobile is drawing
    /// </summary>     
    void DrawOnTouch()
    {
        if(!CanDraw) return;

        //int tapCount = Input.touchCount > 1 && lineSettings.allowMultiTouch ? Input.touchCount : 1;

        //for(int i = 0; i < tapCount; i++)
        if (Input.touchCount > 0)
        {
            if (RayHitUI())
            {
                return;
            }

            int i = 0;
            Touch touch = Input.GetTouch(i);
            Vector3 touchPosition = arCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, lineSettings.distanceFromCamera));
            
            //ARDebugManager.Instance.LogInfo($"{touch.fingerId}");

            if(touch.phase == TouchPhase.Began)
            {
                OnDraw?.Invoke();
                
                ARAnchor anchor = anchorManager.AddAnchor(new Pose(touchPosition, Quaternion.identity));
                if (anchor == null) 
                    Debug.LogError("Error creating reference point");
                else 
                {
                    anchors.Add(anchor);
                    //ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
                    Debug.Log($"Anchor created & total of {anchors.Count} anchor(s)");
                }

                lineSettings.startColor = Color.blue;
                ARLine line = new ARLine(lineSettings);
                Lines.Add(line);
                line.AddNewLineRenderer(transform, anchor, touchPosition);
            }
            else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                Lines[Lines.Count() - 1].AddPoint(touchPosition);                
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                Lines.Clear(); // Remove(touch.fingerId);
            }
        }
    }    

    /// <summary>
    /// Called when local user is drawing on the editor screen
    /// </summary>
    void DrawOnMouse()
    {
        if(!CanDraw) return;

        Vector3 mousePosition = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lineSettings.distanceFromCamera));

        if(Input.GetMouseButton(0))
        {
            OnDraw?.Invoke();

            if(Lines.Count == 0)
            {
                ARLine line = new ARLine(lineSettings);
                Lines.Add(line);
                line.AddNewLineRenderer(transform, (ARAnchor)null, mousePosition);
            }
            else 
            {
                Lines[0].AddPoint(mousePosition);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            Lines.Clear(); // Remove(0);   
        }
    }

    /// <summary>
    /// THis is called when remote user on mobile is drawing
    /// </summary>
    /// <param name="pos">Position where the remote user has touched the screen</param>   
    public void DrawOnTouch(Vector2 pos)
    {
        Vector3 location = DeNormalizedPosition(pos, renderCam);
        if (Lines.Count == 0)
        {
            ARAnchor anchor = anchorManager.AddAnchor(new Pose(location, Quaternion.identity));
            if (anchor == null)
                Debug.LogError("Error creating reference point");
            else
            {
                anchor.transform.SetParent(referenceObject.transform.parent);
                anchors.Add(anchor);
                Debug.Log($"Anchor created & total of {anchors.Count} anchor(s)");
                //ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
            }

            ARLine line = new ARLine(lineSettings);
            Lines.Add(line);
            line.AddNewLineRenderer(transform, anchor, location);
        }
        else
        {
            Lines[0].AddPoint(location);
        }
    }
             
    /// <summary>
    /// THis is called when remote user on web browser is drawing
    /// </summary>
    /// <param name="points">list of points where remote user has clicked for drawing</param>
    internal void DrawOnMouse(List<myPoint> points)
    {
        foreach (myPoint point in points)
        {
            // Denormalize points location
            Vector3 pos = new Vector3(point.x, point.y);
            //pos = arCamera.ViewportToScreenPoint(pos);

            // Generate world points to draw lines at a defined distance
            //Vector3 location = arCamera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, lineSettings.distanceFromCamera));

            Vector3 location = DeNormalizedPosition(pos, renderCam);

            if (Lines.Count == 0)
            {
                OnDraw?.Invoke();
               
                ARAnchor anchor = anchorManager.AddAnchor(new Pose(location, Quaternion.identity));
                if (anchor == null)
                    Debug.LogError("Error creating reference point");
                else
                {
                    anchor.transform.SetParent(referenceObject.transform.parent);
                    anchors.Add(anchor);
                    //ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
                    Debug.Log($"Anchor created & total of {anchors.Count} anchor(s)");
                }

                lineSettings.startColor = Color.red;
                ARLine line = new ARLine(lineSettings);
                Lines.Add(line);
                line.AddNewLineRenderer(transform, anchor, location);
            }
            else 
            {
                //Add point in the last added line
                Lines[Lines.Count() -1].AddPoint(location);
            }            
        }
        Lines.Clear(); // Remove(0);
    }

    public void DrawDot(List<myPoint> points)
    {
        //foreach (myPoint point in points)
        //{
        //    Vector3 location = new Vector3(point.x, point.y, lineSettings.distanceFromCamera);// DeNormalizedPosition(pos, renderCam);
        //    if (Lines.Keys.Count == 0)
        //    {
        //        if (anchorGO == null)
        //        {
        //            anchorGO = new GameObject();
        //            anchorGO.transform.SetParent(referenceObject.transform.parent);
        //            anchorGO.transform.position = Vector3.zero;
        //            anchorGO.transform.localScale = Vector3.one;
        //            anchorGO.name = "DrawAnchor";
        //        }

        //        GameObject go = GameObject.Instantiate(DrawPrefab, location, Quaternion.identity);
        //        go.transform.SetParent(anchorGO.transform);
        //        go.transform.localScale = DotScale * Vector3.one;
        //        go.layer = (int)CameraLayer.IGNORE_RAYCAST;
        //        go.name = "dot " + dotCount;
        //        dotCount++;
        //        Debug.LogFormat("{0} pos:{1} => : {2} ", go.name, pos, location);
        //        Renderer renderer = go.GetComponent<Renderer>();
        //        if (renderer != null)
        //        {
        //            Material mat = renderer.material;
        //            if (mat != null)
        //            {
        //                mat.color = DrawColor;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        Lines[0].AddPoint(location);
        //    }
        //}
        //Lines.Remove(0);
    }

    /// <summary>
    ///    Provide a ViewPort Position (0,0) = bottom left and (1,1) top right
    /// return world position for the current camera
    /// </summary>
    /// <param name="vector2"></param>
    /// <param name="camera"></param>
    /// <returns></returns>
    Vector3 DeNormalizedPosition(Vector2 vector2, Camera camera)
    {
        Vector3 pos = new Vector3(vector2.x, vector2.y);

        pos = camera.ViewportToScreenPoint(pos);

        // Consider using the referenceObject for position calculation
        // Vector3 deltaPos = camera.transform.position - referenceObject.position;

        pos = new Vector3(pos.x, pos.y, GetDistanceFromCamera());

        return camera.ScreenToWorldPoint(pos);
    }


    GameObject[] GetAllLinesInScene()
    {
        return GameObject.FindGameObjectsWithTag("Line");
    }

    public void ClearLines()
    {
        GameObject[] lines = GetAllLinesInScene();
        foreach (GameObject currentLine in lines)
        {
            LineRenderer line = currentLine.GetComponent<LineRenderer>();
            Destroy(currentLine);
        }
        //Destroy(anchorGO);
    }

    public float GetDistanceFromCamera()
    {
        return lineSettings.distanceFromCamera;
    }

    /// <summary>
    ///   Checking if the touch is on a UI component, which should be ignored.
    /// </summary>
    /// <returns></returns>
    bool RayHitUI()
    {
        //Create the PointerEventData with null for the EventSystem
        PointerEventData ped = new PointerEventData(null);
        //Set required parameters, in this case, mouse position
        ped.position = Input.mousePosition;
        //Create list to receive all results
        List<RaycastResult> results = new List<RaycastResult>();
        //Raycast it
        graphicRaycaster.Raycast(ped, results);

        if (results.Count > 0)
        {
            return results.Any(r => r.gameObject.layer == 5 /*LAYER_UI*/);
        }
        return false;
    }

    private void SetupDrawBtn()
    {
        toggleDraw.button1.onClick.AddListener(() =>
        {
            toggleDraw.Tap();
            ClearLines();
            CanDraw = false;            
        });
        toggleDraw.button2.onClick.AddListener(() =>
        {
            toggleDraw.Tap();
            CanDraw = true;
        });
    }

    // Use this for initialization
    void CamStart()
    {
        //TODO: refactor -> find child GO
        arCamera = GameObject.Find("AR Camera").GetComponent<Camera>();
        renderCam = GameObject.Find("RenderCamera").GetComponent<Camera>();        
        viewCam = GameObject.Find("ViewCamera").GetComponent<Camera>();
    }
}