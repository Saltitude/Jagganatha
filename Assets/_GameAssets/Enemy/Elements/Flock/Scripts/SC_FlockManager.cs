﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Le flock Manager contient les guides que suivent une nuée et éxecute les ordres de l'EnemyManager
///  | Sur le prefab Flock
///  | Auteur : Zainix
/// </summary>
public class SC_FlockManager : MonoBehaviour
{

    //---------------------------------------------------------------------//
    //---------------------------- VARIABLES ------------------------------//
    //---------------------------------------------------------------------//
    #region Variables

    //-----------PREFAB
    [SerializeField]
    GameObject _KoaPrefab;
    [SerializeField]
    GameObject _GuidePrefab;

    BezierSolution.BezierSpline[] _splineTab;
    BezierSolution.BezierSpline _curSpline;

    GameObject _Player;

    public SC_MoveKoaSync moveKoaSync;

    BoidSettings[][] _BoidSettings;

    BoidSettings[] spawnSettings;
    BoidSettings[] roamSettings;
    BoidSettings[] attackSettings;
    BoidSettings[] destructionSettings;
    BoidSettings[] getAwaySettings;
    BoidSettings[] hitReactionSettings;
    BoidSettings[] BossFlightSettings;

    int curSettingsIndex;
    public Animator KoaMainAnimator;
    public Animator KoaEmissiveAnimator;
    public Animator BossAuxAnimator;
    public GameObject BossAux;



    BoidSettings _curBoidSetting; //Contient le settings actuel

    FlockSettings flockSettings; //Flocksettings de la nuée (défini a la création par le waveSettings)
    GameObject _KoaManager; //Stock le Koa de la nuée
    public SC_KoaManager _SCKoaManager; //Stock le script KoaManager du Koa
    Transform _mainGuide; //Guide général que suit toujours la nuée (correspond au flock (this) mais pour des pb de lisibilité le Transform est stocké dans une varible Main Guide



    BezierSolution.BezierWalkerWithSpeed bezierWalkerSpeed;
    BezierSolution.BezierWalkerWithTime bezierWalkerTime;

    //SC_PathBehavior pathBehavior;
    SC_FlockWeaponManager flockWeaponManager;

    bool inAttack;
    bool isActive;
    bool isSpawning;
    bool startSpawning = true;
    bool bossDying = false;


    Vector3 spawnPos;

    Quaternion flockInitialRot;

    [SerializeField]
    GameObject spawnPosTargetDummy;
    //---------------------------------------------      MultiGuide Variables  (Split)   ----------------------------------------------------------//

    [HideInInspector]
    public bool _splited = false; //Booléen d'état : nuée split 
    List<Transform> _GuideList; //Permet de stocké la liste des guides lors du split
    List<Vector3> _curCurveDistanceList; //Permet de stocké la distance sur les courbes pour chaque guide lors du split

    Vector3Int sensitivity;
    //--------------------------------------------------------------------------------------------------------------------------------------------//


    [HideInInspector]
    public bool _merged = false;  //Booléen d'état : nuée fusionnée avec une/des autre(s)

    float startAttackTimer = 0;

    public enum PathType
    {
        Spawn = 0,
        Roam = 1,
        AttackPlayer = 2,
        Death = 3,
        Flight = 4,
        ReactionHit = 5,
        bossFlight = 6

    }

    float flightTimer = 0;
    float reactionTimer = 0;
    bool reactionHit = false;

    float delayBeforeEndReaction = 1f;
    float timeBeforeEndReaction = 0f;

    PathType curtype;

    bool laserBoss = false;
    #endregion
    //---------------------------------------------------------------------//


    //---------------------------------------------------------------------//
    //------------------------------- INIT --------------------------------//
    //---------------------------------------------------------------------//
    #region Init
    void Awake()
    {
        bezierWalkerSpeed = GetComponent<BezierSolution.BezierWalkerWithSpeed>();
        bezierWalkerTime = GetComponent<BezierSolution.BezierWalkerWithTime>();
        flockWeaponManager = GetComponent<SC_FlockWeaponManager>();
    }


