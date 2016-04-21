using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public enum NETWORK_STATE
{
    DISCONNECTED,
    ROOM,
    LOBBY,
    GAME
}

public class RZNetworkManager : MonoBehaviour
{
    #region Flags
    public bool DebugMode = false;
    #endregion

    #region Events

    public delegate void NetworkStateChange(int newNetworkState);
    public static event NetworkStateChange OnNetworkStateChanged;

    #endregion

    #region Session Information
    public static int NetworkState { get; private set; }
    public static string LoadedLevelName { get; private set; }
    #endregion

    //Debug
    void OnGUI( )
    {
        if (DebugMode)
        {
            GUILayout.BeginVertical();

            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

            bool isInRoom = PhotonNetwork.inRoom;
            if ( GUILayout.Button(!isInRoom ? "Join Room" : "Leave Room"))
            {
                if (isInRoom)
                    LeaveRoom();
                else
                    JoinRoom();
            }

            GUILayout.Label("Players in room: " + PhotonNetwork.playerList.Length);
            GUILayout.Label("SERVER NETWORK STATE: " + (NETWORK_STATE) NetworkState);

            GUILayout.BeginHorizontal();

            if (PhotonNetwork.isMasterClient)
            {
                if (NetworkState == (int) NETWORK_STATE.ROOM)
                {
                    if (GUILayout.Button("LaunchGame"))
                    {
                        LaunchGame();
                    }
                }
            }
            else
            {
                
            }

            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }

    #region Serialize

    /// <summary>
    /// Serializes important network information
    /// (IMPORTANT STUFFS ONLY)
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="info"></param>
    void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        //Writing
        if (stream.isWriting)
        {
            if (PhotonNetwork.isMasterClient)
            {
                stream.SendNext( NetworkState );
                stream.SendNext( LoadedLevelName );
            }
        }
        //Reading
        else if (stream.isReading)
        {
            if (!PhotonNetwork.isMasterClient)
            {
                NetworkState = (int) stream.ReceiveNext();
                LoadedLevelName = (string) stream.ReceiveNext();
            }
        }
    }
    #endregion

    void Start( )
    {
        DontDestroyOnLoad( gameObject );
        SetNetworkState(NETWORK_STATE.DISCONNECTED.GetHashCode());
        Initialize( );

        //if (!LoadScreen.LevelIsLoaded)
        //{
        //    PlayerPrefs.SetString("TargetSceneToLoad", "GenericArenaTest" );
        //    LoadScreen.BeginLoadScene();
        //}
    }

    public static void Initialize( )
    {
        //Connect to photon
        if ( PhotonNetwork.ConnectUsingSettings( "0.01a" ) )
        {
            SetNetworkState(NETWORK_STATE.LOBBY.GetHashCode());
        }
    }

    public static void JoinRoom( string roomName = "RZDeveloperRoom1" )
    {
        //For now, join a random lobby
        if ( PhotonNetwork.JoinOrCreateRoom( roomName, null, TypedLobby.Default ) )
        {
            SetNetworkState(NETWORK_STATE.ROOM.GetHashCode());
        }
    }

    public static void LeaveRoom( )
    {
        if ( PhotonNetwork.LeaveRoom( ) )
        {
            SetNetworkState(NETWORK_STATE.LOBBY.GetHashCode( ));
        }
    }

    [PunRPC]
    public static void LaunchGame()
    {
        //Disable message queue
        PhotonNetwork.isMessageQueueRunning = false;

        //load level for all players
        if (PhotonNetwork.isMasterClient)
        {
            SetNetworkState((int) NETWORK_STATE.GAME);
            LoadedLevelName = "GenericArenaTest";
            PhotonNetwork.LoadLevel(0);
        }
    }

    private static void SetNetworkState(int newNetworkState)
    {
        //Prepare new networkState
        switch (newNetworkState)
        {

        }

        NetworkState = newNetworkState;

        if (OnNetworkStateChanged != null)
        {
            OnNetworkStateChanged(NetworkState);
        }
    }

    #region Callbacks

    void OnJoinedRoom( )
    {
        NETWORK_STATE state = (NETWORK_STATE) NetworkState;
        switch (state)
        {
                case NETWORK_STATE.DISCONNECTED:
                break;

                case NETWORK_STATE.ROOM:
                break;

                case NETWORK_STATE.LOBBY:
                break;

                case NETWORK_STATE.GAME:
                break;
        }
    }

    void OnLeftRoom( )
    {
        if (NetworkState == (int) NETWORK_STATE.GAME)
        {
            LoadedLevelName = "NetworkManagerTest";
            LoadScreen.BeginLoadScene();
        }
    }

    void OnPhotonPlayerConnected( PhotonPlayer player )
    {

    }

    void OnPhotonPlayerDisconnected( PhotonPlayer player )
    {

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
