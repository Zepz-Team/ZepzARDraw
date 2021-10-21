using agora_gaming_rtc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ShareScreen : MonoBehaviour
{
    IRtcEngine mRtcEngine;
    [SerializeField] ToggleButton toggleButton;
    [SerializeField] GameObject localUser;
    [SerializeField] RectTransform grid;
    [SerializeField] RectTransform Canvas;
    [SerializeField] GameObject Instruments;
    [SerializeField] ToggleButton toggleMainWin;
    [SerializeField] Button CameraButton;
    [SerializeField] Button HangupButton;
    [SerializeField] GameObject Background;
    [SerializeField] GameObject AR;
    [SerializeField] ToggleButton toggleVideo;
    [SerializeField] GameObject Video;
    [SerializeField] GameObject NormalCamera;

    public ICallback callback;

    private void Awake()
    {
        //toggleButton = transform.gameObject.GetComponent<ToggleButton>();
        toggleButton.button2.onClick.AddListener(OnShareScreenClick);
        mRtcEngine = IRtcEngine.QueryEngine();
        SetupMainWinButton();
        SetupToggleVideo();
        CameraButton.onClick.AddListener(StopScreenShare);        
    }

    //private void Start()
    //{
    //    StopScreenShare();        
    //}

    public void OnShareScreenClick()
    {
        //Return if Sharing is ON
        //if (toggleButton.button1.isActiveAndEnabled)
        //{
        //    return;
        //}        

        // Chage sharing icon from black to blue to show "is selected"
        toggleButton.Toggle(true);

        // Hide background panel
        Background.SetActive(false);

        //Change local user screen to full-size and turn off grid layout       
        RectTransform rect = localUser.GetComponent<RectTransform>();
        SetAndStretchToParentSize(rect, Canvas);
        grid.gameObject.SetActive(false);

        //Enable instruments game object
        Instruments.SetActive(true);

        //Change Camera config to Back Camera        
        //mRtcEngine.SwitchCamera();

        //Turn off main wondow button icon
        toggleMainWin.Toggle(true);

        //Show switch camera button
        CameraButton.gameObject.SetActive(true);

        //Turn On AR Session     
        //if (!AR.activeInHierarchy)        
        NormalCamera.SetActive(false);
        //mRtcEngine.SwitchCamera();
        AR.SetActive(true);

        //Call callback method in broadcastVC
        callback.OnGOStateChanged(true);
    }
    
    public void StopScreenShare()
    {
        // Chage sharing icon from blue to black to show "is not selected"
        toggleButton.Toggle(false);

        // Hide background panel
        Background.SetActive(true);

        //Change local user screen to grid cell-size and turn on grid layout
        grid.gameObject.SetActive(true);
        RectTransform rect = localUser.GetComponent<RectTransform>();
        SetAndStretchToParentSize(rect, grid);        

        //Enable instruments game object
        Instruments.SetActive(false);

        //Change Camera config to Back Camera 
        //mRtcEngine.SwitchCamera();

        //Turn off main wondow button icon
        toggleMainWin.Toggle(false);

        //Hide switch camera button
        CameraButton.gameObject.SetActive(false);

        //Turn Off AR Session
        //if (AR.activeInHierarchy)        
        NormalCamera.SetActive(true);
        //mRtcEngine.SwitchCamera();
        AR.SetActive(false);

        //Call callback method in broadcastVC
        callback.OnGOStateChanged(false);
    }

    private void SetAndStretchToParentSize(RectTransform _mRect, RectTransform _parent)
    {
        _mRect.anchoredPosition = _parent.position;
        _mRect.anchorMin = new Vector2(1, 0);
        _mRect.anchorMax = new Vector2(0, 1);
        _mRect.pivot = new Vector2(0.5f, 0.5f);
        _mRect.sizeDelta = _parent.rect.size;
        _mRect.transform.SetParent(_parent);
    }

    private void SetupMainWinButton()
    {
        if (toggleMainWin != null)
        {
            toggleMainWin.button1.onClick.AddListener(() =>
            {
                toggleMainWin.Tap();

                // if screen-sharing then turn it off            
                if (toggleButton.button1.isActiveAndEnabled)
                {
                    StopScreenShare();
                }
            });
            // Do nothing which clicking button2 
            //toggleMainWin.button2.onClick.AddListener(() =>
            //{
            //    toggleMainWin.Tap();                
            //});
        }
    }

    private void SetupToggleVideo()
    {
        toggleVideo.button1.onClick.AddListener(() =>
        {
            toggleVideo.Tap();
            VideoSurface videoSurface = Video.GetComponent<VideoSurface>();
            if (videoSurface != null)
            {
                videoSurface.enabled = true;
            }
            mRtcEngine.EnableVideo();
            mRtcEngine.EnableVideoObserver();
            Video.SetActive(true);
        });
        toggleVideo.button2.onClick.AddListener(() =>
        {
            toggleVideo.Tap();
            VideoSurface videoSurface = Video.GetComponent<VideoSurface>();
            if (videoSurface != null)
            {
                videoSurface.enabled = false;
            }
            mRtcEngine.DisableVideo();
            mRtcEngine.DisableVideoObserver();

            // if screen-sharing then turn it off            
            if (toggleButton.button1.isActiveAndEnabled)
            {
                StopScreenShare();
            }
            Video.SetActive(false);
        });
    }    
}
