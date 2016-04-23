using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LoadScreen : MonoBehaviour
{
    private AsyncOperation Async_LoadLevelOperation;
    public static bool LevelIsLoaded { get; private set; }

    public static string LevelToLoad;
    public static UnityAction<string> LevelFinishedLoadingAction;

    void Start()
    {
        LevelIsLoaded = false;
        Async_LoadLevelOperation = SceneManager.LoadSceneAsync( LevelToLoad,
            LoadSceneMode.Additive );

        print( "Loading Level: " + LevelToLoad );
    }

    private void Update()
    {
        if (Async_LoadLevelOperation != null && Async_LoadLevelOperation.isDone)
        {
            print("Level Load complete");
            PhotonNetwork.isMessageQueueRunning = true;
            LevelIsLoaded = true;

            if (LevelFinishedLoadingAction != null)
            {
                LevelFinishedLoadingAction.Invoke(LevelToLoad);
            }

            LevelToLoad = string.Empty;
            LevelFinishedLoadingAction = null;

            SceneManager.UnloadScene( "LoadingScene" );
        }
    }
}