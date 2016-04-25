using UnityEngine;
using System.Collections;

public enum RZNetworkEventType
{
    DEFAULT = 0,
    ITEM_PICKUP = 1,
}
public class RZEventManager : ScriptableObject
{
    //Delegates
    public delegate void GlobalPlayerBroadcastDelegate(PhotonPlayer player, object Obj);

    //Events
    public static event GlobalPlayerBroadcastDelegate OnItemPickedUp;
    public static event GlobalPlayerBroadcastDelegate OnPlayerKilled;

}