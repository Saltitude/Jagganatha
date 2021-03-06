﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sc_LaserFeedBack : MonoBehaviour
{

    #region Singleton

    private static Sc_LaserFeedBack _instance;
    public static Sc_LaserFeedBack Instance { get { return _instance; } }

    #endregion

    public SC_WeaponLaserGun MainLaserScript;
    public GameObject FirePoint;

    #region SoundDesign
    //GameObject SFX_LaserBeam;
    [SerializeField]
    AudioSource SFX_LaserBeam;
    [SerializeField]
    AudioClip LaserStart;
    [SerializeField]
    AudioClip LaserLoop;
    [SerializeField]
    AudioClip LaserEnd;
    int SoundSourceNumb;
    #endregion

    [SerializeField]
    public Color CurColor;
    public GameObject Laser;
    public GameObject Laser1;
    public GameObject Laser2;
    public Material EnergyBall;
    public GameObject ChargeSpark;
    public GameObject Fioriture;
    public GameObject Fioriture1;
    public GameObject Ondes;
    public GameObject Elice;
    public GameObject EliceDark;
    public GameObject Electricity;
    [SerializeField]
    SC_WeaponLaserGun WeapMainSC;
    public ParticleSystem.MainModule LaserPS;
    public ParticleSystem.MainModule LaserPS1;
    public ParticleSystem.MainModule LaserPS2;
    //public ParticleSystem.MainModule EnergyBallPS;
    public ParticleSystem.MainModule ChargeSparkPS;
    public ParticleSystem.MainModule FioriturePS;
    public ParticleSystem.MainModule FioriturePS1;
    public ParticleSystem.MainModule OndesPS;
    public ParticleSystem.MainModule ElicePS;
    public ParticleSystem.MainModule EliceDarkPS;
    public ParticleSystem.MainModule ElectricityPS;
    [SerializeField]
    Animator Kahme;

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

    private void Update()
    {
        if (SFX_LaserBeam != null && !SFX_LaserBeam.isPlaying)
        {
            //Debug.Log("is not playing no null");
           // Debug.Log(SoundSourceNumb);
            //SFX_LaserBeam.transform.position = new Vector3(Laser.transform.position.x, -1000, Laser.transform.position.z);
            //Debug.Log(SFX_LaserBeam.transform.position);
        }

    }
    public void EnableLaser(RaycastHit hit)
    {
        if (SoundSourceNumb == 0)
        {
            //SFX_LaserBeam = CustomSoundManager.Instance.PlaySound(gameObject, "SFX_p_isShooting", true, 0.5f);
            //Debug.Log("EnableLaser");
            StartCoroutine(PlayLaserSound());
            SoundSourceNumb += 1;
            
        }

        Kahme.SetBool("IsFire", true);
    }

    public void DiseableLaser()
    {
        if(SFX_LaserBeam != null && SFX_LaserBeam.isPlaying/*&& SFX_LaserBeam.GetComponent<AudioSource>().isPlaying*/)
        {
            //SFX_LaserBeam.GetComponent<AudioSource>().Stop();
           // Debug.Log("DisableLaser");
            StopAllCoroutines();
            StartCoroutine(StopLaserSound());
        }
        SoundSourceNumb = 0;
        Kahme.SetBool("IsFire", false);
    }

    IEnumerator PlayLaserSound()
    {
        SFX_LaserBeam.clip = LaserStart;
        SFX_LaserBeam.volume = 0.2f;
        SFX_LaserBeam.Play();
        //Debug.Log("PlayStart");
        yield return new WaitForSeconds(SFX_LaserBeam.clip.length);
        //Debug.Log("PlayLoop");
        SFX_LaserBeam.loop = true;
        SFX_LaserBeam.clip = LaserLoop;
        SFX_LaserBeam.Play();
        yield return null;
    }
        IEnumerator StopLaserSound()
    {
        SFX_LaserBeam.loop = false;
        SFX_LaserBeam.clip = LaserEnd;
        SFX_LaserBeam.Play();
        //Debug.Log("PlayStop");
        yield return null;
        //yield return new WaitForSeconds(SFX_LaserBeam.clip.length - 1f);
        //SFX_LaserBeam.Stop();
        //Debug.Log("Stop");
    }


    public void SetLaserSize(int value)
    {
        Kahme.SetInteger("CalibValue",value);
    }

    public void SetColor(Color32 NewColor)
    {

        if (CurColor != NewColor)
        {
            CurColor = NewColor;

            Gradient gradiend = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];

            alphaKeys[0].time = 0;
            alphaKeys[0].alpha = 1;

            alphaKeys[1].time = 1;
            alphaKeys[1].alpha = 1;

            colorKeys[0].color = NewColor;
            colorKeys[1].color = NewColor;
            colorKeys[2].color = NewColor;

            gradiend.SetKeys(colorKeys, alphaKeys);
            gradiend.SetKeys(colorKeys, alphaKeys);

            LaserPS = Laser.GetComponent<ParticleSystem>().main;
            LaserPS1 = Laser1.GetComponent<ParticleSystem>().main;
            LaserPS2 = Laser2.GetComponent<ParticleSystem>().main;
            ChargeSparkPS = ChargeSpark.GetComponent<ParticleSystem>().main;
            //EnergyBallPS = EnergyBall.GetComponent<ParticleSystem>().main;
            FioriturePS = Fioriture.GetComponent<ParticleSystem>().main;
            FioriturePS1 = Fioriture1.GetComponent<ParticleSystem>().main;
            OndesPS = Ondes.GetComponent<ParticleSystem>().main;
            ElicePS = Elice.GetComponent<ParticleSystem>().main;
            EliceDarkPS = EliceDark.GetComponent<ParticleSystem>().main;
            ElectricityPS = Electricity.GetComponent<ParticleSystem>().main;

            LaserPS.startColor = gradiend;
            LaserPS1.startColor = gradiend;
            LaserPS2.startColor = gradiend;
            ChargeSparkPS.startColor = gradiend;
            EnergyBall.SetColor("_EmissionColor", CurColor);
            FioriturePS.startColor = gradiend;
            FioriturePS1.startColor = gradiend;
            OndesPS.startColor = gradiend;
            ElicePS.startColor = gradiend;
            EliceDarkPS.startColor = gradiend;
            ElectricityPS.startColor = gradiend;

        }

    }

}
