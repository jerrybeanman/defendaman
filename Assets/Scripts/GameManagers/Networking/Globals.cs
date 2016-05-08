/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Globals.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--
--  DATE:           February 20th, 2016
--
--  REVISIONS:      February 25th, 2016: Lobby code added
--                  March 30th, 2016: Class data added
--                  April 3rd, 2016: Refactored
--
--  DESIGNERS:      Jerry Jia, Carson Roscoe
--
--  PROGRAMMER:     Jerry Jia, Carson Roscoe
--
--  NOTES:
--  This file is used to hold all of our global variables/enums needed by different
--  parts of the game.
---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
public enum ClassType 		{ Gunner = 1, Ninja = 2, Wizard = 3, aman = 4 }
public enum GameState 		{ Playing, Dying, Dead, Won, Lost }
public enum BuildingType 	{ Wall = 1, WatchTower = 2, Turret = 3, Potion = 4, Upgarde = 5 }
public enum Themes			{ Grass = 1, Tron = 2 }

/*Carson
Being used to denote what type of data we are sending/receiving for a given JSON object.
e.g. Player is valued at 1. If we receive a JSON object for type Player ID 1, that is "Player 1's" data.
     Projectile is defined at 2. If we receive a JSON object for type Projectile ID 3, that is "Projectile 3's" data
    
Enviroment does not have an ID associated with it, since it is one entity. The ID we use for it will always default to 0

Note: Does not start at value 0. Reason being, if JSON parser fails, it returns 0 for fail, so checking
for fail does not work 
*/
public enum DataType        { Player = 1, Trigger = 2, Environment = 3, StartGame = 4, ControlInformation = 5, Lobby = 6,
                              Item = 7, UI = 8, Hit = 9, Killed = 10, TriggerKilled = 11, AI = 12, AIProjectile = 13,
                              SpecialCase = 14, Potion = 15, StatUpdate = 16, GameOver = 17 }
public enum Protocol { TCP, UDP, NA }

// Lobby message key fields 
public class NetworkKeyString
{
    public static string PlayerID       = "PlayerID";
    public static string TeamID         = "TeamID";
    public static string ClassID        = "ClassID";
	public static string AmanID			= "AmanID";
    public static string WorldItemID    = "WorldItemID";
    public static string Ready          = "Ready";
	public static string Theme			= "Theme";
    public static string StartGame      = "StartGame";
    public static string UserName       = "UserName";
    public static string Message        = "Message";
    public static string XPos           = "XPos";
    public static string YPos           = "YPos";
    public static string ZPos           = "ZPos";
    public static string XRot           = "XRot";
    public static string YRot           = "YRot";
    public static string ZRot           = "ZRot";
    public static string BuildType      = "BuildType";
    public static string Amount         = "Amount";
    public static string MapWidth       = "mapWidth";
    public static string MapHeight      = "mapHeight";
    public static string MapIds         = "mapIDs";
    public static string MapScenery     = "mapSceneryIDs";
    public static string MapResources   = "mapResources";
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
        { Constants.ARMOR_STAT, 0},
        { Constants.SPEED_STAT, 0}
    };
}

public class GameData
{
	// Blocks all in game keyboard inputs
	public static bool 			InputBlocked 	 = false;


    public static Dictionary<int,PlayerData> LobbyData = new Dictionary<int, PlayerData>();
	public static Dictionary<Vector3, Building> Buildings = new Dictionary<Vector3, Building>();

	public static PlayerData 	MyPlayer		= new PlayerData();
	public static int 			EnemyKingID		= -1;
    public static int 			AllyKingID 		= -1;
    public static bool 			MouseBlocked 	{ get; set; }
    public static bool          ItemCollided    = false;
              
    public static Dictionary<int, Vector3> PlayerPosition = new Dictionary<int, Vector3>();   

	public static int 			Seed			{ get; set; }
	public static bool			GameStart		= false;
	public static string 		IP;
    public static GameState     GameState       = GameState.Playing;
    //Pair of x/y spawn points where index+1 is teamid
    public static List<Pair<int, int>> TeamSpawnPoints = new List<Pair<int, int>>();
    public static Pair<int, int> aiSpawn = new Pair<int, int>(10, 10);

	public static Themes		CurrentTheme	= Themes.Grass;

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
    public static int EnemyTeamKillCount 
	{
        get {
            return _enemyTeamKillCount;
        }
        set {
            _enemyTeamKillCount = value;
            GameObject.Find("Enemy Score").GetComponent<Text>().text = value.ToString();
        }
    }
}
