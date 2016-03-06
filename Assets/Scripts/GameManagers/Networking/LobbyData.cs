using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ClassType { Gunner = 1, Ninja = 2, Wizard = 3, NotImplemented = 4}

public struct PlayerData {
    public int PlayerID { get; set; }
    public string Username { get; set; }
    public int TeamID { get; set; }
    public ClassType ClassType { get; set; }
    //public bool King { get; set; }
}

public class GameData
{
    public static List<PlayerData> LobbyData = new List<PlayerData>();
    public static int MyPlayerID { get; set; }
    public static int EnemyKingID { get; set; }
    public static int AllyKingID { get; set; }
}
