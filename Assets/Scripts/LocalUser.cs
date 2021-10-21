using agora_gaming_rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalUser : MonoBehaviour
{
    [SerializeField]
    GameObject Video;        

    public void ConfigureLocalUser()
    {
        VideoSurface videoSurface = Video.AddComponent<VideoSurface>();
        if (videoSurface != null)
        {
            videoSurface.enabled = true;
            // configure videoSurface
            videoSurface.SetForUser(0);
            videoSurface.SetEnable(true);
            videoSurface.SetGameFps(30);

            //videoSurface.SetForUser(0);
            //videoSurface.SetEnable(true);
            //videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
            //videoSurface.SetGameFps(30);
            //videoSurface.EnableFilpTextureApply(enableFlipHorizontal: true, enableFlipVertical: false);
        }
    }

    public void DisposeLocalUser()
    {        
        VideoSurface videoSurface = Video.GetComponent<VideoSurface>();
        if (videoSurface != null)
        {
            videoSurface.SetEnable(false);
        }
    }    
}
