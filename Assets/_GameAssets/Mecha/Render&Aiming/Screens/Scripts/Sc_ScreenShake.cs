﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_ScreenShake : MonoBehaviour
{

    #region Singleton

    private static Sc_ScreenShake _instance;
    public static Sc_ScreenShake Instance { get { return _instance; } }

    #endregion
    // Transform of the camera to shake. Grabs the gameObject's transform
    // if null.
    Transform screenTransform;
    //public Transform screenBDTransform;

    // How long the object should shake for.
    public float shakeDuration = 0f;

    // Amplitude of the shake. A larger value shakes the camera harder.
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    float maxShakeDuration = 2f;
    bool maxShakeReach = false;

    Vector3 originalPos;
    Vector3 originalPosBD;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
        if (screenTransform == null)
        {
            //screenTransform = GetComponent(typeof(Transform)) as Transform;
            screenTransform = this.GetComponent<Transform>();
        }
    }

    void OnEnable()
    {
        originalPos = screenTransform.localPosition;
        //originalPosBD = screenBDTransform.localPosition;
    }

    public void ShakeIt(float amplitude, float duration)
    {
        if (shakeDuration < maxShakeDuration && !maxShakeReach)
        {
            shakeAmount = amplitude;
            shakeDuration = shakeDuration + duration;
        }
        else
        {
            maxShakeReach = true;
        }

        if (shakeDuration <= 0)
        {
            maxShakeReach = false;
        }
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            screenTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;
            //screenBDTransform.localPosition = originalPosBD + Random.insideUnitSphere * shakeAmount;

            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
            screenTransform.localPosition = originalPos;
            //screenBDTransform.localPosition = originalPosBD;
        }

    }
}