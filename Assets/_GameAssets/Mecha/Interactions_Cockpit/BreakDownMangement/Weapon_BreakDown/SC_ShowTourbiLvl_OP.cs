﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_ShowTourbiLvl_OP : MonoBehaviour
{

    [Header("References")]
    [SerializeField]
    RectTransform[] tab_TorbiBar;

    [SerializeField]
    float f_MaxLenght;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBar();
    }

    void UpdateBar()
    {
        for(int i = 0; i < tab_TorbiBar.Length; i++)
        {

            float TargetValue = SC_SyncVar_BreakdownWeapon.Instance.SL_Tourbilols[i].valueWanted;
            float CurrentValue = SC_SyncVar_BreakdownWeapon.Instance.SL_Tourbilols[i].value;

            if (CurrentValue >= 0)
            {
                tab_TorbiBar[i].pivot = new Vector2(1, 0.5f);
                tab_TorbiBar[i].localPosition = new Vector2(0, tab_TorbiBar[i].localPosition.y);
            }       
            else
            {
                tab_TorbiBar[i].pivot = new Vector2(0, 0.5f);
                tab_TorbiBar[i].localPosition = new Vector2(0, tab_TorbiBar[i].localPosition.y);
            }

            if (CurrentValue < 0)
                CurrentValue *= -1;

            tab_TorbiBar[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurrentValue * f_MaxLenght);

        }
    }

}
