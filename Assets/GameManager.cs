using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

//Carson
public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public enum LobbyData { GameEnd = 1, Disconnected = 2 }

    void Awake()
    {
        if (instance == null)               //Check if instance already exists
            instance = this;                //if not, set instance to this
        else if (instance != this)          //If instance already exists and it's not this:
            Destroy(gameObject);            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager. 
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        NetworkingManager.Subscribe(gameEnd, DataType.Lobby, (int)LobbyData.GameEnd);
		StartGame (GameData.Seed);
    }

    public void PlayerTookDamage(int playerID, float damage, BaseClass.PlayerBaseStat ClassStat)
    {
        if (GameData.MyPlayer.PlayerID == playerID)
        {
            HUD_Manager.instance.UpdatePlayerHealth(-(damage / ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0)
                PlayerDied();
        }

        if (playerID == GameData.AllyKingID)
        {
            HUD_Manager.instance.UpdateAllyKingHealth(-(damage / ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0)
                GameLost();

        }

        if (playerID == GameData.EnemyKingID)
        {
            HUD_Manager.instance.UpdateEnemyKingHealth(-(damage / ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0)
                GameWon();
        }
    }

    private static void PlayerDied()
    {
        Debug.Log("You have died");
    }

    public void StartGame(int seed)
    {
        int myPlayer = GameData.MyPlayer.PlayerID;
        int myTeam = 0;
        List<Pair<int, int>> kings = new List<Pair<int, int>>();
        
        NetworkingManager.instance.update_data(NetworkingManager.GenerateMapInJSON(seed));
        
        foreach (var playerData in GameData.LobbyData)
        {
			var createdPlayer = ((Transform)Instantiate(NetworkingManager.instance.playerType, new Vector3(GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].first, GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].second, -10), Quaternion.identity)).gameObject;

            switch (playerData.Value.ClassType)
            {
                case ClassType.Ninja:
                    createdPlayer.AddComponent<NinjaClass>();
                    break;
                case ClassType.Gunner:
                    createdPlayer.AddComponent<GunnerClass>();
                    break;
                case ClassType.Wizard:
                    createdPlayer.AddComponent<WizardClass>();
                    break;
                default:
                    Debug.Log("Player " + playerData.Value.PlayerID + " has not selected a valid class. Defaulting to Gunner");
                    createdPlayer.AddComponent<GunnerClass>();
                    break;
            }

            //TODO: Get Micah to re-hook this up. Current fails cause missing a prefab
            /*if (myTeam == playerData.Value.TeamID) {
				var lighting = ((Transform)Instantiate(lightSource, createdPlayer.transform.position, Quaternion.identity)).gameObject;
				lighting.transform.parent = createdPlayer.transform;
				lighting.transform.Rotate (0,0,-90);
				lighting.transform.Translate(0,0,9);
			}*/

            createdPlayer.GetComponent<BaseClass>().team = playerData.Value.TeamID;
            createdPlayer.GetComponent<BaseClass>().playerID = playerData.Value.PlayerID;

            //if (playerData.King) //Uncomment this one line when kings are in place
            kings.Add(new Pair<int, int>(playerData.Value.TeamID, playerData.Value.PlayerID));

            if (myPlayer == playerData.Value.PlayerID)
            {
                myTeam = playerData.Value.TeamID;
				NetworkingManager.instance.player = createdPlayer;
				GameObject.Find("Main Camera").GetComponent<FollowCamera>().target = NetworkingManager.instance.player.transform;
                if (GameObject.Find("Minimap Camera") != null)
					GameObject.Find("Minimap Camera").GetComponent<FollowCamera>().target = NetworkingManager.instance.player.transform;
				NetworkingManager.instance.player.AddComponent<Movement>();
				NetworkingManager.instance.player.AddComponent<Attack>();
                //Created our player
            }
            else {
                createdPlayer.AddComponent<PlayerReceiveUpdates>();
                createdPlayer.GetComponent<PlayerReceiveUpdates>().playerID = playerData.Value.PlayerID;
                //Created another player
            }
        }

        foreach (var king in kings)
        {
            if (king.first == myTeam)
                GameData.AllyKingID = king.second;
            else
                GameData.EnemyKingID = king.second;
        }
    }

    private void gameEnd(JSONClass packet)
    {
        if  (packet["TeamID"] == GameData.MyPlayer.TeamID)
        {
            GameWon();
        } else
        {
            GameLost();
        }
    }

    private void GameWon()
    {
        Debug.Log("You have won");
    }

    private void GameLost()
    {
        Debug.Log("You have lost");
    }
    
}
