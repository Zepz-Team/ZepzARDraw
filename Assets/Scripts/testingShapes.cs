using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testingShapes : MonoBehaviour
{
    [SerializeField]
    GameObject prefab;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {            
            Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.1f);
            GameObject obj = Instantiate(prefab, mousePos, Quaternion.identity);   
        }
    }
}
