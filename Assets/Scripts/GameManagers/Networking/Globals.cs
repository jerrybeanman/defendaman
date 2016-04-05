﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public enum ClassType { Gunner = 1, Ninja = 2, Wizard = 3, aman = 4}
public enum GameState { Playing, Dying, Dead, Won, Lost}
public enum BuildingType { Wall = 1, WatchTower = 2, Turret = 3, Potion = 4, Upgarde = 5}
// Lobby message key fields 
public class NetworkKeyString
{
	public static string PlayerID 		= "PlayerID";
	public static string TeamID			= "TeamID";
	public static string ClassID		= "ClassID";
	public static string Ready			= "Ready";
	public static string StartGame 		= "StartGame";
	public static string UserName   	= "UserName";
	public static string Message		= "Message";
    public static string XPos       	= "XPos";
    public static string YPos       	= "YPos";
    public static string ZPos       	= "ZPos";
    public static string XRot       	= "XRot";
    public static string YRot       	= "YRot";
    public static string ZRot       	= "ZRot";
	public static string BuildType  	= "BuildType";
	public static string Amount     	= "Amount";
	public static string MapWidth   	= "mapWidth";
	public static string MapHeight  	= "mapHeight";
	public static string MapIds    	 	= "mapIDs";
	public static string MapScenery 	= "mapSceneryIDs";
	public static string MapResources 	= "mapResources";
}

public class PlayerData 
{
    public int PlayerID = -1;
    public string Username { get; set; }
    public int TeamID { get; set; }
	public ClassType ClassType = ClassType.Gunner;
    public bool Ready = false;
    public Dictionary<string, int> Resources = new Dictionary<string, int>()
    {
        { Constants.GOLD_RES, 0 }
    };
    public Dictionary<string, int> WeaponStats = new Dictionary<string, int>()
    {
        { Constants.DAMAGE_STAT, 0},
        { Constants.ARMOR_STAT, 0}
    };
    //public bool King { get; set; }
}

public class GameData
{
	// Blocks all in game keyboard inputs
	public static bool 			InputBlocked 	 = false;
	// Blocks mouse inputs for combat
	public static bool 			KeyBlocked 		 = false;


    public static Dictionary<int,PlayerData> LobbyData = new Dictionary<int, PlayerData>();
	public static Dictionary<Vector3, Building> Buildings = new Dictionary<Vector3, Building>();

	public static PlayerData 	MyPlayer		= new PlayerData();
    public static int 			EnemyKingID 	{ get; set; }
    public static int 			AllyKingID 		{ get; set; }
    public static bool 			MouseBlocked 	{ get; set; }

              
    public static Dictionary<int, Vector3> PlayerPosition = new Dictionary<int, Vector3>();   

	public static int 			Seed			{ get; set; }
	public static bool			GameStart		= false;
	public static string 		IP;
    public static GameState     GameState       = GameState.Playing;
    //Pair of x/y spawn points where index+1 is teamid
    public static List<Pair<int, int>> TeamSpawnPoints = new List<Pair<int, int>>();
    public static Pair<int, int> aiSpawn = new Pair<int, int>(10, 10);


    private static int _allyTeamKillCount = 0;
    public static int AllyTeamKillCount
    {
        get
        {
            return _allyTeamKillCount;
        }
        set
        {
            _allyTeamKillCount = value;
            GameObject.Find("Ally Score").GetComponent<Text>().text = value.ToString();
        }
    }
    private static int _enemyTeamKillCount = 0;
    public static int EnemyTeamKillCount {
        get {
            return _enemyTeamKillCount;
        }
        set {
            _enemyTeamKillCount = value;
            GameObject.Find("Enemy Score").GetComponent<Text>().text = value.ToString();
        }
    }
}
