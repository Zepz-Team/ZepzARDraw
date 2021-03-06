using System.Collections;
using agora_gaming_rtc;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using static agora_gaming_rtc.ExternalVideoFrame;


public interface ICallback
{
    void OnGOStateChanged(bool enable);
}

/// <summary>
///    Broadcast View Controller controls the client that uses the AR Camera to
/// Show the real world surrounding to the Audience client.  It receives the 
/// message about Audience client's drawing and draw in the Unity world space,
/// and such AR object is also included in the video sharing frames to show
/// to the Audience.
/// </summary>
public class BroadcasterVC : PlayerViewControllerBase, ICallback
{    
    Texture2D BufferTexture;

    public static TextureFormat ConvertFormat = TextureFormat.BGRA32;
    public static VIDEO_PIXEL_FORMAT PixelFormat = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;

    public static int ShareCameraMode = 1;  // 0 = unsafe buffer pointer, 1 = renderer iamge
    int i = 0; // monotonic timestamp counter

    Camera ARCamera;
    ARCameraManager cameraManager;
    MonoBehaviour monoProxy;

    RemoteDrawer remoteDrawer;

    string channelName = "";

    bool usesExternalVideoSource = false;

    public BroadcasterVC(string appID) : base(appID) { }

    public override void Join(string channel)
    {
        Debug.Log("calling join (channel = " + channel + ")");

        if (mRtcEngine == null)
            return;

        channelName = channel;

        // set callbacks (optional)
        mRtcEngine.OnJoinChannelSuccess = OnJoinChannelSuccess;
        mRtcEngine.OnUserJoined = OnUserJoined;
        mRtcEngine.OnUserOffline = OnUserOffline;

        CameraCapturerConfiguration config = new CameraCapturerConfiguration();
        config.preference = CAPTURER_OUTPUT_PREFERENCE.CAPTURER_OUTPUT_PREFERENCE_PERFORMANCE;
        //config.preference = CAPTURER_OUTPUT_PREFERENCE.CAPTURER_OUTPUT_PREFERENCE_AUTO;
        config.cameraDirection = CAMERA_DIRECTION.CAMERA_FRONT;
        mRtcEngine.SetCameraCapturerConfiguration(config);

        VideoEncoderConfiguration abc = new VideoEncoderConfiguration();
        abc.dimensions = new VideoDimensions { width = 360, height = 640 };
        abc.frameRate = FRAME_RATE.FRAME_RATE_FPS_15;
        abc.bitrate = 800;
        abc.orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_FIXED_PORTRAIT;
        abc.degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_QUALITY;

        int s = mRtcEngine.SetVideoEncoderConfiguration(abc);

        //int s = mRtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration
        //{
        //    /* dimensions = new VideoDimensions { width = 360, height = 640 },
        //     frameRate = FRAME_RATE.FRAME_RATE_FPS_24,
        //     bitrate = 800,
        //     orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_FIXED_PORTRAIT*/

        //    dimensions = new VideoDimensions { width = 720, height = 1280 },
        //    frameRate = FRAME_RATE.FRAME_RATE_FPS_30,
        //    bitrate = 1710,
        //    orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_FIXED_PORTRAIT
        //});
        Debug.Assert(s == 0, "RTC set video encoder configuration failed.");

        // enable video
        mRtcEngine.EnableVideo();
        // allow camera output callback
        mRtcEngine.EnableVideoObserver();
        //mRtcEngine.EnableLocalVideo(false);

        //  mRtcEngine.SetVideoQualityParameters(true);
        //mRtcEngine.SetExternalVideoSource(false, false);
        mRtcEngine.EnableLocalAudio(false);
        mRtcEngine.MuteLocalAudioStream(true);

        mRtcEngine.SetChannelProfile(GameController.ChannelProfile);
        mRtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        // join channel
        mRtcEngine.JoinChannel(channelName, null, 0);

        // Optional: if a data stream is required, here is a good place to create it
        int streamID = mRtcEngine.CreateDataStream(true, true);

        InitializeRtmClient();        

        Debug.Log("initializeEngine done, data stream id = " + streamID);
    }

    public override void OnSceneLoaded()
    {
        base.OnSceneLoaded();

        GameObject rtmManager = GameObject.Find("DrawListener");
        if (rtmManager != null)
        {
            remoteDrawer = rtmManager.GetComponent<RemoteDrawer>();

            mRtcEngine.OnStreamMessage += remoteDrawer.OnStreamMessageHandler;

            //clientEventHandler.OnMessageReceivedFromPeer = remoteDrawer.OnWebStreamMessageHandler;            
        }

        GameObject go = GameObject.Find("ButtonColor");
        if (go != null)
        {
            // the button is only available for AudienceVC
            go.SetActive(false);
        }        

        go = GameObject.Find("ShareScreen");
        {
            ShareScreen shrScreen = go.GetComponent<ShareScreen>();
            shrScreen.callback = this;
        }

        ///Login the rtmClient
        LoginRtmClient();
    }

