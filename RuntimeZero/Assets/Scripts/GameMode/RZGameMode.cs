using UnityEngine;
using System.Collections;
using Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// This is meant to be the parent class for all game modes in the game.
/// </summary>
public class RZGameMode : PunBehaviour
{
    public static RZGameMode Current;

    protected string GameModeName = "Default Game Mode";

    protected int   MinimumNumPlayers = 0,
                    MaximumNumPlayers = 10,
                    NumOfTeams = 1;

    protected float TimePerRound = 60.0f;

    protected bool  TeamsEnabled = false,
                    WaitingForReadyPlayers = false;

    protected virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        //Writing
        if ( stream.isWriting )
        {
            if ( PhotonNetwork.isMasterClient )
            {

            }
        }
        //Reading
        else if ( stream.isReading )
        {
            if ( !PhotonNetwork.isMasterClient )
            {

            }
        }
    }

    public virtual void Update()
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (WaitingForReadyPlayers)
            {
                bool allReady = true;
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    allReady = ((string) PhotonNetwork.playerList[i].customProperties[0]) == "true";
                }

                if (allReady)
                {
                    WaitingForReadyPlayers = false;
                    StartRound();
                    return;
                }
            }
        }
    }

    [PunRPC]
    public virtual void StartGame()
    {
        print("Starting Game mode " + GameModeName);

        if ( PhotonNetwork.isMasterClient )
        {
            WaitingForReadyPlayers = true;
        }

        RZNetworkManager.PlayerPropertiesHash["IsReady"] = "true";
        PhotonNetwork.player.SetCustomProperties( RZNetworkManager.PlayerPropertiesHash );
    }

    [PunRPC]
    public virtual void StartRound()
    {
        print( "Starting Round..." );
    }

    [PunRPC]
    public virtual void EndRound()
    {
        print( "Ending Round..." );
    }

    [PunRPC]
    public virtual void EndGame()
    {
        print( "Ending Game..." );
    }
}