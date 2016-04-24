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
                    WaitingForReadyPlayers = true;

    protected virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        //Writing
        if ( stream.isWriting )
        {
            if ( PhotonNetwork.isMasterClient )
            {
                stream.SendNext( GameModeName );
                stream.SendNext( MinimumNumPlayers );
                stream.SendNext( MaximumNumPlayers );
                stream.SendNext( NumOfTeams );
                stream.SendNext( TimePerRound );
                stream.SendNext( TeamsEnabled );
                stream.SendNext( WaitingForReadyPlayers );
            }
        }
        //Reading
        else if ( stream.isReading )
        {
            if ( !PhotonNetwork.isMasterClient )
            {
                GameModeName            = ( string )stream.ReceiveNext();
                MinimumNumPlayers       = ( int )stream.ReceiveNext( );
                MaximumNumPlayers       = ( int )stream.ReceiveNext( );
                NumOfTeams              = ( int )stream.ReceiveNext( );
                TimePerRound            = ( float )stream.ReceiveNext( );
                TeamsEnabled            = ( bool )stream.ReceiveNext( );
                WaitingForReadyPlayers  = ( bool )stream.ReceiveNext( );
            }
        }
    }

    public virtual void Update()
    {
        if (PhotonNetwork.isMasterClient)
        {
            if (WaitingForReadyPlayers)
            {
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    if ((string) PhotonNetwork.playerList[i].customProperties["IsReady"] != "true")
                        return;
                }

                photonView.RPC( "StartRound", PhotonTargets.AllBuffered );
                WaitingForReadyPlayers = false;
            }
        }
    }

    public virtual void OnGUI()
    {
        if (LoadScreen.LevelIsLoaded)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 75;

            if (WaitingForReadyPlayers)
            {
                GUI.Label(new Rect(Screen.width/2, Screen.height/2, 500, 500), "Waiting For Players...");

                //if (PhotonNetwork.isMasterClient && GUI.Button( new Rect( Screen.width / 2, 0, 300, 128 ), "Start Game" ) )
                //{
                //    photonView.RPC( "StartRound", PhotonTargets.AllBufferedViaServer );
                //    WaitingForReadyPlayers = false;
                //}
            }
        }
    }

    [PunRPC]
    public virtual void StartGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            print("Starting Game mode on Master" + GameModeName);
        }
        else
        {
            print( "Starting Game mode on Client" + GameModeName );
        }
    }

    [PunRPC]
    public virtual void StartRound()
    {
        print( "Starting Round..." );

        //Spawn players
        RZSpawnPoint.SpawnPlayer(PhotonNetwork.player);
    }

    [PunRPC]
    public virtual void EndRound()
    {
        print( "Ending Round..." );
    }

    [PunRPC]
    public virtual void EndGame()
    {
        print("Ending Game...");
    }
}