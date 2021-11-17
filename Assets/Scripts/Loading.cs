using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loading : MonoBehaviour
{    
    Animator animator;
    agora_gaming_rtc.IRtcEngine mRtcEngine;

    // Start is called before the first frame update
    private void Start()
    {
        GameObject loading = transform.Find("loading").gameObject;
        animator = loading.GetComponent<Animator>();
        mRtcEngine = agora_gaming_rtc.IRtcEngine.QueryEngine();
        mRtcEngine.OnFirstRemoteVideoDecoded += FirstSceneReceived;        
    }    

    public void FirstSceneReceived(uint uid, int width, int height, int elapsed)
    {
        animator.enabled = false;
        animator.gameObject.SetActive(false);
        mRtcEngine.OnFirstRemoteVideoDecoded -= FirstSceneReceived;        
    }
}
