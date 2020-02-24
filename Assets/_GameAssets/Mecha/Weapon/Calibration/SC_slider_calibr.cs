﻿using UnityEngine;
using System.Collections;

public class SC_slider_calibr : MonoBehaviour {
  private ViveGrip_ControllerHandler controller;
  private float oldX;
  private int VIBRATION_DURATION_IN_MILLISECONDS = 50;
  private float MAX_VIBRATION_STRENGTH = 0.2f;
  private float MAX_VIBRATION_DISTANCE = 0.03f;


    private float _localX = 0;
    private float _localY = 0;
    private float _localZ = 0;
    public bool _freezeAlongX = true;
    public bool _freezeAlongY = false;
    public bool _freezeAlongZ = true;

    /// <summary>
    /// Index du slider pour sa structList
    /// </summary>
    /// 

    public int index;


    private GameObject Mng_SyncVar;
    private Rigidbody sliderRigidbody;


    private SC_SyncVar_calibr sc_syncvar_calibr;


    void Start ()
    {
        oldX = transform.position.x;
        sliderRigidbody = gameObject.GetComponent<Rigidbody>();
        GetReferences();
    }

    void GetReferences()
    {
        
        if (Mng_SyncVar == null)
            Mng_SyncVar = GameObject.FindGameObjectWithTag("Mng_SyncVar");
        if (Mng_SyncVar != null && sc_syncvar_calibr == null)
            sc_syncvar_calibr = Mng_SyncVar.GetComponent<SC_SyncVar_calibr>();

    }

    void ViveGripGrabStart(ViveGrip_GripPoint gripPoint)
    {
    controller = gripPoint.controller;
        sliderRigidbody.isKinematic = false;
    }

    void ViveGripGrabStop()
    {
        controller = null;
        sliderRigidbody.isKinematic = true;
    }

	void Update () {
        
        //on traduit la position en position locale pour la freeze
        _localX = transform.localPosition.x;
        _localY = transform.localPosition.y;
        _localZ = transform.localPosition.z;

        if (_freezeAlongX) _localX = 0;
        if (_freezeAlongY) _localY = 0;
        if (_freezeAlongZ) _localZ = 0;
        gameObject.transform.localPosition = new Vector3(_localX, _localY, _localZ);

        //sécurité juste en y

        if (transform.localPosition.y < -0.45f)
        {

            transform.localPosition = new Vector3(transform.localPosition.x, -0.45f, transform.localPosition.z);

        }
        else if (transform.localPosition.y > 0.45f)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0.45f, transform.localPosition.z);

        }


        float newX = gameObject.transform.localPosition.y;

        //on envoie la valeur à la syncvar si celle ci a changé
        if (newX != oldX) sendToSynchVar(Mathf.Round(gameObject.transform.localPosition.y*100)/100);



        if (controller != null) {
          float distance = Mathf.Min(Mathf.Abs(newX - oldX), MAX_VIBRATION_DISTANCE);
          float vibrationStrength = (distance / MAX_VIBRATION_DISTANCE) * MAX_VIBRATION_STRENGTH;
          controller.Vibrate(VIBRATION_DURATION_IN_MILLISECONDS, vibrationStrength);
        }
        oldX = newX;

 


    }


    

    void sendToSynchVar (float value)
    {
     
        if (sc_syncvar_calibr == null)
        {
            GetReferences();
        }
        else
        {

            value = (value + 0.4f) * 6.25f;

            int intvalue = (int)value;

            sc_syncvar_calibr.CalibrInts[index] = intvalue;


            int curWeaponIndex = SC_WeaponManager.Instance.n_CurWeapon;
  
            GameObject curWeapon = SC_WeaponManager.Instance.tab_Weapons[curWeaponIndex];


            curWeapon.GetComponent<IF_Weapon>().SetSensitivity(index, intvalue);
       

        }          

    }

    /**
     * Return the input value according a given range translated to an other range.
     * @param float inputValue
     * @param float inputMax
     * @param float outputMax
     * @param float inputMin
     * @param float outputMin
     * @return float
     */
    float ratio(float inputValue, float inputMax, float outputMax, float inputMin = 0.0f, float outputMin = 0.0f)
    {
        float product = (inputValue - inputMin) / (inputMax - inputMin);
        float output = ((outputMax - outputMin) * product) + outputMin;
        return output;
    }


}
