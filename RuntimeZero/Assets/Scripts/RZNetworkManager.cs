using UnityEngine;
using System.Collections;

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
    public static NETWORK_STATE NetworkState { get; private set; }
    #endregion

    //Debug
    void OnGUI( )
    {
        GUILayout.Label( PhotonNetwork.connectionStateDetailed.ToString( ) + " : " + NetworkState.ToString( ) );

        bool isInRoom = PhotonNetwork.inRoom;
        if ( GUI.Button( new Rect( 0, 64, 128, 32 ), !isInRoom ? "Join Room" : "Leave Room" ) )
        {
            if ( isInRoom )
                LeaveRoom( );
            else
                JoinRoom( );
        }

        //GUI.Label(new Rect(0, 256, 128, 32), );
    }

    void Start( )
    {
        NetworkState = NETWORK_STATE.DISCONNECTED;
        Initialize( );
    }

    public static void Initialize( )
    {
        //Connect to photon
        if ( PhotonNetwork.ConnectUsingSettings( "0.01a" ) )
        {
            NetworkState = NETWORK_STATE.LOBBY;
        }
    }

    public static void JoinRoom( )
    {
        //For now, join a random lobby
        if ( PhotonNetwork.JoinOrCreateRoom( "RZDeveloperRoom1", null, TypedLobby.Default ) )
        {
            NetworkState = NETWORK_STATE.ROOM;
        }
    }

    public static void LeaveRoom( )
    {
        if ( PhotonNetwork.LeaveRoom( ) )
        {
            NetworkState = NETWORK_STATE.LOBBY;
        }
    }

    #region Callbacks

    void OnJoinedRoom( )
    {

    }

    void OnLeftRoom( )
    {

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
