using System;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Reflection;

/// <summary>
/// The State of the Global Network Manager.
/// If client is connected to a Master Client, this value will return the Network State
/// of the Master Client.
/// </summary>
public enum NETWORK_STATE
{
    DISCONNECTED,
    ROOM,
    LOBBY,
    GAME
}

/// <summary>
/// Interface for custom types that require members to be synced to the Master Client.
/// </summary>
public interface ICustomNetworkObject
{
    void OnRegisterNetworkObjects( );
}

/// <summary>
/// Title: RZ-NetworkManager
/// Global Network Manager for clients and master. 
/// Solely responsible for syncing low-level logic across clients.
/// DO NOT MODIFY WITHOUT MALCOLM'S CONSENT!
/// </summary>
public sealed class RZNetworkManager : PunBehaviour
{
    //Singleton
    public static RZNetworkManager Session { get; private set; }

    #region Flags
    public bool DebugMode = false;
    #endregion

    #region Events
    public delegate void NetworkStateChange( int newNetworkState );
    public static event NetworkStateChange OnNetworkStateChanged;
    #endregion

    #region Session Information
    public string MultiplayerLevelToLoad = "GenericArenaTest";
    public static int NetworkState { get; private set; }
    public static string LoadedLevelName { get; private set; }
    public static RZGameMode LoadedGameMode { get; private set; }
    #endregion

    #region Local References
    public static GameModeUI LocalHUD;
    public static PlayerController LocalController;
    public static PlayerInventory LocalInventory;
    #endregion

    #region Player Session Information
    public static Hashtable PlayerPropertiesHash = new Hashtable( )
    {
        {"IsReady", "false"}
    };

    //0 - Ready / Not Ready
    //1 - Player Character Reference
    #endregion

    #region DEBUG DELETE ME
    //Debug
    void OnGUI( )
    {
        if ( DebugMode )
        {
            GUILayout.BeginVertical( );

            GUILayout.Label( PhotonNetwork.connectionStateDetailed.ToString( ) );

            bool isInRoom = PhotonNetwork.inRoom;
            if ( GUILayout.Button( !isInRoom ? "Join Room" : "Leave Room" ) )
            {
                if ( isInRoom )
                    LeaveRoom( );
                else
                    JoinRoom( );
            }

            GUILayout.Label( "Players in room: " + PhotonNetwork.playerList.Length );
            GUILayout.Label( "SERVER NETWORK STATE: " + ( NETWORK_STATE )NetworkState );

            GUILayout.BeginHorizontal( );

            if ( PhotonNetwork.isMasterClient )
            {
                if ( NetworkState == ( int )NETWORK_STATE.ROOM )
                {
                    if ( GUILayout.Button( "LaunchGame" ) )
                    {
                        PhotonNetwork.RPC( PhotonView.Get( this ), "LaunchGame", PhotonTargets.AllBuffered, false );
                    }
                }
            }
            else
            {

            }

            GUILayout.EndHorizontal( );

            GUILayout.EndVertical( );
        }
    }
    #endregion

    #region Network Actions

    public static void Initialize( )
    {
        //Connect to photon
        if ( PhotonNetwork.ConnectUsingSettings( "0.01a" ) )
        {
            SetNetworkState( NETWORK_STATE.LOBBY.GetHashCode( ) );
        }
    }

    public static void JoinRoom( string roomName = "RZDeveloperRoom1" )
    {
        //For now, join a random lobby
        if ( PhotonNetwork.JoinOrCreateRoom( roomName, null, TypedLobby.Default ) )
        {
            SetNetworkState( NETWORK_STATE.ROOM.GetHashCode( ) );
        }
    }

    public static void LeaveRoom( )
    {
        if ( PhotonNetwork.LeaveRoom( ) )
        {
            SetNetworkState( NETWORK_STATE.LOBBY.GetHashCode( ) );

            //Load the main scene
            LoadScreen.LevelToLoad = "NetworkManagerTest";
            LoadScreen.LevelFinishedLoadingAction = new UnityAction<string>( OnNetworkLevelHasLoaded );
            SceneManager.LoadScene( "LoadingScene" );
        }
    }

