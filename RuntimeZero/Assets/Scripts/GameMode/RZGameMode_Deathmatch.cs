using UnityEngine;
using System.Collections;

public class RZGameMode_Deathmatch : RZGameMode
{
    public RZGameMode_Deathmatch()
        : base()
    {
        GameModeName = "Deathmatch";

        MinimumNumPlayers = 1;
        MaximumNumPlayers = 10;
        NumOfTeams = 1;

        TimePerRound = 999999.9f;
    }

    public override void StartGame()
    {
        base.StartGame();
    }
}