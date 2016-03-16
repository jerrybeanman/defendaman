using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using System;

//Carson
public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public Transform AI;

    public Transform playerType;
    public Transform lightSource;
	public Transform shadowoverlay;
    public GameObject player;
    private bool testing = false;

    public enum LobbyData { GameEnd = 1, Disconnected = 2 }

    void Awake()
    {
        if (instance == null)               //Check if instance already exists
            instance = this;                //if not, set instance to this
        else if (instance != this)          //If instance already exists and it's not this:
            Destroy(gameObject);            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager. 

        if (GameObject.Find("NetworkManager") == null) //If we didnt get here from the lobby, we are doing non-networking testing
        {
            testing = true;
            gameObject.AddComponent<NetworkingManager>();
        }
    }

    void Start()
    {
        if (!testing)
        {
            NetworkingManager.Subscribe(gameEnd, DataType.Lobby, (int)LobbyData.GameEnd);
            StartGame(GameData.Seed);
        }
    }

    public void PlayerTookDamage(int playerID, float damage, BaseClass.PlayerBaseStat ClassStat)
    {
        if (GameData.MyPlayer.PlayerID == playerID)
        {
            HUD_Manager.instance.UpdatePlayerHealth(-(damage / ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0)
                PlayerDied();
            else
                ColourizeScreen.instance.PlayerHurt();
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
        ColourizeScreen.instance.PlayerDied();
        Debug.Log("You have died");
    }

    public void StartGame(int seed)
    {
        int myPlayer = GameData.MyPlayer.PlayerID;
        int myTeam = 0;
        List<Pair<int, int>> kings = new List<Pair<int, int>>();
        
        int x = 0;
        Vector2 check = new Vector2();

        /*while (x < 20)
        {
            System.Random rnd = new System.Random();
            int xCoord = rnd.Next(1,99);
            int yCoord = rnd.Next(1,99);
            check.x = xCoord;
            check.y = yCoord;
            RaycastHit2D hit = Physics2D.Raycast(check, Vector2.up, 0.0001f);
            if (hit.collider != null)
            {
                continue;
            }
            x++;
            var createdAI = ((Transform)Instantiate(AI, new Vector3(xCoord,yCoord, -10), Quaternion.identity)).gameObject;
        }*/
        var createdAI = ((Transform)Instantiate(AI, new Vector3(40, 40, -10), Quaternion.identity)).gameObject;

        NetworkingManager.instance.update_data(NetworkingManager.GenerateMapInJSON(seed));
        
        foreach (var playerData in GameData.LobbyData)
        {
			var createdPlayer = ((Transform)Instantiate(playerType, new Vector3(GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].first, GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].second, -10), Quaternion.identity)).gameObject;

            switch (playerData.Value.ClassType)
            {
                case ClassType.Ninja:
					Debug.Log("Added ninja");
                    createdPlayer.AddComponent<NinjaClass>();
                    break;
				case ClassType.Gunner:
					Debug.Log("Added gunner");
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

			Debug.Log(myTeam.ToString());
            // Add FOV to all ally team members
			if (myTeam == playerData.Value.TeamID || myTeam == 0) { // myTeam == 0 is a bugfix until player has teamID of 1
				Debug.Log ("Team Member");
				Transform hpFrame = createdPlayer.transform.GetChild(0);
				Transform hpBar = hpFrame.transform.GetChild(0);
				createdPlayer.layer = LayerMask.NameToLayer("Allies");
				hpFrame.gameObject.layer = LayerMask.NameToLayer("Allies");
				hpBar.gameObject.layer = LayerMask.NameToLayer("Allies");
				var lighting = ((Transform)Instantiate(lightSource, createdPlayer.transform.position, Quaternion.identity)).gameObject;
				lighting.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
				lighting.GetComponent<RotateWithPlayer>().target = createdPlayer.transform;
				lighting.transform.Translate(0,0,9);
				if (myPlayer == playerData.Value.PlayerID) {
					var shadows = ((Transform)Instantiate(shadowoverlay, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					shadows.transform.parent = lighting.transform;
					shadows.transform.Translate(0,0,11);
				}
			}

            createdPlayer.GetComponent<BaseClass>().team = playerData.Value.TeamID;
            createdPlayer.GetComponent<BaseClass>().playerID = playerData.Value.PlayerID;

            //if (playerData.King) //Uncomment this one line when kings are in place
            kings.Add(new Pair<int, int>(playerData.Value.TeamID, playerData.Value.PlayerID));

            if (myPlayer == playerData.Value.PlayerID)
            {
                myTeam = playerData.Value.TeamID;
				player = createdPlayer;
				GameObject.Find("Main Camera").GetComponent<FollowCamera>().target = player.transform;
				GameObject.Find("Camera FOV").GetComponent<FollowCamera>().target = player.transform;
                if (GameObject.Find("Minimap Camera") != null)
					GameObject.Find("Minimap Camera").GetComponent<FollowCamera>().target = player.transform;
				player.AddComponent<Movement>();
				player.AddComponent<Attack>();
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

		NetworkingManager.StartGame();
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