    /// <summary>
    /// Initialisation du Flock
    /// </summary>
    public void InitializeFlock(FlockSettings newFlockSettings, BezierSolution.BezierSpline spawnSpline, Vector3Int sensitivity)
    {
        flockSettings = newFlockSettings;
        flockInitialRot = transform.rotation;

        inAttack = false;
        isSpawning = true;
        _Player = GameObject.FindGameObjectWithTag("Player");

        transform.position = spawnSpline.GetPoint(0);
        bezierWalkerTime.SetNewSpline(spawnSpline);
        bezierWalkerTime.NormalizedT = 0;
        bezierWalkerTime.travelTime = flockSettings.spawnTimer;

        _mainGuide = gameObject.transform; //Main guide prends la valeur de this (CF : Variable _mainGuide)

        _GuideList = new List<Transform>();//Instanciation de la guide list
        _curCurveDistanceList = new List<Vector3>(); // Instanciation de la list de distance sur les courbes pour chaque guide

        int boidSettingsLenght = 6;
        if (flockSettings.attackType == FlockSettings.FlockType.Boss) boidSettingsLenght = 7;

        _BoidSettings = new BoidSettings[boidSettingsLenght][];

        spawnSettings = flockSettings.spawnSettings;
        roamSettings = flockSettings.roamSettings;
        attackSettings = flockSettings.attackSettings;
        destructionSettings = flockSettings.destructionSettings;
        getAwaySettings = flockSettings.getAwaySettings;
        hitReactionSettings = flockSettings.hitReactionSettings;
        BossFlightSettings = flockSettings.BossFlightSettings;


        _BoidSettings[0] = spawnSettings;
        _BoidSettings[1] = roamSettings;
        _BoidSettings[2] = attackSettings;
        _BoidSettings[3] = destructionSettings;
        _BoidSettings[4] = getAwaySettings;
        _BoidSettings[5] = hitReactionSettings;
        if (flockSettings.attackType == FlockSettings.FlockType.Boss)
            _BoidSettings[6] = BossFlightSettings;

        _KoaManager = Instantiate(_KoaPrefab, transform);//Instantiate Koa
        _SCKoaManager = _KoaManager.GetComponent<SC_KoaManager>(); //Récupère le Koa manager du koa instancié
        _SCKoaManager.Initialize(_mainGuide, flockSettings.boidSpawn, spawnSettings[0], newFlockSettings, sensitivity);//Initialise le Koa | paramètre : Guide a suivre <> Nombre de Boids a spawn <> Comportement des boids voulu
        flockWeaponManager.Initialize(flockSettings, KoaMainAnimator, KoaEmissiveAnimator);

        _splineTab = new BezierSolution.BezierSpline[flockSettings.splines.Length];

        for (int i = 0; i < flockSettings.splines.Length; i++)
        {
            if (flockSettings.splines[i] != null)
            {
                _splineTab[i] = Instantiate(flockSettings.splines[i]);
            }
        }

        //Raccourci l'attente avant le premier tir des lasers
        if (flockSettings.timeBeforeFirstAttack == -1) startAttackTimer = 0;
        else
        {
            startAttackTimer = flockSettings.timeBetweenAttacks - flockSettings.timeBeforeFirstAttack;
        }
        if(BossAux)
        BossAuxAnimator = BossAux.GetComponent<Animator>();


        Invoke("ActivateFlock", flockSettings.spawnTimer);
    }
    #endregion
    //---------------------------------------------------------------------//


