using UnityEngine;
using UnityEngine.UI;
using agora_gaming_rtc;
using UnityEngine.SceneManagement;
using io.agora.rtm;
using agora_rtm;
using System;

public class PlayerViewControllerBase : IVideoChatClient
{
    protected IRtcEngine mRtcEngine;
    //protected RtmChatManager rtmChatManager;
    private RtmClient rtmClient = null;
    protected RtmClientEventHandler clientEventHandler;

    private string AppID = "";

    //[SerializeField]
    private string token = "";

    protected const string SelfVideoName = "myImage";
    protected const string MainVideoName = "mainImage";

    string _userName = "";
    string UserName
    {
        get { return _userName; }
        set
        {
            _userName = value;
            PlayerPrefs.SetString("RTM_USER", _userName);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    ///   Where to display the video stream for remote user.  See
    /// the option SelfVideoName or MainVideoName.  Derived class to override.
    /// </summary>
    protected virtual string RemoteStreamTargetImage
    {
        get
        {
            return SelfVideoName;
        }
    }

    protected bool remoteUserJoined = false;

    public PlayerViewControllerBase(string appID)
    {
        this.AppID = appID;
    }

    /// <summary>
    ///   Join a RTC channel
    /// </summary>
    /// <param name="channel"></param>
    public virtual void Join(string channel)
    {
        Debug.Log("calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;

        // enable video
        mRtcEngine.EnableVideo();
        // allow camera output callback
        mRtcEngine.EnableVideoObserver();
        mRtcEngine.EnableLocalAudio(false);
        mRtcEngine.MuteLocalAudioStream(true);

        // join channel
        mRtcEngine.JoinChannel(channel, null, 0);        

        Debug.Log("initializeEngine done");
    }

    protected void InitializeRtmClient()
    {
        clientEventHandler = new RtmClientEventHandler();

        rtmClient = new RtmClient(this.AppID, clientEventHandler);
#if UNITY_EDITOR
        rtmClient.SetLogFile("./rtm_log.txt");
#endif

        clientEventHandler.OnQueryPeersOnlineStatusResult = OnQueryPeersOnlineStatusResultHandler;
        clientEventHandler.OnLoginSuccess = OnClientLoginSuccessHandler;
        clientEventHandler.OnLoginFailure = OnClientLoginFailureHandler;
        //clientEventHandler.OnMessageReceivedFromPeer = OnMessageReceivedFromPeerHandler;
    }

    void OnQueryPeersOnlineStatusResultHandler(int id, long requestId, PeerOnlineStatus[] peersStatus, int peerCount, QUERY_PEERS_ONLINE_STATUS_ERR errorCode)
    {
        if (peersStatus.Length > 0)
        {
            Debug.Log("OnQueryPeersOnlineStatusResultHandler requestId = " + requestId +
            " peersStatus: peerId=" + peersStatus[0].peerId +
            " online=" + peersStatus[0].isOnline +
            " onlinestate=" + peersStatus[0].onlineState);
            //messageDisplay.AddTextToDisplay("User " + peersStatus[0].peerId + " online status = " + peersStatus[0].onlineState, Message.MessageType.Info);
            Debug.Log("User " + peersStatus[0].peerId + " online status = ");
        }
    }

    void OnClientLoginSuccessHandler(int id)
    {
        string msg = "client login successful! id = " + id;
        Debug.Log(msg);
        //messageDisplay.AddTextToDisplay(msg, Message.MessageType.Info);
    }

    void OnClientLoginFailureHandler(int id, LOGIN_ERR_CODE errorCode)
    {
        string msg = "client login unsuccessful! id = " + id + " errorCode = " + errorCode;
        Debug.Log(msg);
        //messageDisplay.AddTextToDisplay(msg, Message.MessageType.Error);
    }

    /// <summary>
    ///   Leave a RTC channel
    /// </summary>
    public virtual void Leave()
    {
        Debug.Log("calling leave");

        if (mRtcEngine == null)
            return;

        // leave channel
        mRtcEngine.LeaveChannel();
        // deregister video frame observers in native-c code
        mRtcEngine.DisableVideoObserver();
    }

    /// <summary>
    ///   Load the Agora RTC engine with given AppID
    /// </summary>
    /// <param name="appId">Get the APP ID from Agora account</param>
    public void LoadEngine(string appId)
    {
        // init engine
        mRtcEngine = IRtcEngine.GetEngine(appId);

        // enable log
        mRtcEngine.SetLogFilter(LOG_FILTER.DEBUG | LOG_FILTER.INFO | LOG_FILTER.WARNING | LOG_FILTER.ERROR | LOG_FILTER.CRITICAL);
    }

    // unload agora engine
    public virtual void UnloadEngine()
    {
        Debug.Log("calling unloadEngine");

        // delete
        if (mRtcEngine != null)
        {
            IRtcEngine.Destroy();  // Place this call in ApplicationQuit
            mRtcEngine = null;
        }
    }

    /// <summary>
    ///   Enable/Disable video
    /// </summary>
    /// <param name="pauseVideo"></param>
    public void EnableVideo(bool pauseVideo)
    {
        if (mRtcEngine != null)
        {
            if (!pauseVideo)
            {
                mRtcEngine.EnableVideo();
            }
            else
            {
                mRtcEngine.DisableVideo();
            }
        }
    }

    public virtual void OnSceneLoaded()
    {
        // find a game object to render video stream from 'uid'
        GameObject go = GameObject.Find(RemoteStreamTargetImage);
        if (go == null)
        {
            return;
        }

        VideoSurface videoSurface = go.AddComponent<VideoSurface>();
        videoSurface.enabled = false;

        go = GameObject.Find("ButtonExit");
        if (go != null)
        {
            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnLeaveButtonClicked);
            }
        }
        SetupToggleMic();         
    }

    // implement engine callbacks
    protected virtual void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("JoinChannelSuccessHandler: uid = " + uid);
        mRtcEngine.OnFirstRemoteVideoDecoded += (a, b, c, d) =>
        {
            Debug.LogWarningFormat("OnFirstRemoteVideoDecoded: uid:{0} w:{1} h:{2} elapsed:{3}", a, b, c, d);
        };

        ///Login the rtmClient
        LoginRtmClient(uid.ToString());
    }

