﻿ using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class FlockSettings : ScriptableObject
{
    [Header("General Attack Settings")]
    public AttackType attackType;

    public enum AttackType
    {
        Bullet,
        Laser,
        Kamikaze,

        none
    }

    public AttackFocus attackFocus ;

    public enum AttackFocus
    {
        Display,
        Movement,
        Weapon,

        none
    }

    [Tooltip("in Second")]
    public int timeBetweenAttacks;


    [Header("Bullet")]
    
    [Tooltip("bullet per sec")]
    [Range(0f,5f)]
    public float fireRate = 0;
    [Tooltip("nb total de bullet a tirer avant de retourner en roam")]
    public float nbBulletToShoot = 0;

    [Header("Laser")]
    [Tooltip("in Second")]
    public float chargingAttackTime = 0;
    [Tooltip("in Second, avant de retourner en roam")]
    public float activeDuration = 0;

    [Header("Kamikaze")]
    public float speedToTarget;


    [Header("Boids")]


    [Tooltip("Index 0 : Roam | Index 1 : Attack")]
    public BoidSettings[] boidSettings;

    public int spawnTimer = 10;


    [Range(10,100)]
    public int boidSpawn;

    [Range(10,100)]
    public int maxBoid;

    [Tooltip("boids per min")]
    public int regenerationRate;


}