    //---------------------------------------------------------------------//
    //----------------------------- UPDATE  -------------------------------//
    //---------------------------------------------------------------------//
    #region Update
    void Update()
    {

        if (isActive && _curBoidSetting != null)
            transform.Rotate(new Vector3(_curBoidSetting.axisRotationSpeed.x, _curBoidSetting.axisRotationSpeed.y, _curBoidSetting.axisRotationSpeed.z));

        if (isSpawning && !isActive)
        {
            bezierWalkerTime.Execute(Time.deltaTime);
       
        }

        if (isActive && isSpawning)
        {

            float speed = 3f;

            if (startSpawning)
            {
                int rndRangePilote = Random.Range(180, 200);
                int rndRangeAngle = 25;

                GameObject targetDummy = Instantiate(spawnPosTargetDummy, transform);//Instantiate Koa

                targetDummy.transform.position = _Player.transform.position;
                targetDummy.transform.LookAt(transform.position);

                transform.transform.Rotate(0, Random.Range(-rndRangeAngle, rndRangeAngle), 0);

                //targetDummy.transform.rotation.SetEulerRotation(transform.rotation.eulerAngles.x + Random.Range(-30, 30), transform.rotation.eulerAngles.y + Random.Range(-30, 30), transform.rotation.eulerAngles.z + Random.Range(-30, 30));
                targetDummy.transform.Translate(Vector3.forward * rndRangePilote);
                spawnPos = new Vector3(targetDummy.transform.position.x, targetDummy.transform.position.y + 80, targetDummy.transform.position.z);
                startSpawning = false;
            }
            transform.position = Vector3.MoveTowards(transform.position, spawnPos, speed);

            if (Vector3.Distance(transform.position, spawnPos) < 1)
            {
                for (int i = 0; i < _BoidSettings.Length; i++)
                {


                    if (_splineTab[i] != null)
                    {
                        _splineTab[i].transform.position = transform.position;
                        if (i != 4 && i != 6)
                            _splineTab[i].transform.rotation = Random.rotation;

                        else
                        {
                            _splineTab[i].transform.position = new Vector3(0, 50, 0);
                        }
                    }
                }
                isSpawning = false;
                StartNewPath(PathType.Roam);


            }
        }


        if (isActive && !isSpawning && curtype != PathType.Death)
        {

            if (curtype != PathType.bossFlight && !bossDying)
            {
                AttackUpdate();
                ReactionUpdate();
            }
            //Si le flock est split, déplace les guides
            if (_splited)
                MultiGuideMovement();


            //Si le flock n'est pas fusionné, déplace le main guide selon la spline actuel      
            if (curtype != PathType.ReactionHit && curtype != PathType.AttackPlayer)
                bezierWalkerSpeed.Execute(Time.deltaTime);


        }

        if (isActive && curtype != PathType.Death && moveKoaSync != null)
        {
            moveKoaSync.SetAnimationBool(KoaMainAnimator.GetBool("Deploy"), KoaMainAnimator.GetBool("Flight"), KoaMainAnimator.GetBool("Bullet"), KoaMainAnimator.GetBool("Laser"), KoaMainAnimator.GetFloat("SpeedFactor"), KoaEmissiveAnimator.GetBool("LaserCharge"));
        }

    }

    /// <summary>
    /// Guides movement when the flock is Split
    /// </summary>
    void MultiGuideMovement()
    {

        //Set every Guide position
        for (int i = 0; i < _GuideList.Count; i++)
        {
            //Init new translate value
            float valueX = 0;
            float valueY = 0;
            float valueZ = 0;
            //Curve offset by guide
            float fguideOffset = (1 / _GuideList.Count) * (i + 1);
            Vector3 v3guideOffset = new Vector3(fguideOffset, fguideOffset, fguideOffset);

            //Add new dt to curv distance (x oscilliation speed)
            _curCurveDistanceList[i] += Time.deltaTime * _curBoidSetting.frequence;
            _curCurveDistanceList[i] += v3guideOffset;

            //curv distance in relation of guide number



            //Keep current curve distance values between 0 - 1
            if (_curCurveDistanceList[i].x > 1)
            {
                _curCurveDistanceList[i] = new Vector3(_curCurveDistanceList[i].x - 1, _curCurveDistanceList[i].y, _curCurveDistanceList[i].z);
            }
            if (_curCurveDistanceList[i].y > 1)
            {
                _curCurveDistanceList[i] = new Vector3(_curCurveDistanceList[i].x, _curCurveDistanceList[i].y - 1, _curCurveDistanceList[i].z);
            }
            if (_curCurveDistanceList[i].z > 1)
            {
                _curCurveDistanceList[i] = new Vector3(_curCurveDistanceList[i].x, _curCurveDistanceList[i].y, _curCurveDistanceList[i].z - 1);
            }


            //Get Z and Y translate value on the curve
            valueX = _curBoidSetting.curveX.Evaluate(_curCurveDistanceList[i].x) * _curBoidSetting.amplitude.x;
            valueY = _curBoidSetting.curveY.Evaluate(_curCurveDistanceList[i].y) * _curBoidSetting.amplitude.y;
            valueZ = _curBoidSetting.curveZ.Evaluate(_curCurveDistanceList[i].z) * _curBoidSetting.amplitude.z;

            if (_curBoidSetting.invert && i % 2 == 1)
            {
                valueX -= (valueX * 2) * _curBoidSetting.invertAxis.x;
                valueY -= (valueY * 2) * _curBoidSetting.invertAxis.y;
                valueZ -= (valueZ * 2) * _curBoidSetting.invertAxis.z;

            }

            //Add the offset value
            _GuideList[i].transform.localPosition = new Vector3(valueX, valueY, valueZ);
        }


    }


