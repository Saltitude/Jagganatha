﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SC_JoystickMove : MonoBehaviour, IF_BreakdownSystem
{

    //Breakdown Infos
    [Header("Breakdown Infos")]
    [SerializeField]
    bool b_InBreakdown = false;
    [SerializeField]
    bool b_BreakEngine = false;

    //Coroutines Infos
    [Header("Smooth Coroutine Infos")]
    [SerializeField]
    bool b_UseCoroutine = false;
    [Range(0, 2)]
    public float f_Duration = 0.5f;
    public enum Dir { None, Left, Right, Off }
    public Dir CurDir = Dir.None;
    public Dir TargetDir = Dir.None;
    public Dir CoroDir = Dir.Off;
    [SerializeField]
    AnimationCurve Acceleration;

    //Rotation Horizontale
    [Header("Horizontal Rotation Settings")]
    [SerializeField]
    float f_RotationSpeedZ = 1.0f;
    [SerializeField]
    float f_LerpRotZ = 1f;  
    public enum RotationMode { TSR, Torque, Normalize, Higher, Clamp }
    public RotationMode TypeRotationZ;
    float f_TransImpulseZ;
    float f_TorqueImpulseZ;
    Quaternion TargetRotY;

    //Rotation Verticale
    [Header("Vertical Rotation Settings")]
    [SerializeField]
    bool b_InvertAxe = false;  
    [SerializeField]
    Transform TargetTRS;
    [Range(0.0f, 1.0f)]
    public float f_RotationSpeedX = 0.5f;
    [Range(0.0f, 1.0f)]
    public float f_LerpRotX = 1f;
    [Range(0.0f, 0.3f)]
    public float f_MaxRotUpX;
    float f_ImpulseX;
    Quaternion xQuaternion; 

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!b_InBreakdown && !b_BreakEngine)
            Move();
    }

    void Move()
    {

        #region Rotation Verticale

        f_ImpulseX = Input.GetAxis("Vertical") * f_RotationSpeedX;

        if (f_ImpulseX != 0)
        {

            if (!b_InvertAxe)
            {

                xQuaternion = Quaternion.AngleAxis(f_ImpulseX, Vector3.left);

                if (f_ImpulseX > 0 && TargetTRS.localRotation.x > -f_MaxRotUpX)
                    TargetTRS.localRotation *= Quaternion.Lerp(TargetTRS.localRotation, xQuaternion, f_LerpRotX);

                if (f_ImpulseX < 0 && TargetTRS.localRotation.x < 0)
                    TargetTRS.localRotation *= Quaternion.Lerp(TargetTRS.localRotation, xQuaternion, f_LerpRotX);

            }

            else if (b_InvertAxe)
            {

                xQuaternion = Quaternion.AngleAxis(-f_ImpulseX, Vector3.left);

                if (f_ImpulseX > 0 && TargetTRS.localRotation.x < 0)
                    TargetTRS.localRotation *= Quaternion.Lerp(TargetTRS.localRotation, xQuaternion, f_LerpRotX);

                if (f_ImpulseX < 0 && TargetTRS.localRotation.x > -f_MaxRotUpX)
                    TargetTRS.localRotation *= Quaternion.Lerp(TargetTRS.localRotation, xQuaternion, f_LerpRotX);

            }

        }

        #endregion

        #region Rotation Horizontale

        f_TorqueImpulseZ = Input.GetAxis("Torque") * f_RotationSpeedZ;
        f_TransImpulseZ = Input.GetAxis("Horizontal") * f_RotationSpeedZ;

        if (f_TorqueImpulseZ != 0 || f_TransImpulseZ != 0)
        {

            Quaternion zQuaternion = new Quaternion();
            float MixImpulseZ;
            float CurImpulse = 0;

            switch (TypeRotationZ)
            {

                case RotationMode.TSR:
                    zQuaternion = Quaternion.AngleAxis(f_TransImpulseZ, Vector3.up);
                    CurImpulse = f_TransImpulseZ;
                    break;

                case RotationMode.Torque:
                    zQuaternion = Quaternion.AngleAxis(f_TorqueImpulseZ, Vector3.up);
                    CurImpulse = f_TorqueImpulseZ;
                    break;

                case RotationMode.Higher:
                    float absTorque = Mathf.Abs(f_TorqueImpulseZ);
                    float absTrans = Mathf.Abs(f_TransImpulseZ);
                    if (absTorque >= absTrans)
                    {
                        zQuaternion = Quaternion.AngleAxis(f_TorqueImpulseZ, Vector3.up);
                        CurImpulse = f_TorqueImpulseZ;
                    }
                    else
                    {
                        zQuaternion = Quaternion.AngleAxis(f_TransImpulseZ, Vector3.up);
                        CurImpulse = f_TransImpulseZ;
                    }                     
                    break;

                case RotationMode.Normalize:
                    MixImpulseZ = (Input.GetAxis("Rotation") + Input.GetAxis("Horizontal")) / 2 * f_RotationSpeedZ;
                    zQuaternion = Quaternion.AngleAxis(MixImpulseZ, Vector3.up);
                    CurImpulse = MixImpulseZ;
                    break;

                case RotationMode.Clamp:
                    MixImpulseZ = Input.GetAxis("Rotation") + Input.GetAxis("Horizontal");
                    if (MixImpulseZ > 1)
                        MixImpulseZ = 1;
                    MixImpulseZ *= f_RotationSpeedZ;
                    zQuaternion = Quaternion.AngleAxis(MixImpulseZ, Vector3.up);
                    CurImpulse = MixImpulseZ;
                    break;

                default:
                    break;

            }

            if (CurImpulse > 0)
                TargetDir = Dir.Right;
            else if (CurImpulse < 0)
                TargetDir = Dir.Left;

            //transform.rotation *= Quaternion.Slerp(transform.rotation, zQuaternion, f_LerpRotZ);
            TargetRotY = this.transform.rotation * zQuaternion;

            if (b_UseCoroutine && CurDir != TargetDir && CoroDir != TargetDir)
                CheckDir();
            else if (!b_UseCoroutine || (CoroDir == Dir.Off && CurDir == TargetDir))
                transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotY, f_LerpRotZ);

        }
        else
        {
            TargetDir = Dir.None;
            TargetRotY = this.transform.rotation;
            if (b_UseCoroutine && CurDir != TargetDir && CoroDir != TargetDir)
                CheckDir();
        }

        #endregion

    }

    void CheckDir()
    {
        StopAllCoroutines();
        if (TargetDir == Dir.None)
            StartCoroutine(GoTargetRot(f_Duration, Dir.None));
        else if (CurDir == Dir.None)
            StartCoroutine(GoTargetRot(f_Duration, TargetDir));
        else
            StartCoroutine(GoTargetRot(f_Duration*2, TargetDir));
    }

    IEnumerator GoTargetRot(float Duration, Dir ToDir)
    {

        CoroDir = ToDir;

        float t = 0;
        float rate = 1 / Duration;

        Quaternion StartRot = transform.rotation;

        while (t < 1)
        {

            t += Time.deltaTime * rate;
            float Lerp = Acceleration.Evaluate(t); 

            transform.rotation = Quaternion.Slerp(StartRot, TargetRotY, Lerp);

            yield return 0;

        }

        CurDir = ToDir;
        CoroDir = Dir.Off;

    }

    public void SetBreakdownState(bool State)
    {
        b_InBreakdown = State;
    }

    public void SetEngineBreakdownState(bool State)
    {
        b_BreakEngine = State;
    }
}