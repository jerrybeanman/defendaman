using UnityEngine;

//Carson
public class GameStart_Testing : MonoBehaviour {
    int myID = -1;
    NetworkingManager networkingManager;

	// Use this for initialization
	void Start () {
        networkingManager = GetComponent<NetworkingManager>();
	}
	
	// Update is called once per frame
	void Update () {
        //if (GameData.GameStart)
        //    return;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            myID = 1;
            StartOfGame();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            myID = 2;
            StartOfGame();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            myID = 3;
            StartOfGame();
        }
        /*if (Input.GetKeyDown(KeyCode.V))
        {
            myID = 4;
            StartOfGame();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            myID = 5;
            StartOfGame();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            myID = 6;
            StartOfGame();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            myID = 7;
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            myID = 8;
            StartOfGame();
        }*/
    }

    public void StartOfGame()
    {
		GameData.GameStart = true;

        GameData.TeamSpawnPoints.Clear();
        GameData.LobbyData.Clear();

        int i = 1;
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Wizard, PlayerID = i++, TeamID = 2 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });
        GameData.LobbyData[i] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = i++, TeamID = 1 });

        GameData.MyPlayer = GameData.LobbyData[myID];
        if (GameData.MyPlayer == null)
            Debug.Log("Player data not set");

        if (Application.platform != RuntimePlatform.LinuxPlayer)
        {
            GameData.TeamSpawnPoints.Add(new Pair<int, int>(32, 32));
            GameData.TeamSpawnPoints.Add(new Pair<int, int>(50, 50));
        }

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
