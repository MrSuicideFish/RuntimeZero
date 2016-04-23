using UnityEngine;
using System.Collections;
using Photon;

/// <summary>
/// This is meant to be the parent class for all game modes in the game.
/// </summary>
public class RZGameMode : PunBehaviour
{
    public static RZGameMode Current;

    public string GameModeName = "Default Game Mode";
    
    public int  MinimumNumPlayers = 0,
                MaximumNumPlayers = 10,
                NumOfTeams = 1;

    public float TimePerRound = 60.0f;

    public bool TeamsEnabled = false;

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
            
        }
    }

    [PunRPC]
    public virtual void StartGame()
    {
        print("Starting Game mode " + GameModeName);
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