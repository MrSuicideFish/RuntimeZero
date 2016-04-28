using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RZGameMode_Deathmatch : RZGameMode
{
    private float CurrentTime;
    private float TimerLimit;
    private int FragLimit;
    private int TopFrag;
    private bool NewRound;

    private bool Waiting = false;

    private float WaveTimer; //Spawn all players at end of Wave Timer and reset
    private float WaveIncrement = 10;

    public RZGameMode_Deathmatch()
        : base()
    {
        GameModeName = "Deathmatch";

        MinimumNumPlayers = 1;
        MaximumNumPlayers = 10;
        NumOfTeams = 1;
    }

    [PunRPC]
    public override void StartGame()
    {

        base.StartGame();
        //LoadMap();                       //already done in base;
        //WaitForAllPlayersToReady();     // already done in base;
        SetWorldConditions();
        StartRound();
    }

    void SetWorldConditions()
    {
        if (PhotonNetwork.isMasterClient)
        {
            // Set World Conditions for this Game Session
            TimePerRound = 360.0f;
            FragLimit = 30;
            TopFrag = 0;
            CurrentTime = Time.time;
            TimerLimit = CurrentTime + TimePerRound;
            WaveTimer = CurrentTime + WaveIncrement;
        }
    }


    [PunRPC]
    public override void StartRound()
    {
        //Game Begin
        base.StartRound();

        //SetMapConditions();  //Sets Environment factors to default state (gates,walls,doors,ect);

        if (PhotonNetwork.isMasterClient)
        {
            if (NewRound == true)
            {
                SetRoundConditions();  // Resets objects and conditions to default state.
                /*        In deathmatch we skip this, or assume its conditions are On player Death.
                          In a teambased mode, we might need to track everyones death.
                */
                SetPlayerConditions();
                SpawnPlayers(); // Takes each player and spawns them in a random position;
            }
        }
    }

    void SetRoundConditions()
    {
        //Sets Objectives.....to nothing (its deathmatch)
    }

    void SetPlayerConditions()
    {
        //Defines Player prefab stats that will spawn.
        //Spawn immunity,Weapons,Perks..etc
        if (PhotonNetwork.isMasterClient)
        {
            //Player.hitpoints = 100;
            //Player.armor = 50;
            //Player.SpawnImmunity();
        }
    }

    void SetMapConditions()
    {
        //Sets Environment factor to default state 
        //Both master and client do this (server independent stuff, particles...)
        //RZActors Listen for reset events;
        EventSystem
    }

    void SpawnPlayers()
    {
        if (PhotonNetwork.isMasterClient)
        {
            //I am a Master
            //Find and open Spawn Positions and allocate Players Randomly
            int RandomStartingPoint = Random.Range(0, SpawnPoints.Count);
            int SpawnLoc = (RandomStartingPoint); // random starting position
            for (int i = 0; i >= SpawnPoints.Count; i++)
            {
                SpawnLoc = (RandomStartingPoint + i) % SpawnPoints.Count;
                if (SpawnPoints[SpawnLoc].IsTaken == false)
                {
                    RZSpawnPoint.SpawnPlayer(PhotonNetwork.Player);
                }
            }
        }
        else
        {
            foreach (GameObject bucket in buckets)
            {
                stopTimer = timerReset;
                bucket.collider.enabled = true;
                stopObject = false;
            }
        }
    }

    public override void Update()
    {
        base.Update();
        //End Round Condition Checking.
        if (!Waiting)
        {
            if (CheckRoundConditions() || CheckPlayerConditions() == true)
            {
                EndRound();
            }
        }
        if (Waiting)
        {
            StartRound();
        }
    }

    bool CheckRoundConditions()
    {
        bool EndFlag = false;
        //Objective Checking.
        if (Level.Objective1 == true) // if the Object is completed
        {
            EndFlag = true;
        }

        return EndFlag;
    }

    bool CheckPlayerConditions()
    {
        bool EndFlag = false;
        //Player Checking.
        if (Player.hp <= 0) // if the player is dead , respawn player
        {
            EndFlag = true;
        }

        return EndFlag;
    }

    [PunRPC]
    public override void EndRound()
    {
        //Game End
        base.EndRound();
        if (CheckWorldConditions() == true)  // Checks the Game conditions, and returns true if one is completed.
        {
            EndGame();
        }
        else {
            Waiting = true;
        }
    }


    [PunRPC]
    public override void EndGame()
    {
        //Post Game End
        base.EndGame();
        ReturnToLobby();
    }


    bool CheckWorldConditions()
    {
        bool EndWorld = false;
        if (!WaitingForReadyPlayers)
        {
            CurrentTime = Time.time;
            if (PhotonNetwork.isMasterClient) //If Master
            {
                if (TimerLimit <= CurrentTime)
                {
                    EndWorld = true;
                }
                if (TopFrag >= FragLimit)
                {
                    EndWorld = true;
                }
            }
        }
        return EndWorld;
    }

    void ReturnToLobby()
    {
        // Return Player to the match making server 
    }




}