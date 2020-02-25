﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SC_GameStates : NetworkBehaviour
{

    #region Singleton

    private static SC_GameStates _instance;
    public static SC_GameStates Instance { get { return _instance; } }

    #endregion

    public enum GameState {Lobby, Tutorial, Game, GameEnd }
    public GameState CurState;

    // Start is called before the first frame update
    void Start()
    {
        if(isServer)
            RpcSetState(GameState.Lobby);
    }

    [ClientRpc]
    public void RpcSetState(GameState TargetState)
    {

        CurState = TargetState;  

        switch (TargetState)
        {
            case GameState.Lobby:

                break;

            case GameState.Tutorial:

                break;

            case GameState.Game:

                break;

            case GameState.GameEnd:
                if (!isServer)
                    SC_EndGameOP.Instance.EndGameDisplay();
                break;

        }

    }

}