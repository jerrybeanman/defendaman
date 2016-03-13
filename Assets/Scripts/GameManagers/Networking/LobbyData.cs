using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ClassType { Gunner = 1, Ninja = 2, Wizard = 3, NotImplemented = 4}

public class PlayerData {
    public int PlayerID { get; set; }
    public string Username { get; set; }
    public int TeamID { get; set; }
    public ClassType ClassType { get; set; }
	public bool Ready { get; set; }
    //public bool King { get; set; }
}

public class GameData
{
    public static Dictionary<int,PlayerData> LobbyData = new Dictionary<int, PlayerData>();
	public static PlayerData 	MyPlayer		= new PlayerData();
    public static int 			EnemyKingID 	{ get; set; }
    public static int 			AllyKingID 		{ get; set; }
    public static bool 			MouseBlocked 	{ get; set; }

    //Pair of x/y spawn points where index+1 is teamid
    public static List<Pair<int, int>> TeamSpawnPoints = new List<Pair<int, int>>();
}
