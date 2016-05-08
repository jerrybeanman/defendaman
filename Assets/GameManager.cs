using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using System;
using System.Linq;

/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    GameManager.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--
--  DATE:           March 10th, 2016
--
--  REVISIONS:      March 10th, 2016: Created class from refactoring NetworkingManager code [Carson]
--                  March 20th, 2016: Added vision logic on game creation [Micah]
--                  March 25th, 2016: Added logic to handle game ending, playing dying and such [Carson]
--                  March 28th, 2016: Added unique vision by class [Jerry]
--                  April 3rd, 2016: Added end-game visual logic [Dhivya]
--
--  DESIGNERS:      Carson Roscoe
--
--  PROGRAMMER:     Carson Roscoe / Dhivya Manohar / Jerry Jia / Micah Willems
--
--  NOTES:
--  This class is a singleton class used to handle general game related concepts that
--  don't have a proper home, for example handling game instantiation, handling how the game
--  ends, the visual side of getting hurt, etc.
---------------------------------------------------------------------------------------*/
public class GameManager : MonoBehaviour {
    //Instance of GameManager used for singleton pattern
    public static GameManager instance;
   
    //Used for Gunner class' vision data. Hooked up in Unity editor
	[System.Serializable]
	public class GunnerVision {
		public Transform lightSourceFOV;
		public Transform hiderFOV;
		public Transform lightSourcePeripheral;
		public Transform hiderPeripheral;
	}

    //Used for Ninja class' vision data. Hooked up in Unity editor
    [System.Serializable]
	public class NinjaVision {
		public Transform lightSourceFOV;
		public Transform hiderFOV;
		public Transform lightSourcePeripheral;
		public Transform hiderPeripheral;
	}

    //Used for Mage class' vision data. Hooked up in Unity editor
    [System.Serializable]
	public class MageVision {
		public Transform lightSourcePeripheral;
		public Transform hiderPeripheral;
	}

    //Gunner's vision data
	public GunnerVision gunnerVision;
    
    //Ninja's vision data
	public NinjaVision ninjaVision;

    //Mage's vision data
	public MageVision mageVision;

    //Our players transform
    public Transform playerType;

    //The shadow overlay for vision system
	public Transform shadowoverlay;

    //Our players GameObject
    public GameObject player;