    void AttackUpdate()
    {
        if (flockSettings.attackType != FlockSettings.FlockType.none && curtype != PathType.Flight && curtype != PathType.ReactionHit)
        {
            if (inAttack == false) startAttackTimer += Time.deltaTime;

            if (startAttackTimer >= flockSettings.timeBetweenAttacks && inAttack == false)
            {
                inAttack = true;
                StartNewPath(PathType.AttackPlayer);
                startAttackTimer = 0;
            }
        }

        transform.LookAt(_Player.transform);

        if (KoaMainAnimator != null)
            KoaMainAnimator.transform.LookAt(_Player.transform);
        if (BossAux != null)
            BossAux.transform.LookAt(_Player.transform);

    }

    void ReactionUpdate()
    {

        if (curtype == PathType.Flight)
        {
            flightTimer += Time.deltaTime;
            if (flightTimer >= flockSettings.flightDuration)
            {
                for (int i = 0; i < _splineTab.Length; i++)
                {
                    if (_splineTab[i] != null)
                    {
                        if (i != 4 && i != 6)
                            _splineTab[i].transform.position = transform.position;
                    }
                }
                KoaMainAnimator.SetBool("Flight", false);
                KoaEmissiveAnimator.SetBool("Flight", false);
                StartNewPath(PathType.Roam);
                flightTimer = 0;
            }
        }

        if (reactionHit)
        {
            timeBeforeEndReaction += Time.deltaTime;
            if (timeBeforeEndReaction >= delayBeforeEndReaction)
            {
                reactionHit = false;

            }
            reactionTimer += Time.deltaTime;

            if (reactionTimer >= flockSettings.hitReactionDelay)
            {
                reactionHit = false;
                reactionTimer = 0;
                flockWeaponManager.FireSuperBullet();
                if (flockSettings.attackType != FlockSettings.FlockType.Laser)
                    EndReaction();

            }
        }

        if (!reactionHit)
        {

            if (reactionTimer > 0)
            {

                reactionTimer -= Time.deltaTime;

                if (KoaMainAnimator != null)
                    KoaMainAnimator.SetFloat("SpeedFactor", -1);

                if (KoaEmissiveAnimator != null)
                    KoaEmissiveAnimator.SetFloat("SpeedFactor", -1);

            }

            if (reactionTimer < 0)
            {
                EndReaction();
            }

        }
    }


    #endregion
    //---------------------------------------------------------------------//



    //---------------------------------------------------------------------//
    //---------------------------- UTILITIES ------------------------------//
    //---------------------------------------------------------------------//
    #region Utilities

