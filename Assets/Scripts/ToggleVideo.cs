using agora_gaming_rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleVideo : MonoBehaviour
{
    [SerializeField] ToggleButton toggleVideo;

    [SerializeField] GameObject Video;

    IRtcEngine rtcEngine;

    // Start is called before the first frame update
    void Start()
    {
        rtcEngine = IRtcEngine.QueryEngine();
        SetupToggleVideo();
        toggleVideo = GetComponent<ToggleButton>();
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
            rtcEngine.EnableVideo();
            rtcEngine.EnableVideoObserver();
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
            rtcEngine.DisableVideo();
            rtcEngine.DisableVideoObserver();
            Video.SetActive(false);
        });
    }
}
