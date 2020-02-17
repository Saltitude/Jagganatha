﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_BulletFlock : MonoBehaviour
{
    Rigidbody rb = null;
    void Start()
    {
        GetRigidBody();
    }

    private void OnTriggerEnter(Collider other)
    {
        //JE TOUCHE LE PLAYER 
        if(other.tag == "Player")
        {
            Sc_ScreenShake.Instance.ShakeIt(0.005f,0.1f);
        }
     
    }

    void ResetPos()
    {

        if (rb == null)
            GetRigidBody();

        if (rb != null)
        {
            GetComponent<Rigidbody>().isKinematic = true;
            transform.position = new Vector3(1000, 1000, 1000);
        }

    }


    void GetRigidBody()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            Debug.LogWarning("SC_BulletMiniGun - ResetPos - Can't Find RigidBody");
    }
}
