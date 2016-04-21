using UnityEngine;
using System.Collections;

/// <summary>
/// This is meant to be the parent class for all game modes in the game.
/// </summary>
public class RZGameMode : MonoBehaviour
{
    public string GameModeName = "Default Game Mode";
    
    public int  MinimumNumPlayers = 0,
                MaximumNumPlayers = 10,
                NumOfTeams = 1;

    public float TimePerRound = 60.0f;

    public bool TeamsEnabled = false;

    public virtual void StartGame()
    {
        
    }

    public virtual void StartRound()
    {
        
    }

    public virtual void EndRound()
    {
        
    }

    public virtual void EndGame()
    {
        
    }
}