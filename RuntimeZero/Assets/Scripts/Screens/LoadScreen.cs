using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadScreen : MonoBehaviour
{
    private AsyncOperation Async_LoadLevelOperation;
    public static bool LevelIsLoaded { get; private set; }

    public static void BeginLoadScene()
    {
        LevelIsLoaded = false;
        SceneManager.LoadScene("LoadingScene");
    }

    void Start()
    {
        Async_LoadLevelOperation = SceneManager.LoadSceneAsync( RZNetworkManager.LoadedLevelName,
            LoadSceneMode.Additive );

        print( "Begin load" );
    }

    private void Update()
    {
        if (Async_LoadLevelOperation != null && Async_LoadLevelOperation.isDone)
        {
            print("Level Load complete");
            PhotonNetwork.isMessageQueueRunning = true;
            LevelIsLoaded = true;
            SceneManager.UnloadScene("LoadingScene");
        }
    }
}