    void StartNewPath(PathType pathType)
    {
        _SCKoaManager.ChangeKoaState((int)pathType);




        switch (pathType)
        {
            case PathType.Roam:

                if (flockSettings.attackType == FlockSettings.FlockType.Boss)
                {
                    BossAuxAnimator.SetBool("Cube", false);
                    BossAuxAnimator.SetBool("FullShield", false);
                    BossAuxAnimator.SetBool("Spike", false);
                    BossAuxAnimator.SetBool("Pos", true);

                }

                break;

            case PathType.AttackPlayer:
                KoaMainAnimator.SetBool("Deploy", false);
                KoaEmissiveAnimator.SetBool("Deploy", false);

                KoaMainAnimator.SetBool("Flight", false);
                KoaEmissiveAnimator.SetBool("Flight", false);


                if (flockSettings.attackType == FlockSettings.FlockType.Boss)
                {
                    BossAuxAnimator.SetBool("FullShield", false);
                    BossAuxAnimator.SetBool("Spike", false);
                    BossAuxAnimator.SetBool("Pos", false);
                    BossAuxAnimator.SetBool("Cube", true);


                    laserBoss = !laserBoss;
                    flockWeaponManager.StartFire(true, laserBoss);
                }
                else
                    flockWeaponManager.StartFire();

                break;


            case PathType.ReactionHit:

                if (reactionHit != true)
                {
                    KoaMainAnimator.SetBool("Deploy", true);
                    KoaEmissiveAnimator.SetBool("Deploy", true);
                    KoaMainAnimator.SetFloat("SpeedFactor", 1);
                    KoaEmissiveAnimator.SetFloat("SpeedFactor", 1);
                    reactionHit = true;
                }
                timeBeforeEndReaction = 0;

                break;

            case PathType.Flight:

                KoaMainAnimator.SetBool("Flight", true);
                KoaEmissiveAnimator.SetBool("Flight", true);

                break;
            case PathType.Death:

                flockWeaponManager.SetBoolCanFire(false);

                KoaMainAnimator.SetBool("Deploy", false);
                KoaEmissiveAnimator.SetBool("Deploy", false);

                KoaMainAnimator.SetBool("Flight", false);
                KoaEmissiveAnimator.SetBool("Flight", false);
                KoaMainAnimator.SetBool("Deploy", true);
                KoaEmissiveAnimator.SetBool("Deploy", true);
                KoaMainAnimator.SetFloat("SpeedFactor", 1);
                KoaEmissiveAnimator.SetFloat("SpeedFactor", 1);
                break;

            case PathType.bossFlight:

                if (curtype != PathType.bossFlight)
                {

                    flockWeaponManager.SetBoolCanFire(false);

                    KoaMainAnimator.SetBool("Deploy", false);
                    KoaEmissiveAnimator.SetBool("Deploy", false);

                    KoaMainAnimator.SetBool("Flight", false);
                    KoaEmissiveAnimator.SetBool("Flight", false);

                    KoaMainAnimator.SetBool("Flight", true);
                    KoaEmissiveAnimator.SetBool("Flight", true);

                    KoaMainAnimator.SetFloat("SpeedFactor", 1);
                    KoaEmissiveAnimator.SetFloat("SpeedFactor", 1);


                    BossAuxAnimator.SetBool("Spike", false);
                    BossAuxAnimator.SetBool("Pos", false);
                    BossAuxAnimator.SetBool("Cube", false);
                    BossAuxAnimator.SetBool("FullShield", true);

                }


                break;
        }
        curtype = pathType;
        StartNewBehavior((int)pathType);

    }

    void DestroyKoa()
    {
        _SCKoaManager.AnimDestroy();
    }

    void ActivateFlock()
    {
        isActive = true;

        _SCKoaManager.ActivateKoa();

    }

    public void EndReaction()
    {
        KoaEmissiveAnimator.SetBool("Deploy", false);
        KoaEmissiveAnimator.SetBool("Flight", false);
        KoaEmissiveAnimator.SetBool("Laser", false);
        KoaEmissiveAnimator.SetBool("LaserCharge", false);
        KoaEmissiveAnimator.SetBool("Bullet", false);
        KoaMainAnimator.SetBool("Deploy", false);
        KoaMainAnimator.SetBool("Flight", false);
        KoaMainAnimator.SetBool("Laser", false);
        KoaMainAnimator.SetBool("Bullet", false);
        reactionHit = false;
        reactionTimer = 0;
        timeBeforeEndReaction = 0;

        StartNewPath(PathType.Roam);

    }


    public void StartNewBehavior(int behaviorIndex)
    {
        transform.rotation = flockInitialRot;

        StopAllCoroutines();
        curSettingsIndex = 0;
        _curSpline = _splineTab[behaviorIndex];
        if (curtype != PathType.bossFlight)
        {
            bezierWalkerSpeed.SetNewSpline(_curSpline);
        }
        else
        {
            Invoke("DestroyKoa", flockSettings.fleeingTime);
            _curSpline.transform.position = transform.position;
            bezierWalkerSpeed.SetNewSpline(_curSpline);
            bezierWalkerSpeed.travelMode = BezierSolution.TravelMode.Once;
            
        }

        if (curtype == PathType.AttackPlayer && flockSettings.attackType == FlockSettings.FlockType.Boss)
        {
            BoidSettings[] settings;
            if (laserBoss)
            {
                settings = new BoidSettings[1];
                settings[0] = attackSettings[0];
            }
            else
            {
                settings = new BoidSettings[2];
                settings[0] = attackSettings[1];
                settings[1] = attackSettings[2];
            }
            StartCoroutine(SwitchSettings(settings));
        }
        else
            StartCoroutine(SwitchSettings(_BoidSettings[behaviorIndex]));

    }