    private void LoginRtmClient(string userName)
    {
        UserName = userName;// userNameInput.text;

        if (string.IsNullOrEmpty(UserName))
        {
            Debug.LogError("We need a username to login");
            return;
        }

        rtmClient.Login(token, UserName);
    }

    void LogoutRtmClient()
    {
        //messageDisplay.AddTextToDisplay(UserName + " logged out of the rtm", Message.MessageType.Info);
        Debug.Log(UserName + " logged out of the rtm");
        rtmClient.Logout();
    }

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    protected virtual void OnUserJoined(uint uid, int elapsed)
    {
        Debug.Log("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
        // this is called in main thread

        // find a game object to render video stream from 'uid'
        GameObject go = GameObject.Find(RemoteStreamTargetImage);
        if (go == null)
        {
            return;
        }

        VideoSurface videoSurface = go.GetComponent<VideoSurface>();
        if (videoSurface != null)
        {
            videoSurface.enabled = true;
            // configure videoSurface
            videoSurface.SetForUser(uid);
            videoSurface.SetEnable(true);
            videoSurface.SetGameFps(30);
        }
    }

    // When remote user is offline, this delegate will be called. Typically
    // delete the GameObject for this user
    protected virtual void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        // remove video stream
        Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);
        // this is called in main thread
        GameObject go = GameObject.Find(RemoteStreamTargetImage);
        if (go != null)
        {
            RawImage rawImage = go.GetComponent<RawImage>();
            if (rawImage == null)
            {
                return;
            }

            VideoSurface videoSurface = go.GetComponent<VideoSurface>();
            videoSurface.enabled = false;
        }
    }

    private void OnLeaveButtonClicked()
    {
        Leave(); // leave channel
        UnloadEngine(); // delete engine

        //Logging out from rtmClient
        LogoutRtmClient();
        DisposeRtm();

        SceneManager.LoadScene(GameController.HomeSceneName, LoadSceneMode.Single);
        GameObject gameObject = GameObject.Find("GameController");
        UnityEngine.Object.Destroy(gameObject);

        Screen.sleepTimeout = SleepTimeout.SystemSetting;
    }

    private void SetupToggleMic()
    {

        GameObject go = GameObject.Find("ToggleButton");
        if (go != null)
        {
            ToggleButton toggle = go.GetComponent<ToggleButton>();
            if (toggle != null)
            {
                toggle.button1.onClick.AddListener(() =>
                {
                    toggle.Tap();
                    mRtcEngine.EnableLocalAudio(false);
                    mRtcEngine.MuteLocalAudioStream(true);
                });
                toggle.button2.onClick.AddListener(() =>
                {
                    toggle.Tap();
                    mRtcEngine.EnableLocalAudio(true);
                    mRtcEngine.MuteLocalAudioStream(false);
                });
            }
        }
    }

    public void DisposeRtm()
    {
        if (rtmClient != null)
        {
            rtmClient.Dispose();
            rtmClient = null;
        }
    }
}
