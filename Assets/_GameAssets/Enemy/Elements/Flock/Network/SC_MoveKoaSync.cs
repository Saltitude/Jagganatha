﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_MoveKoaSync : NetworkBehaviour
{

    public GameObject mr_P;
    public GameObject mr_OP;
    public GameObject mr_Boss;

    Transform guide;
    [SyncVar]
    public int curboidNumber = 0;
    [SyncVar]
    public int MaxboidNumber = 0;

    public string KoaID;

    // Start is called before the first frame update
    void Start()
    {
        if (isServer)
        {
            mr_OP.GetComponent<SphereCollider>().enabled = false;
            mr_OP.GetComponent<MeshRenderer>().enabled = false;
            mr_OP.SetActive(false);
            mr_P.GetComponent<SphereCollider>().enabled = false;

            for (int i =0; i< mr_P.transform.childCount; i++)
            {
                mr_P.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
            }
        }
        else if (!isServer)
        {
            for (int i = 0; i < mr_P.transform.childCount; i++)
            {
                mr_P.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = false;
            }
            if (mr_Boss) mr_Boss.SetActive(false);
            mr_OP.SetActive(true);
        }         
    }

    public void SetPilotMeshActive()
    {
        if(isServer)
        {
            mr_P.GetComponent<SphereCollider>().enabled = true;
            for (int i = 0; i < mr_P.transform.childCount; i++)
            {
                mr_P.transform.GetChild(i).GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    //public void SetBiggerMeshBoss(float scale)
    //{
    //    if(isServer)
    //    {
    //        mr_P.transform.localScale *= scale;
    //        RpcSendVt3Scale(scale);
    //    }
    //}

    //[ClientRpc]
    //public void RpcSendVt3Scale(float scaleFactor)
    //{
    //    if (!isServer)
    //        mr_OP.transform.localScale *= scaleFactor;
    //}

    // Update is called once per frame
    void Update()
    {
        if (isServer)
            RpcSendVt3Position(gameObject, gameObject.transform.position);
    }

    /// <summary>
    ///Vector3 Transform => change la position d'un objet dans un espace 3D
    /// </summary>
    [ClientRpc]
    public void RpcSendVt3Position(GameObject Target, Vector3 vt3_Position)
    {
        if (!isServer)
            Target.transform.position = new Vector3(vt3_Position.x, 50, vt3_Position.z);       
    }

    [ClientRpc]
    public void RpcSendIntCurLife(GameObject Target, float curLife)
    {
        if (!isServer) 
            Target.transform.GetChild(1).GetComponent<SC_KoaSettingsOP>().SetKoaLife(curLife);
    }
    
    [ClientRpc]
    public void RpcSendIntBehaviorIndex(GameObject Target, int boidSettingsIndex)
    {
        if (!isServer) 
            Target.transform.GetChild(1).GetComponent<SC_KoaSettingsOP>().SetBoidSettings(boidSettingsIndex);
    }

    [ClientRpc]
    public void RpcSendIntCurState(GameObject Target, int curState)
    {
        if (!isServer)
            Target.transform.GetChild(1).GetComponent<SC_KoaSettingsOP>().SetKoaState(curState);
    }

    [ClientRpc]
    public void RpcSendStartInfo(GameObject Target, Vector3 vt3_Sensibility, int timeBeforeSpawn,string KoaID,float curLife, float maxLife,int type, bool spawnScale)
    {
        this.KoaID = KoaID;
        if (!isServer)
        {
            SC_KoaSettingsOP sc_KoaSettings = Target.transform.GetChild(1).GetComponent<SC_KoaSettingsOP>();
            sc_KoaSettings.SetSensibility(vt3_Sensibility);
            sc_KoaSettings.SetTimeBeforeSpawn(timeBeforeSpawn);
            sc_KoaSettings.SetKoaID(KoaID);
            sc_KoaSettings.SetKoaLife(curLife);
            sc_KoaSettings.SetKoamaxLife(maxLife);
            sc_KoaSettings.SetKoaType(type, spawnScale);
            
            

        }
    }

    [ClientRpc]
    public void RpcSendAnimationBool(GameObject Target,bool deploy, bool flight, bool bullet, bool laser, float speedFactor, bool chargeLaser)
    {
        if(!isServer)
        {
            SC_KoaSettingsOP sc_KoaSettings = Target.transform.GetChild(1).GetComponent<SC_KoaSettingsOP>();
            sc_KoaSettings.SetBoolAnimation(deploy, flight, bullet, laser, speedFactor, chargeLaser);
        }
     
    }


    [ClientRpc]
    public void RpcSendHideOPMesh()
    {
        if(!isServer)
        {
            mr_OP.GetComponent<MeshRenderer>().enabled = false;

        }
    }

    [ClientRpc]
    public void RpcSetNewSensitivity(GameObject Target,Vector3 sensibility)
    {
        SC_KoaSettingsOP sc_KoaSettings = Target.transform.GetChild(1).GetComponent<SC_KoaSettingsOP>();
        sc_KoaSettings.SetSensibility(sensibility);
    }


    public void InitOPKoaSettings(Vector3 sensibility, int timeBeforeSpawn, string KoaID,float curLife, float maxLife, int type, Transform guide, bool spawnScale)
    {
        if (isServer)
        {
            RpcSendStartInfo(gameObject, sensibility, timeBeforeSpawn, KoaID, curLife, maxLife, type, spawnScale);
            this.guide = guide;
        }
    }


    public void SetNewSensitivity(Vector3 sensibility)
    {
        RpcSetNewSensitivity(gameObject,sensibility);
    }
    public void SetCurLife(float curLife)
    {
        RpcSendIntCurLife(gameObject, curLife);
    }

    public void SetCurState(int curState)
    {
        if(gameObject != null)
            RpcSendIntCurState(gameObject, curState);
    }

    public void SetNewBehavior(int boidSettingsIndex)
    {
        if(gameObject != null)
            RpcSendIntBehaviorIndex(gameObject, boidSettingsIndex);
    }


    public void SetAnimationBool(bool deploy,bool flight, bool bullet,bool laser,float speedFactor, bool chargeLaser )
    {

        RpcSendAnimationBool(gameObject ,deploy, flight, bullet, laser, speedFactor, chargeLaser);
    }

    public void HideOPMesh()
    {
        RpcSendHideOPMesh();
    }
}