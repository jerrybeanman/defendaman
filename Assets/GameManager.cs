using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using System;
using System.Linq;

//Carson
public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public Transform AI;

	[System.Serializable]
	public class GunnerVision
	{
		public Transform lightSourceFOV;
		public Transform hiderFOV;
		public Transform lightSourcePeripheral;
		public Transform hiderPeripheral;
	}

	[System.Serializable]
	public class NinjaVision
	{
		public Transform lightSourceFOV;
		public Transform hiderFOV;
		public Transform lightSourcePeripheral;
		public Transform hiderPeripheral;
	}

	[System.Serializable]
	public class MageVision
	{
		public Transform lightSourcePeripheral;
		public Transform hiderPeripheral;
	}

	public GunnerVision gunnerVision;
	public NinjaVision ninjaVision;
	public MageVision mageVision;
    public Transform playerType;
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
            StartGame(GameData.Seed);
        }
    }

    public void PlayerTookDamage(int playerID, float newHP, BaseClass.PlayerBaseStat ClassStat)
    {
        var damage = (ClassStat.CurrentHp - newHP);
        if (GameData.MyPlayer.PlayerID == playerID)
        {
            HUD_Manager.instance.UpdatePlayerHealth(-(damage/ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0) {
                PlayerDied();
            } else {
                if (damage > 0)
                {
                    HUD_Manager.instance.colourizeScreen.PlayerHurt();
                } else if (damage < 0)
                {
                    HUD_Manager.instance.colourizeScreen.PlayerHealed();
                }
            }
        }
    }

    public void PlayerDied()
    {
        GameData.EnemyTeamKillCount++;
        NetworkingManager.send_next_packet(DataType.Killed, GameData.MyPlayer.PlayerID, new List<Pair<string, string>>(), Protocol.TCP);
        GameData.PlayerPosition.Remove(GameData.MyPlayer.PlayerID);
        GameData.GameState = GameState.Dying;
        HUD_Manager.instance.colourizeScreen.PlayerDied();
        Debug.Log("You have died");
        instance.player = null;
        if (GameData.MyPlayer.PlayerID == GameData.AllyKingID)
        {
            NetworkingManager.TCP_Send(NetworkingManager.send_next_packet(DataType.Lobby, 10, new List<Pair<string, string>>(), Protocol.NA), 512);
            GameLost();
        }

        if (GameData.MyPlayer.PlayerID == GameData.EnemyKingID)
        {
            NetworkingManager.TCP_Send(NetworkingManager.send_next_packet(DataType.Lobby, 10, new List<Pair<string, string>>(), Protocol.NA), 512);
            GameWon();
        }
    }

    public void StartGame(int seed)
    {
        int myPlayer = GameData.MyPlayer.PlayerID;
        int myTeam = GameData.MyPlayer.TeamID;
        List<Pair<int, int>> kings = new List<Pair<int, int>>();

        /*var createdAI1 = ((Transform)Instantiate(AI, new Vector3(45, 30, -10), Quaternion.identity)).gameObject;
        var createdAI2 = ((Transform)Instantiate(AI, new Vector3(55, 10, -10), Quaternion.identity)).gameObject;
        var createdAI3 = ((Transform)Instantiate(AI, new Vector3(85, 10, -10), Quaternion.identity)).gameObject;
        var createdAI4 = ((Transform)Instantiate(AI, new Vector3(75, 55, -10), Quaternion.identity)).gameObject;
        var createdAI5 = ((Transform)Instantiate(AI, new Vector3(70, 40, -10), Quaternion.identity)).gameObject;*/


        NetworkingManager.instance.update_data(NetworkingManager.GenerateMapInJSON(seed));
        

        foreach (var playerData in GameData.LobbyData.OrderBy(x => x.Key))
        {
			var createdPlayer = ((Transform)Instantiate(playerType, new Vector3(GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].first, GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].second, -10), Quaternion.identity)).gameObject;

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

            createdPlayer.GetComponent<BaseClass>().team = playerData.Value.TeamID;
            createdPlayer.GetComponent<BaseClass>().playerID = playerData.Value.PlayerID;

            //if (playerData.King) //Uncomment this one line when kings are in place
            kings.Add(new Pair<int, int>(playerData.Value.TeamID, playerData.Value.PlayerID));

            if (myPlayer == playerData.Value.PlayerID)
            {
                myTeam = playerData.Value.TeamID;
				player = createdPlayer;
				GameObject.Find("Main Camera").GetComponent<FollowCamera>().target = player.transform;
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

				switch (GameData.LobbyData[playerData.Value.PlayerID].ClassType) {
				case ClassType.Gunner:

					// These are the FOV & peripheral vision occlusion masks
					var lightingGunnerFOV = ((Transform)Instantiate(gunnerVision.lightSourceFOV, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					lightingGunnerFOV.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					lightingGunnerFOV.GetComponent<RotateWithPlayer>().target = createdPlayer.transform;
					lightingGunnerFOV.transform.Translate(0, 0, 8);
					// Send the gunner a reference to this lightsource for their special attack
                    DynamicLight dl = lightingGunnerFOV.gameObject.GetComponent<DynamicLight>();
                    createdPlayer.GetComponent<GunnerClass>().FOVCone = dl;

					var lightingGunnerPeripheral = ((Transform)Instantiate(gunnerVision.lightSourcePeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					lightingGunnerPeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					lightingGunnerPeripheral.transform.Translate(0, 0, 8);

					// These are the FOV & peripheral vision stencil masks
					var hiderLayerGunnerFOV = ((Transform)Instantiate(gunnerVision.hiderFOV, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					hiderLayerGunnerFOV.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					hiderLayerGunnerFOV.GetComponent<RotateWithPlayer>().target = createdPlayer.transform;
					hiderLayerGunnerFOV.transform.Translate(0, 0, 8);
					// Send the gunner a reference to this lightsource for their special attack
					createdPlayer.GetComponent<GunnerClass>().FOVConeHidden = hiderLayerGunnerFOV.gameObject.GetComponent<DynamicLight>();
					
					var hiderLayerGunnerPeripheral = ((Transform)Instantiate(gunnerVision.hiderPeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					hiderLayerGunnerPeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					hiderLayerGunnerPeripheral.transform.Translate(0, 0, 8);

					if (myPlayer == playerData.Value.PlayerID)
					{
						// shadow overlay square
						var shadows = ((Transform)Instantiate(shadowoverlay, createdPlayer.transform.position, Quaternion.identity)).gameObject;
						// set to child of FOV lighting
						shadows.transform.parent = lightingGunnerFOV.transform;
						// move below everything
						shadows.transform.Translate(0, 0, 11);
					}

					break;
				case ClassType.Ninja:

					// These are the FOV & peripheral vision occlusion masks
					var lightingNinjaFOV = ((Transform)Instantiate(ninjaVision.lightSourceFOV, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					lightingNinjaFOV.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					lightingNinjaFOV.GetComponent<RotateWithPlayer>().target = createdPlayer.transform;
					lightingNinjaFOV.transform.Translate(0, 0, 8);
					
					var lightingNinjaPeripheral = ((Transform)Instantiate(ninjaVision.lightSourcePeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					lightingNinjaPeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					lightingNinjaPeripheral.transform.Translate(0, 0, 8);
					
					// These are the FOV & peripheral vision stencil masks
					var hiderLayerNinjaFOV = ((Transform)Instantiate(ninjaVision.hiderFOV, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					hiderLayerNinjaFOV.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					hiderLayerNinjaFOV.GetComponent<RotateWithPlayer>().target = createdPlayer.transform;
					hiderLayerNinjaFOV.transform.Translate(0, 0, 8);
					
					var hiderLayerNinjaPeripheral = ((Transform)Instantiate(ninjaVision.hiderPeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					hiderLayerNinjaPeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					hiderLayerNinjaPeripheral.transform.Translate(0, 0, 8);

					if (myPlayer == playerData.Value.PlayerID)
					{
						// shadow overlay square
						var shadows = ((Transform)Instantiate(shadowoverlay, createdPlayer.transform.position, Quaternion.identity)).gameObject;
						// set to child of FOV lighting
						shadows.transform.parent = lightingNinjaFOV.transform;
						// move below everything
						shadows.transform.Translate(0, 0, 11);
					}

					break;
				case ClassType.Wizard:

					var lightingMagePeripheral = ((Transform)Instantiate(mageVision.lightSourcePeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					lightingMagePeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					lightingMagePeripheral.transform.Translate(0, 0, 8);
					
					var hiderLayerMagePeripheral = ((Transform)Instantiate(mageVision.hiderPeripheral, createdPlayer.transform.position, Quaternion.identity)).gameObject;
					hiderLayerMagePeripheral.GetComponent<LightFollowPlayer>().target = createdPlayer.transform;
					hiderLayerMagePeripheral.transform.Translate(0, 0, 8);

					if (myPlayer == playerData.Value.PlayerID)
					{
						// shadow overlay square
						var shadows = ((Transform)Instantiate(shadowoverlay, createdPlayer.transform.position, Quaternion.identity)).gameObject;
						// set to child of FOV lighting
						shadows.transform.parent = lightingMagePeripheral.transform;
						// move below everything
						shadows.transform.Translate(0, 0, 11);
					}

					break;
				default:
					break;
				}

                
            } else { //Hide enemies
				// Fetch the stencil masked material
				Material hiddenMat = (Material)Resources.Load("Stencil_01_Diffuse Sprite", typeof(Material));
				// Move enemy behind stencil mask
				createdPlayer.transform.Translate(0,0,9);
				// set the enemy, hpFrame & hpBar materials to stencil masked and layer to hidden
				createdPlayer.GetComponent<SpriteRenderer> ().material = hiddenMat;
				SetLayerRecursively(createdPlayer, "HiddenThings");
				SetMaterialRecursively(createdPlayer, hiddenMat);
				
			}
        }

		NetworkingManager.StartGame();
    }

	/*----------------------------------------------------------------------------
    --	Recursively sets all child object's material to mat
    --
    --	Interface:  void SetMaterialRecursively(GameObject obj, Material mat)
    --					-GameObject obj: Base / parent game object
    --					-Material mat  : Target material
    --
    --	progrbammer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void SetMaterialRecursively(GameObject obj, Material mat)
	{
		SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
		if(sr != null)
			sr.material = mat;

		// Goes through child
		foreach( Transform child in obj.transform )
		{
			SetMaterialRecursively(child.gameObject, mat);
		}
	}

	/*----------------------------------------------------------------------------
    --	Recursively sets all child object's material to mat
    --
    --	Interface:  void SetMaterialRecursively(GameObject obj, Material mat)
    --					-GameObject obj: Base / parent game object
    --					-Material mat  : Target material
    --
    --	progrbammer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void SetLayerRecursively(GameObject obj, string name)
	{
		obj.layer = LayerMask.NameToLayer(name);
		
		foreach( Transform child in obj.transform )
		{
			SetLayerRecursively( child.gameObject, name);
		}
	}

    public void GameWon()
    {
        GameData.GameState = GameState.Won;
		HUD_Manager.instance.winScreen.Parent.gameObject.SetActive(true);
		HUD_Manager.instance.winScreen.Parent.SetTrigger("Play");
		HUD_Manager.instance.winScreen.Child.SetTrigger("Play");
    }

    public void GameLost()
    {
        GameData.GameState = GameState.Lost;
        HUD_Manager.instance.loseScreen.Parent.gameObject.SetActive(true);
        HUD_Manager.instance.loseScreen.Parent.SetTrigger("Play");
        HUD_Manager.instance.loseScreen.Child.SetTrigger("Play");
    }
	

   	public void ReturnToMenu()
    {
        NetworkingManager.instance.ResetConnections();
        NetworkingManager.ClearSubscriptions();
        GameData.LobbyData.Clear();
        GameData.aiSpawn = new Pair<int, int>(10, 10);
        GameData.AllyKingID = -1;
        GameData.EnemyKingID = -1;
        GameData.AllyTeamKillCount = 0;
        GameData.EnemyTeamKillCount = 0;
        GameData.GameStart = false;
        GameData.InputBlocked = false;
        GameData.IP = "192.168.0.";
        GameData.ItemCollided = false;
        GameData.MouseBlocked = false;
        GameData.Seed = 0;
        GameData.TeamSpawnPoints.Clear();
        GameData.PlayerPosition.Clear();
        GameData.MyPlayer = new PlayerData();
        GameData.GameState = GameState.Playing;
        GameData.CurrentTheme = Themes.Grass;
        Destroy(NetworkingManager.instance);
        Destroy(Inventory.instance);
        Destroy(WorldItemManager.Instance);
        Application.LoadLevel("MenuScene");
    }
}
