﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_KoaSpawn : MonoBehaviour
{
    #region Singleton

    private static SC_KoaSpawn _instance;
    public static SC_KoaSpawn Instance { get { return _instance; } }

    #endregion





    [SerializeField]
    GameObject koaPrefab;

    [SerializeField]
    GameObject containerPrefab;

    GameObject Player;

    GameObject[,,,] koaTab2;

    public int nb_totalFlock;

    BezierSolution.BezierSpline[] splineSpawn;

    int indexSpawn = 0;
    float fallSpeed = 1200;

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
        splineSpawn = SC_SpawnInfo.Instance.GetBezierSplines();
        Player = GameObject.FindGameObjectWithTag("Player");
        
    }

    public void KoaCountMaster()
    {
        for (int i = 0; i < koaTab2.GetLength(0); i++)
        {
            for (int j = 0; j < koaTab2.GetLength(1); j++)
            {
                for (int k = 0; k < koaTab2.GetLength(2); k++)
                {
                    for (int l = 0; l < koaTab2.GetLength(3); l++)
                    {
                        if (koaTab2 [i, j, k, l] != null)
                        {
                            nb_totalFlock += 1;
                        }
                        
                    }
                }
            }
        }
    }

    public void InitNewPhase(PhaseSettings newPhaseSettings)
    {
        int nbMaxFlock = 0;
        int nbSpawn = 8;
        int nbWaves = newPhaseSettings.waves.Length;

        GameObject container = Instantiate(containerPrefab);

        for (int i = 0; i < nbWaves; i++)
        {
            int nbFlock = newPhaseSettings.waves[i].initialSpawnFlock.Length;
            if (nbFlock < newPhaseSettings.waves[i].backupSpawnFlock.Length) nbFlock = newPhaseSettings.waves[i].backupSpawnFlock.Length;


            if (nbFlock > nbMaxFlock)
            {
                nbMaxFlock = nbFlock;
            }
        }

        koaTab2 = new GameObject[nbWaves, 2, nbMaxFlock, nbSpawn];

        for (int i = 0; i < nbWaves; i++)
        {

            WaveSettings curWave = newPhaseSettings.waves[i];

            for (int j = 0; j < curWave.initialSpawnFlock.Length; j++)
            {

                GameObject curKoa;           

                int pos = curWave.initialSpawnPosition[j];
                //int orientedPos = (pos + (int)SC_PhaseManager.Instance.curPhaseSettings.WavesOrientation[SC_PhaseManager.Instance.curWaveIndex]) % 8;
                int orientedPos = (pos + (int)SC_PhaseManager.Instance.curPhaseSettings.WavesOrientation[i]) % 8;

                koaTab2[i,0,j, orientedPos] = Instantiate(koaPrefab);
                curKoa = koaTab2[i, 0, j, orientedPos];
                DisplaceKoaOnSpawn(curKoa, orientedPos);

                StartCoroutine(GoTargetPos(i, 0, j, pos, 700, 8f));
                curKoa.transform.SetParent(container.transform);


            }

            for (int k = 0; k < curWave.backupSpawnFlock.Length; k++)
            {

                GameObject curKoa;

                int pos = curWave.backupSpawnPosition[k];
                //int orientedPos = (pos + (int)SC_PhaseManager.Instance.curPhaseSettings.WavesOrientation[SC_PhaseManager.Instance.curWaveIndex]) % 8;
                int orientedPos = (pos + (int)SC_PhaseManager.Instance.curPhaseSettings.WavesOrientation[i]) % 8;

                koaTab2[i, 1, k, orientedPos] = Instantiate(koaPrefab);
                curKoa = koaTab2[i, 1, k, orientedPos];
                DisplaceKoaOnSpawn(curKoa, orientedPos);

                StartCoroutine(GoTargetPos(i, 1, k, pos, 700, 8f));
                curKoa.transform.SetParent(container.transform);

            }
        }

        KoaCountMaster();

    }

 


    void DisplaceKoaOnSpawn(GameObject koa, int spawnPoint)
    {
        int rndx = Random.Range(-200, -150);
        int rndy = Random.Range(100, 800);
        int rndz = Random.Range(-400,400);

        float rndSphereScale = Random.Range(0.6f, 1.6f);

        koa.transform.localScale = new Vector3(koa.transform.localScale.x*rndSphereScale, koa.transform.localScale.y*rndSphereScale, koa.transform.localScale.z*rndSphereScale);

        float rndscale = Random.Range(1.0f, 2.0f);
        for (int i = 0; i<koa.transform.childCount;i++)
        {
            koa.transform.GetChild(i).transform.localScale = new Vector3(1,1, koa.transform.GetChild(i).transform.localScale.z*rndscale);
        }

        koa.transform.position = splineSpawn[spawnPoint].GetPoint(1);
        koa.transform.LookAt(Player.transform);
        koa.transform.Translate(new Vector3(-1200 + rndx , rndy, rndz), Space.Self);
        koa.transform.LookAt(Player.transform);
    
    }

    public IEnumerator SpawnCoro(int wi, int backup, int flockrank, int spawnPos )
    {

        int pos = spawnPos;
        int orientedPos = (pos + (int)SC_PhaseManager.Instance.curPhaseSettings.WavesOrientation[wi]) % 8;

        GameObject curKoa = koaTab2[wi,backup,flockrank, orientedPos];

        //Debug.Log("SpawnCoro - orientedPos = " + orientedPos);
        //Debug.Log("SpawnCoro - curKoa = " + curKoa);

        while (curKoa.transform.position.y > -150)
        {
            curKoa.transform.Translate(new Vector3(0, -fallSpeed * Time.deltaTime, 0));
            yield return 0;
        }
        yield return 0;
    }


    public IEnumerator GoTargetPos(int wi, int backup, int flockrank, int spawnPos, int minDist, float travelTime)
    {

        int pos = spawnPos;
        int orientedPos = (pos + (int)SC_PhaseManager.Instance.curPhaseSettings.WavesOrientation[wi]) % 8;

        GameObject curKoa = koaTab2[wi, backup, flockrank, orientedPos];

        //Debug.Log("GoTargetPos - orientedPos = " + orientedPos);
        //Debug.Log("GoTargetPos - curKoa = " + curKoa);


        float curDist = Vector3.Distance(curKoa.transform.position, Player.transform.position);
        float distanceToTravel = curDist - minDist;
        float distancePerSec = distanceToTravel/ travelTime;

        float t = 0;
        float rate = 1 / travelTime;

        while (t < 1)
        {

            t += Time.deltaTime * rate;

            if (Vector3.Distance(curKoa.transform.position, Player.transform.position) > minDist)
                curKoa.transform.Translate(Vector3.forward * Time.deltaTime * distancePerSec);

            
            yield return 0;

        }
    }
}
