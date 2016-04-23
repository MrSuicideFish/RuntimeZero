using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadScreen : MonoBehaviour
{
    private AsyncOperation Async_LoadLevelOperation;
    public static bool LevelIsLoaded { get; private set; }
    private static string LevelToLoad;

    private static UnityAction<string> LevelFinishedLoadingAction;

    public static void BeginLoadScene( UnityAction<string> asyncFinishAction = null )
    {
        LevelIsLoaded = false;
        LevelToLoad = RZNetworkManager.LoadedLevelName;
        LevelFinishedLoadingAction = asyncFinishAction;
        SceneManager.LoadScene("LoadingScene");
    }

    void Start()
    {
        LevelToLoad = RZNetworkManager.LoadedLevelName;

        Async_LoadLevelOperation = SceneManager.LoadSceneAsync( LevelToLoad,
            LoadSceneMode.Additive );

        print( "Begin load" );
    }

    private void Update()
    {
        if (Async_LoadLevelOperation != null && Async_LoadLevelOperation.isDone)
        {
            print("Level Load complete");

            PhotonNetwork.isMessageQueueRunning = true;
            SceneManager.UnloadScene("LoadingScene");
            LevelIsLoaded = true;

            if (LevelFinishedLoadingAction != null)
            {
                LevelFinishedLoadingAction.Invoke(LevelToLoad);
            }
        }
    }
}