    IEnumerator SwitchSettings(BoidSettings[] settings)
    {
        while (true)
        {

      
            if (curtype == PathType.Death)
            {
                bezierWalkerSpeed.speed = 0;
                yield return new WaitForSeconds(1.3f);
            }

            _curBoidSetting = settings[curSettingsIndex];

            if(curtype != PathType.bossFlight)
            {

                int rnd = Random.Range(0, 2);
                if (rnd == 0)
                    bezierWalkerSpeed.speed = _curBoidSetting.speedOnSpline;
                else
                    bezierWalkerSpeed.speed = -_curBoidSetting.speedOnSpline;
            }
            else
            {
                bezierWalkerSpeed.speed = 150;
            }

            Reassemble();
            if (_curBoidSetting.split)
            {
                SplitDivision(_curBoidSetting.splitNumber);
            }
            _SCKoaManager.SetBehavior(_curBoidSetting);
            //https://www.youtube.com/watch?v=bOZT-UpRA2Y

            if (settings.Length == 1)
            {
                StopAllCoroutines();
                break;
            }

            yield return new WaitForSeconds(_curBoidSetting.settingDuration);

            curSettingsIndex++;
            if (curSettingsIndex >= settings.Length)
                curSettingsIndex = 0;

        }

    }


    /// <summary>
    /// Start the Flock split [Param : (int)Split Number]
    /// </summary>
    /// <param name="splitNumber"></param>
    /// 
    void SplitDivision(int splitNumber)
    {
        //Register old guides to destroy after creating new ones
        List<Transform> _oldGuideList = new List<Transform>();
        foreach (Transform guide in _GuideList)
        {
            if (!GameObject.Equals(guide, transform))
            {
                _oldGuideList.Add(guide);
            }
        }

        //Set splited to true
        _splited = true;

        //Clear current guide list
        _GuideList.Clear();

        //Clear Curve distance value List for multi guides
        _curCurveDistanceList.Clear();

        //Set new values link to split number
        for (int i = 0; i < splitNumber; i++)
        {
            //Create new guides 
            Transform curGuide = Instantiate<GameObject>(_GuidePrefab).transform;
            //Add them to guide list
            _GuideList.Add(curGuide);
            curGuide.SetParent(this.transform);

            //Calculate Offset for Curve distance
            float offset = (1f / splitNumber) * (i);
            //Register curve offset distance to Curve distance list
            _curCurveDistanceList.Add(new Vector3(offset, offset, offset));


        }

        _SCKoaManager.Split(_GuideList);

        //Destroy old useless guides
        foreach (Transform guide in _oldGuideList)
        {
            Destroy(guide.gameObject);
        }

    }

    void Reassemble()
    {

        foreach (Transform guide in _GuideList)
        {
            if (!GameObject.Equals(guide, transform))
                Destroy(guide.gameObject);
        }
        //Set splited to false
        _splited = false;

        //Clear current guide list
        _GuideList.Clear();

        //Clear Curve distance value List for multi guides
        _curCurveDistanceList.Clear();

        //Add main guide to guide list
        _GuideList.Add(_mainGuide);

        _SCKoaManager.Split(_GuideList);
    }

    public void AnimDestroy()
    {
        StartNewPath(PathType.Death);
    }

    public void DestroyFlock()
    {
        GetComponent<SC_FlockWeaponManager>().DestroyFx();
        SC_WaveManager.Instance.FlockDestroyed(this.gameObject);
        Destroy(this.gameObject);
    }

    public void ReactionFlock(PathType pathType)
    {
        if (pathType == PathType.bossFlight)
        {
            if (curtype != PathType.bossFlight)
            StartNewPath(pathType);
        }
        else if (curtype != PathType.AttackPlayer && curtype != PathType.Death)
        {
            StartNewPath(pathType);
        }

    }

    public void StopAttack()
    {
        bossDying = true;
    }

    public void EndAttack()
    {

        inAttack = false;
        StartNewPath(PathType.Roam);

        if (KoaMainAnimator != null)
            KoaMainAnimator.SetBool("Laser", false);

        if (KoaMainAnimator != null)
            KoaMainAnimator.SetBool("Bullet", false);

        if (KoaEmissiveAnimator != null)
            KoaEmissiveAnimator.SetBool("Bullet", false);

    }


    Vector3 GetRandomSpawnPosition()
    {
        var radius = 210;
        float x = Random.Range(0f, 1f);
        float y = 1 - x;

        int rndNeg1 = Random.Range(0, 2);
        int rndNeg2 = Random.Range(0, 2);

        if (rndNeg1 == 1) x = -x;
        if (rndNeg2 == 1) y = -y;


        return new Vector3(x * radius, 80, y * radius);
    }

    #endregion
    //---------------------------------------------------------------------//

}
