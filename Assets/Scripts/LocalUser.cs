using agora_gaming_rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalUser : MonoBehaviour
{
    [SerializeField]
    GameObject Video;

    [SerializeField]
    Animator loadingAnimator;    

    private void Start()
    {
        StartCoroutine(DelayAction(1.2f,
                    () =>
                    {
                        loadingAnimator.enabled = false;
                        loadingAnimator.gameObject.SetActive(false);
                    }));
    }

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

    IEnumerator DelayAction(float delay, System.Action doAction)
    {
        yield return new WaitForSeconds(delay);
        doAction();
    }
}
