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
        if (Input.GetKeyDown(KeyCode.V))
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
        }
    }

    public void StartOfGame()
    {
        NetworkingManager.InGame = true;

        GameData.TeamSpawnPoints.Clear();
        GameData.LobbyData.Clear();

        GameData.LobbyData[1] = (new PlayerData { ClassType = ClassType.Ninja, PlayerID = 1, TeamID = 1 });
        GameData.LobbyData[2] = (new PlayerData { ClassType = ClassType.Wizard, PlayerID = 2, TeamID = 2 });
        GameData.LobbyData[3] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = 3, TeamID = 1 });
        GameData.LobbyData[4] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = 4, TeamID = 2 });
        GameData.LobbyData[5] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = 5, TeamID = 1 });
        GameData.LobbyData[6] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = 6, TeamID = 2 });
        GameData.LobbyData[7] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = 7, TeamID = 1 });
        GameData.LobbyData[8] = (new PlayerData { ClassType = ClassType.Gunner, PlayerID = 8, TeamID = 2 });

        GameData.MyPlayer.PlayerID = myID;

        if (Application.platform != RuntimePlatform.LinuxPlayer)
        {
            GameData.TeamSpawnPoints.Add(new Pair<int, int>(30, 30));
            GameData.TeamSpawnPoints.Add(new Pair<int, int>(50, 50));
        }

        GameManager.instance.StartGame(1000);
        //networkingManager.update_data("[{DataType : 4, ID : 0, Seed : 1000}]");
    }
}
