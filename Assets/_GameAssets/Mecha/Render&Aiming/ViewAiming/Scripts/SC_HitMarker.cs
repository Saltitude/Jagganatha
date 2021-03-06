﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_HitMarker : MonoBehaviour
{
    
    #region Singleton

    private static SC_HitMarker _instance;
    public static SC_HitMarker Instance { get { return _instance; } }

    #endregion


    MeshRenderer meshRenderer;
    Material mat;
    bool bAnimation;
    public GameObject Weapon;

    [SerializeField]
    float animationTime;
    float curTime;

    GameObject SFX_HitMarker;

    Vector3 initialScale;
    Quaternion initialRotation;
    [SerializeField]
    float scaleFactor;

    public bool hit;
    bool turn;

    public enum HitType { Normal, Critical,Koa, none };

    public void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
        meshRenderer = this.GetComponent<MeshRenderer>();
        mat = meshRenderer.material;
        bAnimation = false;
        curTime = 0;
        initialScale = transform.localScale;
        hit = false;
    }

    public void HitMark(HitType Type)
    {
        meshRenderer.enabled = true;
        mat.color = Color.white;
        bAnimation = true;
        curTime = 0;
        if(Type != HitType.none)
        {
            SFX_HitMarker = CustomSoundManager.Instance.PlaySound(gameObject, "SFX_p_HitMarker", false, 0.02f);
        }

        switch (Type)
        {

            case HitType.Normal :
                turn = false;
                transform.rotation = initialRotation;
                break;

            case HitType.Critical:
 
                turn = true;
                break;

            case HitType.Koa:
                mat.color = Color.green;


                break;


            case HitType.none:

                bAnimation = false;
                meshRenderer.enabled = false;
                transform.localScale = initialScale;

                turn = false;
                break;

        }
    }

    void Update()
    {
      

        if(bAnimation)
        {
            if(Weapon == null)
            {
                Weapon = GameObject.FindGameObjectWithTag("WeaponAnim");
            }
            /// INSERTFLICK
            /*
            curTime += Time.deltaTime;
            float scale = animationTime / (initialScale.x * scaleFactor);

            scale = scale * Time.deltaTime;
            
            if(curTime >= animationTime)
            {
                transform.localScale = initialScale;
            }
            else if(curTime >= animationTime/2)
            {
                transform.localScale -= new Vector3(scale, scale, scale);
            }
            else
            {
                transform.localScale += new Vector3(scale, scale, scale);
            }*/
            if (turn)
            {
                transform.Rotate(new Vector3(0, 0, 300 * Time.deltaTime));
                Weapon.GetComponent<Animator>().SetBool("IsCalib", true);
            }
            else
            {
                Weapon.GetComponent<Animator>().SetBool("IsCalib", false);
            }
        }
    }

}
