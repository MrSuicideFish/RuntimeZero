using UnityEngine;
using System.Collections;
using Photon;

public class RZActor : PunBehaviour
{
    protected virtual void Awake()
    {
        //Hook into game mode events
        RZNetworkManager.LoadedGameMode.OnGameStart += OnGameStart;
        RZNetworkManager.LoadedGameMode.OnRoundStart += OnRoundStart;
        RZNetworkManager.LoadedGameMode.OnRoundEnd += OnRoundEnd;
        RZNetworkManager.LoadedGameMode.OnGameEnd += OnGameEnd;
    }

    //EVENT CALLBACKS
    protected virtual void OnGameStart()
    {
        
    }

    protected virtual void OnRoundStart( )
    {

    }

    protected virtual void OnRoundEnd( )
    {

    }

    protected virtual void OnGameEnd( )
    {

    }

    protected virtual void OnPhotonInstantiate()
    {
        
    }
}