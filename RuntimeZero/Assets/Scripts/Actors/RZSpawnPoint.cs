using UnityEngine;
using System.Collections;
using Photon;

public class RZSpawnPoint : PunBehaviour
{
    [HideInInspector]
    public bool IsAvailable = true;

    private RZSpawnPoint[] SpawnPoints;

    public static RZSpawnPoint[] GetAllSpawnPoints()
    {
        return GameObject.FindObjectsOfType<RZSpawnPoint>();
    }

    [PunRPC]
    public static void SpawnPlayer(PhotonPlayer player)
    {
        RZSpawnPoint[] spawns = GetAllSpawnPoints();

        if (spawns.Length < PhotonNetwork.playerList.Length)
        {
            Debug.LogError("Not enough Spawn Points. Please place more.");
            return;
        }

        for (int i = 0; i < spawns.Length; i++)
        {
            if (spawns[i].IsAvailable)
            {
                var spawnLoc = new Vector3( spawns[i].transform.position.x, spawns[i].transform.position.y + 1, spawns[i].transform.position.z );
                PhotonNetwork.Instantiate( "Players/TestPlayer", spawnLoc, Quaternion.identity, 0 );
                spawns[i].IsAvailable = false;
                return;
            }
        }
    }
}
