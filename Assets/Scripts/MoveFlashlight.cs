using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskMovement : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.position.Set(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0)
            return;

        transform.position = Input.GetTouch(0).position;
    }
}
