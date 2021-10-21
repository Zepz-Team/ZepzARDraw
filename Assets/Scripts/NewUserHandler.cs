using agora_gaming_rtc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewUserHandler : MonoBehaviour
{
    [SerializeField]
    GameObject UserPrefab;    

    [SerializeField]
    LocalUser localUser;

    //Dictionary<uint, GameObject> Users;

    //private void Awake()
    //{
    //    Users = new Dictionary<uint, GameObject>();
    //}    

    public GameObject CreateAndConfigureUserVideo(uint userID)
    {
        //if (!Users.ContainsKey(userID))
        //{
            GameObject user = Instantiate(UserPrefab, transform);
            GameObject userVideo = user.transform.Find("Video").gameObject;
            VideoSurface videoSurface = userVideo.AddComponent<VideoSurface>();
            if (videoSurface != null)
            {
                videoSurface.enabled = true;
                // configure videoSurface
                videoSurface.SetForUser(userID);
                videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.RawImage);
                videoSurface.SetEnable(true);
                videoSurface.SetGameFps(30);
            }
        return user;
            //Add this user in dictionary
        //    Users.Add(userID, user);
        //}
    }

    public void RemoveUserFromGrid(GameObject user)
    {           
        GameObject userVideo = user.transform.Find("Video").gameObject;
        VideoSurface videoSurface = userVideo.GetComponent<VideoSurface>();
        videoSurface.enabled = false;
        Destroy(user);        
    }    

    internal void ConfigureLocalUser()
    {
        localUser.ConfigureLocalUser();        
    }

    public void DisposeLocalUser()
    {
        localUser.DisposeLocalUser();
    }
}