    private static void SetNetworkState( int newNetworkState )
    {
        //Prepare new networkState
        switch ( newNetworkState )
        {
            default:
                print( "Reset ready" );
                //Reset ready status        
                PlayerPropertiesHash["IsReady"] = "false";
                PhotonNetwork.player.SetCustomProperties( PlayerPropertiesHash );
                break;
        }

        NetworkState = newNetworkState;
        if ( OnNetworkStateChanged != null )
        {
            OnNetworkStateChanged( NetworkState );
        }
    }
    #endregion

    #region Game Actions
    [PunRPC]
    public void LaunchGame( )
    {
        //Disable message queue
        PhotonNetwork.isMessageQueueRunning = false;

        //load level for all players
        if ( PhotonNetwork.isMasterClient )
        {
            SetNetworkState( ( int )NETWORK_STATE.GAME );
            photonView.RPC( "SetGameMode", PhotonTargets.AllBuffered, 0 );
        }

        //Load generic level for now
        LoadScreen.LevelToLoad = MultiplayerLevelToLoad;
        LoadScreen.LevelFinishedLoadingAction = new UnityAction<string>( OnNetworkLevelHasLoaded );
        PhotonNetwork.LoadLevel( "LoadingScene" );
    }
    #endregion

    #region Start / Update
    void Awake( )
    {
        if ( !Session )
            Session = this;
    }

    void Start( )
    {
        DontDestroyOnLoad( gameObject );
        SetNetworkState( NETWORK_STATE.DISCONNECTED.GetHashCode( ) );
        Initialize( );
    }
    #endregion

    #region Serialization
    /// <summary>
    /// Serializes important network information
    /// (IMPORTANT STUFFS ONLY)
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        //Writing
        if ( stream.isWriting )
        {
            if ( PhotonNetwork.isMasterClient )
            {
                stream.SendNext( NetworkState );
                stream.SendNext( LoadedLevelName );
                stream.SendNext( MultiplayerLevelToLoad );
            }
        }
        //Reading
        else if ( stream.isReading )
        {
            if ( !PhotonNetwork.isMasterClient )
            {
                NetworkState = ( int )stream.ReceiveNext( );
                LoadedLevelName = ( string )stream.ReceiveNext( );
                MultiplayerLevelToLoad = (string) stream.ReceiveNext();
            }
        }
    }
    #endregion

    #region Callbacks / Registration
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
    }

    void OnJoinedRoom( )
    {
        NETWORK_STATE state = ( NETWORK_STATE )NetworkState;
        switch ( state )
        {
            case NETWORK_STATE.DISCONNECTED:
                break;

            case NETWORK_STATE.ROOM:

                if ( PhotonNetwork.isMasterClient )
                {

                }

                break;

            case NETWORK_STATE.LOBBY:



                break;

            case NETWORK_STATE.GAME:
                break;
        }
    }

    void OnLeftRoom( )
    {
        if ( NetworkState == ( int )NETWORK_STATE.GAME )
        {
            LoadScreen.LevelToLoad = "NetworkManagerTest";
            SceneManager.LoadScene( "LoadingScene" );
        }
    }

    void OnPhotonPlayerConnected( PhotonPlayer player )
    {
    }

    void OnPhotonPlayerDisconnected( PhotonPlayer player )
    {

    }

    static void OnNetworkLevelHasLoaded( string loadedLevel )
    {
        if ( PhotonNetwork.isMasterClient )
        {
            //Start the game mode on server
            PhotonView view = PhotonView.Get(LoadedGameMode);
            view.RPC( "StartGame", PhotonTargets.AllBuffered );
        }

        PhotonNetwork.isMessageQueueRunning = true;

        RZNetworkManager.PlayerPropertiesHash["IsReady"] = "true";
        PhotonNetwork.player.SetCustomProperties( RZNetworkManager.PlayerPropertiesHash );
    }

    [PunRPC]
    private void SetGameMode(int gameModeIdx = 0)
    {
        switch (gameModeIdx)
        {
            case 0:
            {
                    LoadedGameMode = Session.gameObject.AddComponent<RZGameMode_Deathmatch>( );
                    Session.photonView.ObservedComponents.Add( LoadedGameMode );
                    Session.photonView.RefreshRpcMonoBehaviourCache( );
            }
            break;
        }
    }
    #endregion

    #region Exception

    private static void OnFailedToCreateRoom( )
    {

    }

    private static void OnFailedToJoinRoom( )
    {

    }

    #endregion
}