    protected override void OnMessageReceivedFromPeerHandler(int id, string peerId, agora_rtm.TextMessage message)
    {
        base.OnMessageReceivedFromPeerHandler(id, peerId, message);
        remoteDrawer.OnWebStreamMessageHandler(id, peerId, message);
    }   

    // When a remote user joined, this delegate will be called. Typically
    // create a GameObject to render video on it
    protected override void OnUserJoined(uint uid, int elapsed)
    {
        base.OnUserJoined(uid, elapsed);
    }

    protected override void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        base.OnJoinChannelSuccess(channelName, uid, elapsed);
        //TODO: instead of calling below method, enable/disable UI sharing button when user joins or leaves
        if (usesExternalVideoSource)
            EnableSharing();           
    }    

    public override void Leave()
    {
        //TODO: instead of calling below method, enable/disable UI sharing button when user joins or leaves
        DisableSharing();

        base.Leave();
    }    

    // When remote user is offline, this delegate will be called. Typically
    // delete the GameObject for this user
    protected override void OnUserOffline(uint uid, USER_OFFLINE_REASON reason)
    {
        // remove video stream
        Debug.Log("onUserOffline: uid = " + uid + " reason = " + reason);

        base.OnUserOffline(uid, reason);
        // this is called in main thread
        GameObject go = GameObject.Find(uid.ToString());
        if (!ReferenceEquals(go, null))
        {
            UnityEngine.Object.Destroy(go);
        }
        //TODO: instead of calling below method, enable/disable UI sharing button when user joins or leaves
       DisableSharing();
    }
    /******************************/

    public void OnGOStateChanged(bool enable)
    {
        usesExternalVideoSource = enable;        

        // if sharing is enabled
        if (enable)
        {
            #region Find AR Camera, ARCameraManager and Sphere
            GameObject CameraGO = GameObject.Find("AR Camera");
            if (CameraGO != null)
            {
                //TODO: Take lock for setting monoPRoxy and cameraManager
                ARCamera = CameraGO.GetComponent<Camera>();
                //if (!ReferenceEquals(ARCamera, null))
                //{
                //    // Dynamically set the width & height of target render texture with that of screen resolution
                //    ARCamera.targetTexture.width = Screen.width;
                //    ARCamera.targetTexture.height = Screen.height;
                //}
                monoProxy = CameraGO.GetComponent<MonoBehaviour>();
                cameraManager = CameraGO.GetComponent<ARCameraManager>();

                if (cameraManager == null)
                {
                    Debug.Log("ARCameraManager object not found");
                    return;
                }
            } 
            //TODO: temporarily hiding the sphere
            //go = GameObject.Find("sphere");
            //if (go != null)
            //{
            //    var sphere = go;
            //    // hide this before AR Camera start capturing
            //    sphere.SetActive(false);
            //    monoProxy.StartCoroutine(DelayAction(.5f,
            //        () =>
            //        {
            //            sphere.SetActive(true);
            //        }));
            //}
            #endregion

            clientEventHandler.OnMessageReceivedFromPeer += OnMessageReceivedFromPeerHandler;
        } 
        else
        {
            GameObject.Destroy(ARCamera);
            //Subscribe to the OnMessageReceivedFromPeer event handler only when sharing is ON
            clientEventHandler.OnMessageReceivedFromPeer -= OnMessageReceivedFromPeerHandler;
        }

        ReJoinChannel(enable);

        //Call this to initialize instance of ARDrawManager in remoteDrawer
        //remoteDrawer.SetDrawManager(enable);
    }

    void ReJoinChannel(bool enable)
    {
        //Temporarily leave the channel
        mRtcEngine.LeaveChannel();

        //LogoutRtmClient();

        //Set external video source
        mRtcEngine.SetExternalVideoSource(enable, false);

        //Now join the channel again
        mRtcEngine.JoinChannel(channelName, null, 0);

        //LoginRtmClient();
    }   

        /***************Sharing Screen Methods*********************/
        public void EnableSharing()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
        //GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");//.GetComponent<Camera>();
        //RenderTexture renderTexture = camera.GetComponent<Camera>().targetTexture;        

        //GameObject camera = GameObject.Find("AR Camera");
        RenderTexture renderTexture = ARCamera.targetTexture;

        if (renderTexture != null)
        {
            BufferTexture = new Texture2D(renderTexture.width, renderTexture.height, ConvertFormat, false);
            Debug.Log("BufferTexture resolution " + renderTexture.width + " , " + renderTexture.height);
            // Editor only, where onFrameReceived won't invoke
            if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            {
                Debug.LogWarning(">>> Testing in Editor, start coroutine to capture Render data");
                monoProxy.StartCoroutine(CoShareRenderData());
            }
        }
    }

    /// <summary>
    ///   Delegate callback handles every frame generated by the AR Camera.
    /// </summary>
    /// <param name="eventArgs"></param>
    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        // There are two ways doing the capture. 
        if (ShareCameraMode == 0)
        {
            // See function header for what this function is
            // CaptureARBuffer();
        }
        else
        {
            ShareRenderTexture();
        }
    }

    // Uncomment the follow function to try out XRCameraImage method to 
    // get the image of the AR Camera. Requires unsafe code compilation option
    // in Settings.
    /*
    private unsafe void CaptureARBuffer()
    {
        // Get the image in the ARSubsystemManager.cameraFrameReceived callback

        XRCameraImage image;
        if (!cameraManager.TryGetLatestImage(out image))
        {
            Debug.LogWarning("Capture AR Buffer returns nothing!!!!!!");
            return;
        }

        var conversionParams = new XRCameraImageConversionParams
        {
            // Get the full image
            inputRect = new RectInt(0, 0, image.width, image.height),

            // Downsample by 2
            outputDimensions = new Vector2Int(image.width, image.height),

            // Color image format
            outputFormat = ConvertFormat,

            // Flip across the x axis
            transformation = CameraImageTransformation.MirrorX

            // Call ProcessImage when the async operation completes
        };
        // See how many bytes we need to store the final image.
        int size = image.GetConvertedDataSize(conversionParams);

        // Allocate a buffer to store the image
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        // Extract the image data
        image.Convert(conversionParams, new System.IntPtr(buffer.GetUnsafePtr()), buffer.Length);

        // The image was converted to RGBA32 format and written into the provided buffer
        // so we can dispose of the CameraImage. We must do this or it will leak resources.

        byte[] bytes = buffer.ToArray();
        monoProxy.StartCoroutine(PushFrame(bytes, image.width, image.height,
                 () => { image.Dispose(); buffer.Dispose(); }));
    }
    */

    /// <summary>
    ///   Get the image from renderTexture.  (AR Camera must assign a RenderTexture prefab in
    /// its renderTexture field.)
    /// </summary>
    private void ShareRenderTexture()
    {
        if (BufferTexture == null) // offlined
        {
            return;
        }
        //Camera targetCamera =  Camera.main; // ARCamera
        //RenderTexture.active = targetCamera.targetTexture; // the targetTexture holds render texture
        //Rect rect = new Rect(0, 0, targetCamera.targetTexture.width, targetCamera.targetTexture.height);

        RenderTexture.active = ARCamera.targetTexture;
        Rect rect = new Rect(0, 0, ARCamera.targetTexture.width, ARCamera.targetTexture.height);

        BufferTexture.ReadPixels(rect, 0, 0);
        BufferTexture.Apply();

        byte[] bytes = BufferTexture.GetRawTextureData();

        // sends the Raw data contained in bytes
        monoProxy.StartCoroutine(PushFrame(bytes, (int)rect.width, (int)rect.height,
         () =>
         {
             bytes = null;
         }));
        RenderTexture.active = null;
    }

    /// <summary>
    ///    For use in Editor testing only.
    /// </summary>
    /// <returns></returns>
    IEnumerator CoShareRenderData()
    {
        while (ShareCameraMode == 1)
        {
            yield return new WaitForEndOfFrame();
            OnCameraFrameReceived(default);
        }
        yield return null;
    }

    /// <summary>
    /// Push frame to the remote client.  This is the same code that does ScreenSharing.
    /// </summary>
    /// <param name="bytes">raw video image data</param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="onFinish">callback upon finish of the function</param>
    /// <returns></returns>
    IEnumerator PushFrame(byte[] bytes, int width, int height, System.Action onFinish)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("Zero bytes found!!!!");
            yield break;
        }

       // IRtcEngine rtc = IRtcEngine.QueryEngine();
        //if the engine is present
        if (mRtcEngine != null)
        {
            //Create a new external video frame
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            //Set the buffer type of the video frame
            externalVideoFrame.type = ExternalVideoFrame.VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            // Set the video pixel format
            externalVideoFrame.format = PixelFormat; // VIDEO_PIXEL_BGRA for now
            //apply raw data you are pulling from the rectangle you created earlier to the video frame
            externalVideoFrame.buffer = bytes;
            //Set the width of the video frame (in pixels)
            externalVideoFrame.stride = width;
            //Set the height of the video frame
            externalVideoFrame.height = height;
            //Remove pixels from the sides of the frame
            //externalVideoFrame.cropLeft = 10;
            //externalVideoFrame.cropTop = 10;
            //externalVideoFrame.cropRight = 10;
            //externalVideoFrame.cropBottom = 10;
            //Rotate the video frame (0, 90, 180, or 270)
            externalVideoFrame.rotation = 180; //Check after removing this rotation value
            // increment i with the video timestamp
            externalVideoFrame.timestamp = i++;
            //Push the external video frame with the frame we just created
            // int a = 
            mRtcEngine.PushVideoFrame(externalVideoFrame);
            // Debug.Log(" pushVideoFrame(" + i + ") size:" + bytes.Length + " => " + a);

        }
        yield return null;
        onFinish();
    }

    void DisableSharing()
    {
        //cameraManager.frameReceived -= OnCameraFrameReceived;

        //TODO: Take lock for setting monoPRoxy and cameraManager
        monoProxy = null;
        cameraManager = null;

        BufferTexture = null;
    }

    IEnumerator DelayAction(float delay, System.Action doAction)
    {
        yield return new WaitForSeconds(delay);
        doAction();
    }   
}
