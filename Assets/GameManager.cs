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
    public Transform lightSourceFOV;
	public Transform hiderFOV;
	public Transform lightSourcePeripheral;
	public Transform hiderPeripheral;
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
            if (ClassStat.CurrentHp <= 0) {
                PlayerDied();
            } else {
                if (damage > 0)
                {
                    ColourizeScreen.instance.PlayerHurt();
                } else if (damage < 0)
                {
                    ColourizeScreen.instance.PlayerHealed();
                }
            }
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
        NetworkingManager.send_next_packet(DataType.Killed, GameData.MyPlayer.PlayerID, new List<Pair<string, string>>(), Protocol.UDP);
        GameData.GameState = GameState.Dying;
        ColourizeScreen.instance.PlayerDied();
        Debug.Log("You have died");
        GameData.MyPlayer = null;
    }

    public void StartGame(int seed)
    {
        int myPlayer = GameData.MyPlayer.PlayerID;
        int myTeam = GameData.MyPlayer.TeamID;
        List<Pair<int, int>> kings = new List<Pair<int, int>>();
       
       /* var createdAI1 = ((Transform)Instantiate(AI, new Vector3(45, 30, -10), Quaternion.identity)).gameObject;
        var createdAI2 = ((Transform)Instantiate(AI, new Vector3(55, 10, -10), Quaternion.identity)).gameObject;
        var createdAI3 = ((Transform)Instantiate(AI, new Vector3(85, 10, -10), Quaternion.identity)).gameObject;
        var createdAI4 = ((Transform)Instantiate(AI, new Vector3(75, 55, -10), Quaternion.identity)).gameObject;
        var createdAI5 = ((Transform)Instantiate(AI, new Vector3(70, 40, -10), Quaternion.identity)).gameObject;*/


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
				GameObject.Find("Camera Enemies").GetComponent<FollowCamera>().target = player.transform;
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

            //If they are on my team, add a light
            if (myTeam == playerData.Value.TeamID)
            {
				// Get the player/teammember's hpFrame and bar
                Transform hpFrame = createdPlayer.transform.GetChild(0);
                Transform hpBar = hpFrame.transform.GetChild(0);
                createdPlayer.layer = LayerMask.NameToLayer("Allies");
                hpFrame.gameObject.layer = LayerMask.NameToLayer("Allies");
                hpBar.gameObject.layer = LayerMask.NameToLayer("Allies");
				// These are the FOV & peripheral vision occlusion masks
                var lightingFOV = ((Transform)Instantiate(lightSourceFOV, createdPlayer.transform.position, Quaternion.identity)).gameObject;
                lightingFOV.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
                lightingFOV.GetComponent<RotateWithPlayer>().target = createdPlayer.transform;
                lightingFOV.transform.Translate(0, 0, 8);
                var lightingPeripheral = ((Transform)Instantiate(lightSourcePeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
                lightingPeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
                lightingPeripheral.transform.Translate(0, 0, 8);
				// These are the FOV & peripheral vision stencil masks
				var hiderLayerFOV = ((Transform)Instantiate(hiderFOV, createdPlayer.transform.position, Quaternion.identity)).gameObject;
				hiderLayerFOV.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
				hiderLayerFOV.GetComponent<RotateWithPlayer>().target = createdPlayer.transform;
				hiderLayerFOV.transform.Translate(0, 0, 8);
				var hiderLayerPeripheral = ((Transform)Instantiate(hiderPeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
				hiderLayerPeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
				hiderLayerPeripheral.transform.Translate(0, 0, 8);
                if (myPlayer == playerData.Value.PlayerID)
                {
					// shadow overlay square
                    var shadows = ((Transform)Instantiate(shadowoverlay, createdPlayer.transform.position, Quaternion.identity)).gameObject;
                    // set to child of FOV lighting
					shadows.transform.parent = lightingFOV.transform;
					// move below everything
                    shadows.transform.Translate(0, 0, 11);
                }
            } else { //Hide enemies
				// Get the hpFrame & Bar for enemy players
				Transform hpFrame = createdPlayer.transform.GetChild(0);
				Transform hpBar = hpFrame.transform.GetChild(0);
				// Fetch the stencil masked material
				Material hiddenMat = (Material)Resources.Load("Stencil_01_Diffuse Sprite", typeof(Material));
				// Move enemy behind stencil mask
				createdPlayer.transform.Translate(0,0,9);
				// set the enemy, hpFrame & hpBar materials to stencil masked and layer to hidden
				createdPlayer.GetComponent<SpriteRenderer> ().material = hiddenMat;
				createdPlayer.layer = LayerMask.NameToLayer("HiddenThings");
				hpFrame.gameObject.GetComponent<SpriteRenderer>().material = hiddenMat;
				hpFrame.gameObject.layer = LayerMask.NameToLayer("HiddenThings");
				hpBar.gameObject.GetComponent<SpriteRenderer>().material = hiddenMat;
				hpBar.gameObject.layer = LayerMask.NameToLayer("HiddenThings");
			}
        }

        foreach (var king in kings)
        {
            if (king.first == myTeam)
            {
                GameData.AllyKingID = king.second;
            }
            else {
                GameData.EnemyKingID = king.second;
            }
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
        GameData.GameState = GameState.Won;
        Debug.Log("You have won");
    }

    private void GameLost()
    {
        GameData.GameState = GameState.Lost;
        Debug.Log("You have lost");
    }

    void OnGUI()
    {
        switch (GameData.GameState)
        {
            case GameState.Won:
                GUI.Label(new Rect(Screen.width / 2 - 20, Screen.height / 2 - 20, Screen.width / 2 + 20, Screen.height / 2 + 20), "You won!");
                Invoke("ReturnToMenu", 2f);
                break;
            case GameState.Lost:
                GUI.Label(new Rect(Screen.width / 2 - 20, Screen.height / 2 - 20, Screen.width / 2 + 20, Screen.height / 2 + 20), "You lost!");
                Invoke("ReturnToMenu", 2f);
                break;
            default:
                break;
        }
    }

    void ReturnToMenu()
    {
        NetworkingManager.ClearSubscriptions();
        GameData.LobbyData.Clear();
        GameData.PlayerPosition.Clear();
        GameData.GameStart = false;
        GameData.TeamSpawnPoints.Clear();
        Destroy(instance);
        Application.LoadLevel("MenuScene");
    }
}
