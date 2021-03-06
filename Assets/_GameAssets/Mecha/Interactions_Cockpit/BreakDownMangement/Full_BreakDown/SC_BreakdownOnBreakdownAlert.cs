﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// TIMER s'affichant pour les pannes totales
/// </summary>

public class SC_BreakdownOnBreakdownAlert : MonoBehaviour
{

    #region Singleton

    private static SC_BreakdownOnBreakdownAlert _instance;
    public static SC_BreakdownOnBreakdownAlert Instance { get { return _instance; } }

    #endregion

    public float timerDuration = 30f;
    float timer = 0f;

    public GameObject go_timer;
    public GameObject go_Overload;
    public GameObject go_Repar;

    public TextMeshPro textComponent;
    public TextMeshPro textOverload;
    public TextMeshPro textRepar;

    Coroutine coroutineTimer;

    public float clignFreq = 1f;

    float timerStorage=0;

    private void Awake()
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


        private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            LaunchGlobalAlert();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            StopGlobalAlert();
        }
    }

    IEnumerator Timer(float duration)
    {
        while (SC_BreakdownDisplayManager.Instance.CurNbOfBreakdown != 0 || SC_WeaponBreakdown.Instance.CurNbOfBreakdown != 0 || SC_MovementBreakdown.Instance.n_InteractibleInBreakDown != 0)
        {
            go_timer.SetActive(true);
            go_Overload.SetActive(true);
            go_Repar.SetActive(true);
            textComponent.color = Color.white;
            textOverload.color = Color.white;
            textRepar.color = Color.white;

            timer = duration;

            while (timer > 0)
            {
                timer -= Time.deltaTime;

                if (timer < 0)
                    timer = 0;

                if (go_timer.activeSelf == true && timer % clignFreq > Random.Range(0f, 1))
                {
                    go_timer.SetActive(false);
                    go_Overload.SetActive(false);
                    timerStorage = timer;

                }


                if (go_timer.activeSelf == false && timerStorage - timer > 0.02f)
                {
                    go_timer.SetActive(true);
                    go_Overload.SetActive(true);
                }



                textComponent.SetText(((Mathf.Round(timer * 100)) / 100).ToString());

                yield return null;
            }

            if(timer == 0)
            {
                Sc_ScreenShake.Instance.ShakeIt(0.02f, 1f);
                textOverload.SetText("Overload!");
            }

            //Debug.Log("Damage/shake/FX");

            for (int i = 0; i < 4; i++)
            {
                yield return new WaitForSeconds(.4f);
                go_Overload.SetActive(false);
                go_timer.SetActive(false);
                if (i < 3)
                {
                    textComponent.SetText(textComponent.text + "0");
                    textOverload.SetText("Overload!");
                }
                    
                else
                {
                    textComponent.SetText("XXXX");
                    textComponent.color = Color.red;
                    textOverload.SetText("");
                }


                yield return new WaitForSeconds(.4f);
                go_timer.SetActive(true);
                go_Overload.SetActive(true);
            }

            yield return new WaitForSeconds(1.5f);

            textComponent.SetText("ERROR");
            textOverload.SetText("Overload!");

            yield return new WaitForSeconds(1.5f);

            textComponent.SetText("ALERT");
            textOverload.SetText("");

            yield return new WaitForSeconds(0.8f);

            textComponent.SetText("CANNOT");
            textOverload.SetText("Overload!");

            yield return new WaitForSeconds(0.6f);

            textComponent.SetText("REBOOT");
            textOverload.SetText("");

            yield return new WaitForSeconds(0.8f);

            textComponent.SetText("CRITICAL");
            textOverload.SetText("Overload!");

            yield return new WaitForSeconds(0.6f);

            textComponent.SetText("FAILURE");
            textOverload.SetText("");


            for (int i = 0; i < 9; i++)
            {
                go_timer.SetActive(false);
                go_Overload.SetActive(false);
                yield return new WaitForSeconds(0.1f);
                go_timer.SetActive(true);
                go_Overload.SetActive(true);
                yield return new WaitForSeconds(0.5f);

            }

            yield return new WaitForSeconds(1f);
            textOverload.SetText("Overload!");
            go_timer.SetActive(false);
            go_Overload.SetActive(true);

        }
        StopAllCoroutines();
    }



    public void LaunchGlobalAlert()

    {

        coroutineTimer = StartCoroutine("Timer", timerDuration);


    }


    public void StopGlobalAlert()
    {
        if(coroutineTimer != null)
            StopCoroutine(coroutineTimer);

        go_timer.SetActive(false);
        go_Overload.SetActive(false);
        go_Repar.SetActive(false);
    }

}
