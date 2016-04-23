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
    }
}
