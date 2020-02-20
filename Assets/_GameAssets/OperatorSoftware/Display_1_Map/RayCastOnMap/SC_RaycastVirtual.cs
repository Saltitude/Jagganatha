﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_RaycastVirtual : MonoBehaviour
{
    private RaycastHit hit;
    private Camera curCam;

    //public GameObject guide;
    void Start()
    {
        curCam = this.gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        //int layerMask = 1 << 9;
        //Cast un ray à partir du casque
        if(Input.GetMouseButton(0))
        {
            Ray ray = curCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                //Debug.DrawRay(transform.position, hit.point * hit.distance, Color.yellow);

            }
        }

    }

    /// <summary>
    /// Renvois le raycasthit du cockpit
    /// </summary>
    /// <returns></returns>
    public RaycastHit getRay()
    {
        return hit;
    }

}