﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_CordViewver : MonoBehaviour
{

    #region Singleton

    private static SC_CordViewver _instance;
    public static SC_CordViewver Instance { get { return _instance; } }

    #endregion

    [Header("References")]
    [SerializeField]
    RectTransform[] tab_CordBars;
    [SerializeField]
    GameObject[] tab_CordCursor;

    [Header("Bars Parameters")]
    [SerializeField]
    float BarMaxLenght;
    [SerializeField]
    float BarValidLenght;
    [SerializeField, Range(0,1)]
    float RatioFactor;

    [Header("Basics Infos")]
    [SerializeField]
    float ConstraintRange;
    [SerializeField]
    float DeadZone;
    [SerializeField]
    float AddMaxRange; 

    [Header("Ratio Infos")]
    [SerializeField]
    float CordRatio01;
    [SerializeField]
    bool b_Cord01Valid;
    [SerializeField]
    float CordRatio02;
    [SerializeField]
    bool b_Cord02Valid;
    [SerializeField]
    float CordRatio03;
    [SerializeField]
    bool b_Cord03Valid;

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

    }

    // Start is called before the first frame update
    void Start()
    {
        GetBaseValue();
    }

    // Update is called once per frame
    void Update()
    {
        ComputeRatio();
        ScaleCordBar();
    }

    public void GetBaseValue()
    {
        ConstraintRange = SC_SyncVar_MovementSystem.Instance.ConstraintRange;
        DeadZone = SC_SyncVar_MovementSystem.Instance.DeadZone;
        AddMaxRange = SC_SyncVar_MovementSystem.Instance.AddMaxRange;
        ComputeRatio();
    }

    public void ComputeRatio()
    {

        CordRatio01 = (SC_SyncVar_MovementSystem.Instance.CordLenght01 - (ConstraintRange * RatioFactor)) / (DeadZone + ConstraintRange - (ConstraintRange * RatioFactor));
        CordRatio02 = (SC_SyncVar_MovementSystem.Instance.CordLenght02 - (ConstraintRange * RatioFactor)) / (DeadZone + ConstraintRange - (ConstraintRange * RatioFactor));
        CordRatio03 = (SC_SyncVar_MovementSystem.Instance.CordLenght03 - (ConstraintRange * RatioFactor)) / (DeadZone + ConstraintRange - (ConstraintRange * RatioFactor));

        if (CordRatio01 >= 1 && !b_Cord01Valid)
            b_Cord01Valid = true;
        else if(b_Cord01Valid)
            b_Cord01Valid = false;

        if (CordRatio02 >= 1 && !b_Cord02Valid)
            b_Cord02Valid = true;
        else if (b_Cord01Valid)
            b_Cord02Valid = false;

        if (CordRatio03 >= 1 && !b_Cord03Valid)
            b_Cord03Valid = true;
        else if (b_Cord03Valid)
            b_Cord03Valid = false;

    }

    void ScaleCordBar()
    {

        float TargetSize01 = BarValidLenght * CordRatio01;
        if (TargetSize01 >= BarMaxLenght)
            TargetSize01 = BarMaxLenght;

        float TargetSize02 = BarValidLenght * CordRatio02;
        if (TargetSize02 >= BarMaxLenght)
            TargetSize02 = BarMaxLenght;

        float TargetSize03 = BarValidLenght * CordRatio03;
        if (TargetSize03 >= BarMaxLenght)
            TargetSize03 = BarMaxLenght;

        tab_CordBars[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TargetSize01);
        tab_CordBars[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TargetSize02);
        tab_CordBars[2].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, TargetSize03);

    }

    public void ShowSelection()
    {
        switch (SC_SyncVar_MovementSystem.Instance.CurSelectedCord)
        {

            default:
                tab_CordCursor[0].SetActive(false);
                tab_CordCursor[1].SetActive(false);
                tab_CordCursor[2].SetActive(false);
                break;

            case 1:
                tab_CordCursor[0].SetActive(true);
                tab_CordCursor[1].SetActive(false);
                tab_CordCursor[2].SetActive(false);
                break;

            case 2:
                tab_CordCursor[0].SetActive(false);
                tab_CordCursor[1].SetActive(true);
                tab_CordCursor[2].SetActive(false);
                break;

            case 3:
                tab_CordCursor[0].SetActive(false);
                tab_CordCursor[1].SetActive(false);
                tab_CordCursor[2].SetActive(true);
                break;

        }
    }

}
