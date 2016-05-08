using UnityEngine;

//Carson
public class GameStart_Testing : MonoBehaviour {
    public int myID = -1;
    NetworkingManager networkingManager;
	public static GameStart_Testing instance;

	// Use this for initialization
	void Start () {
		//Check if instance already exists
		if (instance == null)				
			//if not, set instance to this
			instance = this;				
		//If instance already exists and it's not this:
		else if (instance != this)			
			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
			Destroy(gameObject);   		
        networkingManager = GetComponent<NetworkingManager>();
	}
	
	// Update is called once per frame
	void Update () {
    }

    public void StartOfGame()
    {
		GameData.GameStart = true;
        GameData.AllyTeamKillCount = 0;
        GameData.EnemyTeamKillCount = 0;

        GameData.TeamSpawnPoints.Clear();
        GameData.LobbyData.Clear();

        int i = 1;
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Wizard, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 2 });
        /*GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });*/

        GameData.MyPlayer = GameData.LobbyData[myID];
        if (GameData.MyPlayer == null)
            Debug.Log("Player data not set");

        GameData.Seed = 1000;

        GameData.IP = "192.168.0.3";

        if (GameObject.Find("NetworkManager") == null)
            GameManager.instance.StartGame(GameData.Seed);
        else
		    Application.LoadLevel("EngineTeam_master");

        // Update the stat boost a player gets from a compatible equipped weapon
        //Inventory.instance.UpdateWeaponStats();
    }
}
