using System.Collections;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

public class RemoteDrawer : MonoBehaviour
{ 
    private ARDrawManager drawManager = null;

    public ARDrawManager DrawManager
    {
        get
        {
            if (drawManager == null)
            {
                GameObject GO = GameObject.Find("AR Session Origin");

                if (!ReferenceEquals(GO, null))
                {
                    drawManager = GO.GetComponent<ARDrawManager>();
                }
            }
            return drawManager;
        }
        set { drawManager = value; }
    }    

    //private Color DrawColor = Color.black;     

    //public void SetDrawManager(bool allow)
    //{
    //    if (allow)
    //    {
    //        GameObject GO = GameObject.Find("AR Session Origin");

    //        if (!ReferenceEquals(GO, null))
    //        {
    //            drawManager = GO.GetComponent<ARDrawManager>();
    //        }
    //    }       
    //}

    /// <summary>
    ///    The delegate function to handle message sent from Android mobile Audience side
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="streamId"></param>
    /// <param name="data"></param>
    /// <param name="length"></param>
    public void OnStreamMessageHandler(uint userId, int streamId, byte[] buffer, int length)
    {
        string data = System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        if (data.Contains("color"))
        {
            StartCoroutine(CoProcessDrawingData(data));
        }
        else if (data.Contains("clear"))
        {
            //ClearLines();
            //Destroy(anchorGO);
            DrawManager?.ClearLines();
        }

        Debug.Log("Main Camera pos = " + Camera.main.transform.position);
    }

    /// <summary>
    ///    The delegate function to handle message sent from Web Audience side
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="streamId"></param>
    /// <param name="data"></param>
    /// <param name="length"></param>
    public void OnWebStreamMessageHandler(int id, string peerId, agora_rtm.TextMessage message)//(int userId, string msg)
    {
        //string data = System.Text.Encoding.UTF8.GetString(buffer, 0, length);
        string data = message.GetText();

        //if (data.Contains("color"))
        if (data.Contains("points"))
        {
            StartCoroutine(CoProcessWebDrawingData(data));
        }
        else if (data.Contains("clear"))
        {
            //ClearLines();
            //Destroy(anchorGO);
            DrawManager?.ClearLines();
        }

        Debug.Log("Main Camera pos = " + Camera.main.transform.position);
    }

    /// <summary>
    ///  Do the drawing async
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IEnumerator CoProcessDrawingData(string data)
    {
        try
        {
            DrawmarkModel dm = JsonUtility.FromJson<DrawmarkModel>(data);            
            foreach (Vector2 pos in dm.points)
            {
                // DrawDot(pos);
                Debug.Log("Touch Position: " + pos.ToString());                
                DrawManager?.DrawOnTouch(pos);
            }            
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        yield return null;
    }

    IEnumerator CoProcessWebDrawingData(string data)
    {
        try
        {
            DrawmarkModel2 dm = JsonUtility.FromJson<DrawmarkModel2>(data);           

            DrawManager?.DrawOnMouse5(dm.points);

            //drawManager.DrawDot(dm.points);            
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
        yield return null;
    }    
}