    //Flag used to check if we are in testing (Windows) or not
    private bool testing = false;

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Awake
    --
    -- DATE: March 10th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void Awake(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Awake is invoked before Start in the creation of a script. In this case, it is used to initiate our singleton pattern
    -- by setting instance if it is not set, and if it is set destroying this script since we do not want two singletons
    -- of the same object.
    ---------------------------------------------------------------------------------------------------------------------*/
    void Awake() {
        //Check if instance already exists
        if (instance == null)
            //if not, set instance to this
            instance = this;
        //If instance already exists and it's not this:
        else if (instance != this)
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager. 
            Destroy(gameObject);

        //If we didnt get here from the lobby, we are doing non-networking testing
        if (GameObject.Find("NetworkManager") == null)  {
            testing = true;
            gameObject.AddComponent<NetworkingManager>();
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Start
    --
    -- DATE: March 10th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void Start(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Start is called after Awake during the instantiation of all Unity game objects. We are simply using to chec
    -- that if we are not testing, invoke the game via the seed found from Lobby.
    ---------------------------------------------------------------------------------------------------------------------*/
    void Start() {
        if (!testing) {
            StartGame(GameData.Seed);
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: PlayerTookDamage
    --
    -- DATE: March 25th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void PlayerTookDamage(int playerID of the one taking data
    --                                  float newHP of the player after having taken data
    --                                  PlayerBaseStat class stats of the player in question)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Handles what to do when someone takes damage in the game in regards to making the screen flash
    -- and killing our player if our health is below/equal to zero.
    ---------------------------------------------------------------------------------------------------------------------*/
    public void PlayerTookDamage(int playerID, float newHP, BaseClass.PlayerBaseStat ClassStat) {
        var damage = (ClassStat.CurrentHp - newHP);
        if (GameData.MyPlayer.PlayerID == playerID) {
            HUD_Manager.instance.UpdatePlayerHealth(-(damage/ClassStat.MaxHp));
            if (ClassStat.CurrentHp <= 0) {
                PlayerDied();
            } else {
                if (damage > 0) {
                    HUD_Manager.instance.colourizeScreen.PlayerHurt();
                } else if (damage < 0) {
                    HUD_Manager.instance.colourizeScreen.PlayerHealed();
                }
            }
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
   -- METHOD: PlayerDied
   --
   -- DATE: March 25th, 2016
   --
   -- REVISIONS: N/A
   --
   -- DESIGNER: Carson Roscoe
   --
   -- PROGRAMMER: Carson Roscoe
   --
   -- INTERFACE: void PlayerDied(void)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Handles our player dying, sending out a packet telling everyone we have died, sending to the server a "Game Over"
   -- packet if our player is the aman, colourizing the screen and overall cleanup in regards to our player.
   ---------------------------------------------------------------------------------------------------------------------*/
    public void PlayerDied() {
        GameData.EnemyTeamKillCount++;
        NetworkingManager.send_next_packet(DataType.Killed, GameData.MyPlayer.PlayerID, new List<Pair<string, string>>(), Protocol.TCP);
        GameData.PlayerPosition.Remove(GameData.MyPlayer.PlayerID);
        GameData.GameState = GameState.Dying;
        HUD_Manager.instance.colourizeScreen.PlayerDied();
        Debug.Log("You have died");
        instance.player = null;
        if (GameData.MyPlayer.PlayerID == GameData.AllyKingID) {
            NetworkingManager.TCP_Send(NetworkingManager.send_next_packet(DataType.Lobby, 10, new List<Pair<string, string>>(), Protocol.NA), 512);
            GameLost();
        }

        if (GameData.MyPlayer.PlayerID == GameData.EnemyKingID) {
            NetworkingManager.TCP_Send(NetworkingManager.send_next_packet(DataType.Lobby, 10, new List<Pair<string, string>>(), Protocol.NA), 512);
            GameWon();
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
   -- METHOD: StartGame
   --
   -- DATE: March 10th, 2016
   --
   -- REVISIONS: March 10th, 2016: Created from refactored code in NetworkingManager
   --            March 20th, 2016: Add lighting to players and hide enemies
   --
   -- DESIGNER: Carson Roscoe
   --
   -- PROGRAMMER: Carson Roscoe / Micah Willems
   --
   -- INTERFACE: void StartGame(int seed to generate world from)
   --
   -- RETURNS: void
   --
   -- NOTES:
   -- Handles the physical creation of the game once we move over to the game scene. All the logic from
   -- creating our players, generating the map, assigning classes, teams, everything is done here.
   ---------------------------------------------------------------------------------------------------------------------*/
    public void StartGame(int seed) {
        int myPlayer = GameData.MyPlayer.PlayerID;
        int myTeam = GameData.MyPlayer.TeamID;
        List<Pair<int, int>> kings = new List<Pair<int, int>>();

        //Generate map "packet" and force us to "receive" the packet locally to generate world
        NetworkingManager.instance.update_data(NetworkingManager.GenerateMapInJSON(seed));
        
        //Loop through every player in data from the Lobby and create that player
        foreach (var playerData in GameData.LobbyData.OrderBy(x => x.Key)) {
            //Create the player at a spawn position determined by whatever team that player is on
			var createdPlayer = ((Transform)Instantiate(playerType, new Vector3(GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].first, GameData.TeamSpawnPoints[playerData.Value.TeamID - 1].second, -10), Quaternion.identity)).gameObject;

            //Assign the new player a class
            switch (playerData.Value.ClassType) {
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

            kings.Add(new Pair<int, int>(playerData.Value.TeamID, playerData.Value.PlayerID));

            //If the created player is our player, make the camera follow it and give it movement script
            if (myPlayer == playerData.Value.PlayerID) {
                myTeam = playerData.Value.TeamID;
				player = createdPlayer;
				GameObject.Find("Main Camera").GetComponent<FollowCamera>().target = player.transform;
				if (GameObject.Find("Minimap Camera") != null)
					GameObject.Find("Minimap Camera").GetComponent<FollowCamera>().target = player.transform;
				player.AddComponent<Movement>();
				player.AddComponent<Attack>();
                //Created our player
            } else {
                //Otherwise, this is another player. Hook them up to receive updates from server updates
                createdPlayer.AddComponent<PlayerReceiveUpdates>();
                createdPlayer.GetComponent<PlayerReceiveUpdates>().playerID = playerData.Value.PlayerID;
                //Created another player
            }

            //If they are on my team, add a light
            if (myTeam == playerData.Value.TeamID) {
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
            //Otherwise, they are not on our team. They are enemies.
            } else { 
                //Hide enemies
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

        //Invoke StartGame in NetworkingManager so we start listening for game updates
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

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: GameWon
    --
    -- DATE: March 25th, 2016
    --
    -- REVISIONS: March 25th, 2016: Created and hooked up game to call it when needed
    --            April 3rd, 2016: Added functionality of end-game
    --
    -- DESIGNER: Carson Roscoe / Dhivya Manohar
    --
    -- PROGRAMMER: Carson Roscoe / Dhivya Manohar
    --
    -- INTERFACE: void GameWon(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Invoked when the other teams Aman is deemed dead
    ---------------------------------------------------------------------------------------------------------------------*/
    public void GameWon() {
        StartCoroutine(sleep());
        GameData.GameState = GameState.Won;
		HUD_Manager.instance.winScreen.Parent.gameObject.SetActive(true);
		HUD_Manager.instance.winScreen.Parent.SetTrigger("Play");
		HUD_Manager.instance.winScreen.Child.SetTrigger("Play");
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: GameLost
    --
    -- DATE: March 25th, 2016
    --
    -- REVISIONS: March 25th, 2016: Created and hooked up game to call it when needed
    --            April 3rd, 2016: Added functionality of end-game
    --
    -- DESIGNER: Carson Roscoe / Dhivya Manohar
    --
    -- PROGRAMMER: Carson Roscoe / Dhivya Manohar
    --
    -- INTERFACE: void GameLost(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Invoked when our teams Aman is deemed dead
    ---------------------------------------------------------------------------------------------------------------------*/
    public void GameLost()
    {
        StartCoroutine(sleep());
        GameData.GameState = GameState.Lost;
        HUD_Manager.instance.loseScreen.Parent.gameObject.SetActive(true);
        HUD_Manager.instance.loseScreen.Parent.SetTrigger("Play");
        HUD_Manager.instance.loseScreen.Child.SetTrigger("Play");
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: sleep
    --
    -- DATE: April 3rd, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Dhivya Manohar
    --
    -- PROGRAMMER: Dhivya Manohar
    --
    -- INTERFACE: IEnumerator sleep(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Wait 5 seconds and then invoke all cleanup code and close the game.
    ---------------------------------------------------------------------------------------------------------------------*/
    IEnumerator sleep()
    {
        yield return new WaitForSeconds(5);
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        NetworkingManager.instance.ResetConnections();
        NetworkingManager.ClearSubscriptions();
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: ReturnToMenu
    --
    -- DATE: March 25th, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Carson Roscoe
    --
    -- PROGRAMMER: Carson Roscoe
    --
    -- INTERFACE: void ReturnToMenu(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Cleanup all data & networking code since the game is over when this is invoked. Essentially reset everything
    ---------------------------------------------------------------------------------------------------------------------*/
    public void ReturnToMenu() {
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
        //Application.LoadLevel("MenuScene");
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: ExitGame
    --
    -- DATE: April 3rd, 2016
    --
    -- REVISIONS: N/A
    --
    -- DESIGNER: Dhivya Manohar
    --
    -- PROGRAMMER: Dhivya Manohar
    --
    -- INTERFACE: void ExitGame(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Wrapper around killing the game when the game is being exited
    ---------------------------------------------------------------------------------------------------------------------*/
    public void ExitGame() {
        System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}
