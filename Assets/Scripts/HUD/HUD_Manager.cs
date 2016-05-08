using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Globalization;
using SimpleJSON;
using System.Collections.Generic;


public enum UICode
{
	Chat 				= 1,
	BuildingCreation 	= 2,
	BuildingDestruction = 3
}

public class HUD_Manager : MonoBehaviour 
{
	// Singleton object
	public static HUD_Manager 	instance; 

	// HUD elements
	public Currency				currency;
	public PlayerProfile 		playerProfile;
	public AllyKing				allyKing;
	public EnemyKing			enemyKing;
	public MainSkill			mainSkill;
	public SubSkill				subSkill;
	public PassiveSkill			passiveSkill;
	public Chat					chat;
	public Shop					shop;	
	public PlayerDP				playerDp;
	public ChargeBar			chargeBar;
	public Stamina				stamina;
	public WinScreen			winScreen;
    public LoseScreen           loseScreen;
	public Text					upgradeAmount;
	public Text					timer;
	public GameObject			placementRange;
	public GameObject			statsPanel;
	public ColourizeScreen		colourizeScreen;
	
	// Need to reference MapManager to manipulate its building lists
	public MapManager			mapManager;

	// Where the mouse is currently at
	private Vector3 			currFramePosition;

	// Indicates wheter or not chat is currently selected 
	private bool InputSelected = false;
	

	// Indicates if an item has been bought or not
	bool ItemBought = false;

	// Singleton pattern
	void Awake()
	{
		//Check if instance already exists
		if (instance == null)				
			//if not, set instance to this
			instance = this;				
		//If instance already exists and it's not this:
		else if (instance != this)			
			//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
			Destroy(gameObject);   			
	}
	
	// Called on start of game
	void Start()
	{
		// Subscribe our chat system to the TCP network
		NetworkingManager.Subscribe(UpdateChatCallBack, DataType.UI, (int)UICode.Chat);
		// Subscribe building creation to the TCP network
		NetworkingManager.Subscribe(UpdateBuildingCreationCallBack, DataType.UI, (int)UICode.BuildingCreation);
		// Subscribe building destruction to the TCP network
		NetworkingManager.Subscribe(UpdateBuildingDestructionCallBack, DataType.UI, (int)UICode.BuildingDestruction);
		switch(GameData.MyPlayer.ClassType)
		{
			case ClassType.Gunner: 
				playerDp.Container.sprite = playerDp.GunnerDP;
				break;
			case ClassType.Wizard:
				playerDp.Container.sprite = playerDp.MageDP;
				break;
			case ClassType.Ninja:
				playerDp.Container.sprite = playerDp.NinjaDP;
				break;
		}
	}

	// Called once per frame
	void Update()
	{
		if(Input.GetKeyDown(KeyCode.B))
			Buy();

		// If an item has been bought in the shop menu
		if(ItemBought)
		{

			// Have the item hover over our mouse 
			CheckBuildingPlacement(shop.Selected.Building);

			// If the left mouse button is clicked while we have the item hovering
			if(Input.GetKeyDown(KeyCode.Mouse0))
			{
				// Attempt to place the building onto of where our mouse is at
				if(PlaceBuilding(shop.Selected.Building))
					// If success, we deselect the item in our shop
					UnHighlightItem();
			}
		}

		// Check for any inputs for chat
		CheckChatAction();

		// Check if skills are used or not
		CheckSkillStatus();

		CheckStaminaStatus();

		UpdateTimer();
	}


	/*----------------------------------------------------------------------------
    --	Check for any events triggered in the chat box
    --
    --	Interface:  void CheckChatAction()
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void CheckChatAction()
	{
		// If return key has been pressed
		if(Input.GetKeyDown(KeyCode.Return))
		{

			// See if the chat window is currently open
			if(!chat.input.IsInteractable())
			{
				// Block all other keyboard inputs
				GameData.InputBlocked = true;
				// If not then open the chat window
				chat.input.interactable = true;
				chat.input.Select();
				chat.input.ActivateInputField();
			}
			else
			{
				// Unblocck keyboard inputs 
				GameData.InputBlocked = false;
				if (Application.platform == RuntimePlatform.LinuxPlayer)
				{
					// Send the packet, with Team ID, user name, and the message input
					List<Pair<string, string>> packetData = new List<Pair<string, string>>();
					packetData.Add(new Pair<string, string>(NetworkKeyString.TeamID, GameData.MyPlayer.TeamID.ToString()));
					packetData.Add(new Pair<string, string>(NetworkKeyString.UserName, "\"" + GameData.MyPlayer.Username + "\""));
					packetData.Add(new Pair<string, string>(NetworkKeyString.Message, "\"" + chat.input.text + "\""));
					NetworkingManager.send_next_packet(DataType.UI, (int)UICode.Chat, packetData, Protocol.TCP);
				}

				// Clear out the chat window
				chat.input.text = "";

				// Close the window
				chat.input.interactable = false;
			}
		}
	}

	/*----------------------------------------------------------------------------
    --	Check if main and sub skills need to be recharged
    --
    --	Interface:  void CheckSkillStatus()
    --
    --	progrbammer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void CheckSkillStatus()
	{
		// If main skill bar is below full
		if(mainSkill.ProgressBar.fillAmount  < 1)
		{
			// Char it up slowly corresponding to the cool down timer
			mainSkill.ProgressBar.fillAmount += Time.deltaTime / mainSkill.CoolDown;
			mainSkill.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, mainSkill.ProgressBar.fillAmount);
		}

		// If sub skill bar is below full
		if(subSkill.ProgressBar.fillAmount < 1)
		{
			// Char it up slowly corresponding to the cool down timer
			subSkill.ProgressBar.fillAmount += Time.deltaTime / subSkill.CoolDown;
			subSkill.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, subSkill.ProgressBar.fillAmount);
		}
	}

	void CheckStaminaStatus()
	{
		// If main skill bar is below full
		if(stamina.ProgressBar.fillAmount  < 1)
		{
			// Char it up slowly corresponding to the cool down timer
			stamina.ProgressBar.fillAmount += Time.deltaTime / stamina.FillTimer;
			stamina.ProgressBar.fillAmount = Mathf.Lerp(0f, 1f, stamina.ProgressBar.fillAmount);
		}
	}


	/*----------------------------------------------------------------------------
    --	Check for keyboard event on key "m" to open shop menu
    --
    --	Interface:  void CheckShopOption()
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void CheckShopOption()
	{
		if(Input.GetKeyDown(KeyCode.M))
		{
			DisplayShop();
		}
	}

	private BuildingType buildType;
	/*----------------------------------------------------------------------------
    --	Called by the OnClick function of the panle items in the shop menu,
    --	highlight and indicates the currently selected item
    --
    --	Interface:  public void SelectItem(int i)
    --				[i] The item to select
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void SelectItem(int i)
	{
		buildType = (BuildingType)i;
		// If nothing is currently selected
		if(shop.Selected.Option == null)
		{
			// Highlight and select that item
			HighlightItem(i);
		}else
		// If an selected item has been selected
		if(shop.Selected.Option == shop.Items[i-1].Option)
		{
			// Unselect and take off the highlight
			UnHighlightItem();
		}
		// If selecting another item
		else
		{
			// Unselect the previous item
			UnHighlightItem();
			// Hightlight and select that item
			HighlightItem(i);
		}
	}

	/*----------------------------------------------------------------------------
    --	Called by the OnClick function on the buy button in the shop menu
    --	Interface:  public void Buy()
    --
    --	programmer: Jerry Jia, Thomas Yu
    --	@return: void
	------------------------------------------------------------------------------*/
	public void Buy()
	{
		// If nothing is currently selected, do nothing 
		if(shop.Selected.Option != null && !ItemBought && GameData.MyPlayer.Resources[Constants.GOLD_RES] >= shop.Selected.Building.GetComponent<Building>().cost)
		{
			Inventory.instance.UseResources(Constants.GOLD_RES, shop.Selected.Building.GetComponent<Building>().cost);

			// Indicates that an item has been bought
			ItemBought = true;

			// Find where the mouse position is
			Vector3 cursorPosition = new Vector3((int)currFramePosition.x,(int)currFramePosition.y, -2);

			// Assign team attribute so ally cannot damage the building 
			shop.Selected.Building.GetComponent<Building>().team = GameManager.instance.player.GetComponent<BaseClass>().team;

			// Instantitate the selected building at where the mouse is 
			shop.Selected.Building = (GameObject)Instantiate(shop.Selected.Building, cursorPosition, Quaternion.identity);

			// Let the building know that it is currently being placed
			shop.Selected.Building.GetComponent<Building>().placing = true;

			// Make sprite a bit transparent 
			shop.Selected.Building.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 0.3f);

			// Set the collider to false so it cannot collide with player 
			SetAllCollidersStatus(shop.Selected.Building, false);
			shop.Selected.Building.GetComponent<Building>().collidercounter = 0;
			print ("BUILDING value is " + shop.Selected.Building.GetComponent<Building>().collidercounter);
			// Set current rotation value to be zero
			curRot = 0;

			// Display placement area on HUD
			placementRange.SetActive(true);


		}else
		{
			print ("cant buy");
		}
	}


	/*----------------------------------------------------------------------------
    --	Called by the OnClick function on the buy button in the shop menu
    --	Interface:  public void Buy()
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void SetAllCollidersStatus (GameObject go, bool active) 
	{
		foreach(BoxCollider2D c in go.GetComponents<BoxCollider2D> ())
		{
			c.isTrigger = !active;
			if(!active)
				go.layer =  LayerMask.NameToLayer("Default");
			else
				go.layer =  LayerMask.NameToLayer("Minimap");
		}
	}

	private float speed = 0.1f;
	/*----------------------------------------------------------------------------
    --	Called when ItemBought is set to true, have the instantiated building follow
    --  where the mouse cursor
	--	Interface:  private void CheckBuildingPlacement(GameObject buildings)
	--					[building] building that will follow the mouse 
    --
    --	programmer: Jerry Jia, Thomas Yu
    --	@return: void
	------------------------------------------------------------------------------*/
	private void CheckBuildingPlacement(GameObject buildings)
	{
		// Get our mouse position
		currFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		int tempx=(int)currFramePosition.x;
		int tempy=(int)currFramePosition.y;
		currFramePosition.z=0;		
		Vector3 cursorPosition = new Vector3(tempx,tempy,-2);

		// have the building hover with the mouse by changing its transform 
		buildings.transform.position = cursorPosition;

		// Check if it is a valid location to place the building 
		if(!CheckValidLocation(buildings))
		{
			// Set the color transparency 
			shop.Selected.Building.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 0.3f);
		}else
			shop.Selected.Building.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 0.3f);

		if(Input.GetAxis("Mouse ScrollWheel") > 0)
		{
			object[] parms = new object[3]{90, shop.Selected.Building, 0.2f};
			StartCoroutine(Rotate(parms));
		}else
		if(Input.GetAxis("Mouse ScrollWheel") < 0)
		{
			object[] parms = new object[3]{-90, shop.Selected.Building, 0.2f};
			StartCoroutine(Rotate(parms));
		}
	}

	float curRot = 0;
	/*----------------------------------------------------------------------------
    --	Coroutine used to smootly animate an object's rotation
    --
	--	Interface:  IEnumerator Rotate(object[] parms)
	--					-parms[0]: The angle in degrees to rotate by
	--					-parms[1]: The game object to rotate
	--					-parms[2]: Time it takes for the rotation to finish
    --
    --	programmer: Jerry Jia
    --	@return: IEnumerator type for seconds to wait till next instruction
	------------------------------------------------------------------------------*/
	IEnumerator Rotate(object[] parms)
	{
		float elapsedTime = 0.0f;
		Quaternion startingRotation = ((GameObject)parms[1]).transform.rotation; // have a startingRotation as well
		Quaternion targetRotation =  Quaternion.Euler (0f, 0f, curRot + (int)parms[0]);
		curRot += (int)parms[0];
		if(curRot >= 360)
			curRot = 0;
		while (elapsedTime < (float)parms[2]) 
		{
			elapsedTime += Time.deltaTime; // <- move elapsedTime increment here
			// Rotations
			((GameObject)parms[1]).transform.rotation = Quaternion.Slerp(startingRotation, targetRotation,  (elapsedTime / (float)parms[2]));
			yield return new WaitForEndOfFrame ();
		}
	}

	/*----------------------------------------------------------------------------
    --	Subscriber function to NetworkingManager, recieves events when Building
    --  is created 
    --
	--	Interface:  void UpdateBuildingCallBack(JSONClass data)
	--					-JSONClass data: JSON object recieved back
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	private void UpdateBuildingCreationCallBack(JSONClass data)
	{
		int team = data[NetworkKeyString.TeamID].AsInt;
		GameObject building = shop.Items[data[NetworkKeyString.BuildType].AsInt-1].Building;

		// Retrieve the Building component attached with the game object
		Building bComponent = building.GetComponent<Building>();

		Vector3 pos = new Vector3(data[NetworkKeyString.XPos].AsFloat, data[NetworkKeyString.YPos].AsFloat, data[NetworkKeyString.ZPos].AsFloat);

		bComponent.team = data[NetworkKeyString.TeamID].AsInt;

		building.GetComponent<Building>().placing = false;

		GameObject b1 = (GameObject)Instantiate(building, pos, Quaternion.Euler(0, 0, data[NetworkKeyString.ZRot].AsFloat));
        if (b1.GetComponent<Building>().type == Building.BuildingType.Turret) 
		{
           // building.GetComponent<AI>()
           b1.GetComponent<AI>().instantTurret(1, 40, data[NetworkKeyString.TeamID].AsInt, 18, 100);
            Debug.Log("Instant turret 1");
        }
		b1.GetComponent<Building>().notifycreation();
	}

	private void UpdateBuildingDestructionCallBack(JSONClass data)
	{
		Vector3 Key = new Vector3(data[NetworkKeyString.XPos].AsFloat, data[NetworkKeyString.YPos].AsFloat, data[NetworkKeyString.ZPos].AsFloat);
        //If the building exists, destroy it
        if (GameData.Buildings.ContainsKey(Key))
        {
            Destroy(GameData.Buildings[Key].gameObject);
            GameData.Buildings.Remove(Key);
        }
	}
	/*----------------------------------------------------------------------------
    --	Attempt to place a building to where the mouse is at when an left click 
    --  event is triggered. Assigns the corresponding attributes to the Building
    --  component.
	--	Interface:  private bool PlaceBuilding(GameObject building)
    --					[building] Building that will be placed 
    --	programmer: Jerry Jia, Thomas Yu
    --	@return: void
	------------------------------------------------------------------------------*/
	private bool PlaceBuilding(GameObject building)
	{

		// Construct a vector of where the Gameobject will be placed 
		Vector3 buildingLocation = new Vector3((int)currFramePosition.x, (int)currFramePosition.y,-2);

		// Check if it is a valid location to place the building 
		if(!CheckValidLocation(building))
			return false;


		// Set the color transparency 
		shop.Selected.Building.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);

		SetAllCollidersStatus(building, true);


		// Indicate that the item has been successfully bought and placed 
		ItemBought = false; 

        //weird merge conflict here (END)
		placementRange.SetActive(false);

		print ("x: " + building.transform.rotation.x + " y: " + building.transform.rotation.y + " z: " + building.transform.rotation.z);
		if (Application.platform == RuntimePlatform.LinuxPlayer)
		{
			// Send the packet, with Team ID, user name, and the message input
			List<Pair<string, string>> packetData = new List<Pair<string, string>>();
			packetData.Add(new Pair<string, string>(NetworkKeyString.TeamID, GameData.MyPlayer.TeamID.ToString()));
			packetData.Add(new Pair<string, string>(NetworkKeyString.XPos, buildingLocation.x.ToString()));
			packetData.Add(new Pair<string, string>(NetworkKeyString.YPos, buildingLocation.y.ToString()));
			packetData.Add(new Pair<string, string>(NetworkKeyString.ZPos, buildingLocation.z.ToString()));
			packetData.Add(new Pair<string, string>(NetworkKeyString.XRot, building.transform.rotation.x.ToString()));
			packetData.Add(new Pair<string, string>(NetworkKeyString.YRot, building.transform.rotation.y.ToString()));
			packetData.Add(new Pair<string, string>(NetworkKeyString.ZRot, building.transform.eulerAngles.z.ToString()));
			packetData.Add(new Pair<string, string>(NetworkKeyString.BuildType, ((int)buildType).ToString()));
			var packet = NetworkingManager.send_next_packet(DataType.UI, (int)UICode.BuildingCreation, packetData, Protocol.NA);
			Send(packet);
		}else
		{
			GameObject testBuild = (GameObject)Instantiate(building, buildingLocation, building.transform.rotation);
			testBuild.GetComponent<Building>().placing = false;
            if (testBuild.GetComponent<Building>().type == Building.BuildingType.Turret)
            {//
                testBuild.GetComponent<AI>().instantTurret(2, 40, 2, 15, 15);

                //testBuild.GetComponent<AI>().instantTurret(2, 40, GameData.MyPlayer.TeamID, 15, 15);
                Debug.Log("Instant turret 2");
            }
        }
	
		Destroy(building);
		return true;
 	}
		

	/*----------------------------------------------------------------------------
    --	Highlight and select an item in the shop menu
	--	Interface:  void HighlightItem(int i)
	--
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void HighlightItem(int i)
	{
		shop.Selected.Option = shop.Items[i-1].Option;
		shop.Selected.Building = shop.Items[i-1].Building;
		shop.Selected.Option.image.color = Color.green;
	}

	
	/*----------------------------------------------------------------------------
    --	UnHighlight and deselect an item in the shop menu
	--	Interface:  void HighlightItem(int i)
	--
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void UnHighlightItem()
	{
		// Unselect the current selected item
		shop.Selected.Option.image.color = Color.white;
		shop.Selected.Building = null;
		shop.Selected.Option = null;
	}

	/*----------------------------------------------------------------------------
    --	Check if the building cana be placed based on distance from player and 
    --	existing building
	--	Interface:  void HighlightItem(int i)
    --	
    --	programmer: Jerry Jia, Thomas Yu
    --	@return: void
	------------------------------------------------------------------------------*/
	private bool CheckValidLocation(GameObject building)
	{
		//Check if player isn't too far to place building
		Vector2 player = GameManager.instance.player.transform.position;
		float distance_from_player = Vector3.Distance(player, building.transform.position);
		if(distance_from_player > 8 || distance_from_player <= 2  )
			return false;
		if(building.GetComponent<Building>().collidercounter==0)
			return true;
		return false;

	}

	/*----------------------------------------------------------------------------
    --	Display the shop menu
	--	Interface:  public void DisplayShop()
	--
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void DisplayShop()
	{
		GameData.MouseBlocked = false;
		shop.MainPanel.SetActive(shop.MainPanel.activeSelf ? false : true);
	}
	
	/*----------------------------------------------------------------------------
    --  Wrapper for NetworkingManager.TCP_Send to use for chat system
	--	Interface: private static void Send(string packet)
	--
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	private static void Send(string packet)
	{
		if(NetworkingManager.TCP_Send(packet, 256) < 0)
			Debug.Log("[Debug]: SelectTeam(): Packet sending failed\n");
	}



	/*----------------------------------------------------------------------------
    --  CallBack function that will subscribe to NetworkingManager to recieve
    --  TCP chat messages
	--	Interface: void UpdateChatCallBack(JSONClass data)
	--
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void UpdateChatCallBack(JSONClass data)
	{

		int team 		= data[NetworkKeyString.TeamID].AsInt;
		string username = data[NetworkKeyString.UserName];
		string message 	= data[NetworkKeyString.Message];

		UpdateChat(team, username, message);
	}

	/*----------------------------------------------------------------------------
    --  Update the chat panel with messages from NetworkingManger. Display will be 
    --  shown differently depending on the team that sends the message
	--	Interface: public void UpdateChat(int team, string username, string message)
	--					[team] 		The team that sent the message
	--					[username] 	User name of the player
	--					[message]	Content of the message
	--
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdateChat(int team, string username, string message)
	{
		// For styling
		username = "[" + username + "]:";
		GameObject childObject;
		// Ally Message
		if(team == GameData.MyPlayer.TeamID)
		{
			// Assign corresponding values 
			foreach(Transform child in chat.AllyMessage.transform)
			{
				if(child.name == "Name")
					child.GetComponent<Text>().text = username;
				else
					child.GetComponent<Text>().text = message;
			}
			childObject = Instantiate (chat.AllyMessage) as GameObject;								//Instantitate arrow
			childObject.transform.SetParent (chat.Container.transform, false);						//Make arrow a child object of InputHistory
		}
		// Enemy Message
		else
		{
			// Assign corresponding values 
			foreach(Transform child in chat.EnemyMessage.transform)
			{
				if(child.name == "Name")
					child.GetComponent<Text>().text = username;
				else
					child.GetComponent<Text>().text = message;
			}
			childObject = Instantiate (chat.EnemyMessage) as GameObject;					//Instantitate arrow
			childObject.transform.SetParent (chat.Container.transform, false);				//Make arrow a child object of InputHistory
		}
	}

	float time = 0;
	private void UpdateTimer()
	{
		time += Time.deltaTime;
		float t = Math.Abs(time);
		int seconds = (int)(t % 60);
		int minutes = (int)(t / 60); // calculate the minutes
		timer.text = String.Format("{0:00}:{1:00}", minutes, seconds);
	}

	bool slideStat = false;
	public void StatAnimation()
	{
		if(!slideStat)
		{
			statsPanel.GetComponent<Animator>().SetTrigger("Slide_in");
		}
		else
		{
			statsPanel.GetComponent<Animator>().SetTrigger("Slide_out");
		}
		slideStat = !slideStat;
	}

	/*----------------------------------------------------------------------------
    --	Update player hp on the HUD, and triggers the "TakeDmg" animation
    --
    --	Interface:  public void UpdatePlayerHealth(float Dmg)
    --	            [dmg] Amount of HP added or removed, has to be less than 1. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdatePlayerHealth(float Dmg)
	{
		if((Dmg < 0 && playerProfile.Health.fillAmount <= 0) || (Dmg > 0 && playerProfile.Health.fillAmount >= 1))
			return;
		playerProfile.Health.fillAmount += Dmg;
		playerProfile.HealthAnimator.SetTrigger("TakeDmg");
	}

	/*----------------------------------------------------------------------------
    --	Update ally king's hp on the HUD, and triggers the "TakeDmg" animation
    --
    --	Interface:  public void UpdateAllyKingHealth(float Dmg)
    --	            [dmg] Amount of HP added or removed, has to be less than 1. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdateAllyKingHealth(float Dmg)
	{
		if((Dmg < 0 && allyKing.Health.fillAmount <= 0) || (Dmg > 0 && allyKing.Health.fillAmount >= 1))
			return;
		allyKing.Health.fillAmount += Dmg;
		allyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	/*----------------------------------------------------------------------------
    --	Update enemy king's hp on the HUD, and triggers the "TakeDmg" animation
    --
    --	Interface:  public void UpdateEnemyKingHealth(float Dmg)
    --	            [dmg] Amount of HP added or removed, has to be less than 1. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdateEnemyKingHealth(float Dmg)
	{
		if((Dmg < 0 && enemyKing.Health.fillAmount <= 0) || (Dmg > 0 && enemyKing.Health.fillAmount >= 1))
			return;
		enemyKing.Health.fillAmount += Dmg;
		enemyKing.HealthAnimator.SetTrigger("TakeDmg");
	}

	/*----------------------------------------------------------------------------
    --	Update currency value on the HUD, and triggers the "UpdateCurrency" animation
    --
    --	Interface:  public void UpdateCurrency(float amount)
    --	            [amount] Amount of currency added or removed. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UpdateCurrency(int amount)
	{
		if((amount < 0 && int.Parse(currency.Amount.text) <= 0))
			return;
		int num = int.Parse(currency.Amount.text, NumberStyles.AllowThousands) + amount;
		currency.Amount.text = String.Format("{0:n0}", num);
		currency.CurrencyAnimator.SetTrigger("UpdateCurrency");
	}

	/*----------------------------------------------------------------------------
    --	Sets fill amount of main skill's progress bar to empty whenever a skill is 
    --  being used. The fill amount will be recharged in Update() corresponding to 
    --  the CoolDown value
    --
    --	Interface:  public void UseMainSkill(float CoolDown)
    --	            [CoolDown] Skill cool down value in seconds. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UseMainSkill(float CoolDown)
	{
		if(mainSkill.ProgressBar.fillAmount >= 1)
		{
			mainSkill.ProgressBar.fillAmount = 0f;
			mainSkill.CoolDown = CoolDown;
		}
	}

	/*----------------------------------------------------------------------------
    --	Sets fill amount of sub skill's progress bar to empty whenever a skill is 
    --  being used. The fill amount will be recharged in Update() corresponding to 
    --  the CoolDown value
    --
    --	Interface:  public void UseSubSkill(float CoolDown)
    --	            [CoolDown] Skill cool down value in seconds. 
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	public void UseSubSkill(float CoolDown)
	{
		if(subSkill.ProgressBar.fillAmount >= 1)
		{
			subSkill.ProgressBar.fillAmount = 0f;
			subSkill.CoolDown = CoolDown;
		}
	}

	public void UpgradeWeapon()
	{
		Debug.Log ("Upgrade weapon here");
	}
	public void AddPotion(String potionType)
	{
		Debug.Log (potionType + " Potion");
		Debug.Log ("Add potion here");
	}

	#region Classes
	/**
	 *  Indicates the player health on bottom left corner of HUD
	 */
	[System.Serializable]
	public class PlayerProfile
	{
		public Image 	Health;
		public Animator HealthAnimator;
	}
	/**
	 *  Indicates the health bar of ally king
	 */
	[System.Serializable]
	public class AllyKing
	{
		public Image 	Health;
		public Animator HealthAnimator;
	}
	/**
	 *  Indicates the health bar of ally king
	 */
	[System.Serializable]
	public class EnemyKing
	{
		public Image 	Health;
		public Animator HealthAnimator;
	}
	
	/**
	 *  Indicates current currency amount
	 */
	[System.Serializable]
	public class Currency 		
	{ 
		public Text  	Amount;						
		public Animator CurrencyAnimator; 	
	}
	
	/**
	 *  Indicates main skill bar
	 */
	[System.Serializable]
	public class MainSkill 		
	{ 
		public Image 	ProgressBar;					
		public float 	CoolDown; 			
	}
	/**
	 *  Indicates sub skill bar
	 */
	[System.Serializable]
	public class SubSkill 		
	{ 
		public Image 	ProgressBar;					
		public float 	CoolDown; 			
	}
	/**
	 *  Indicates passive skill bar
	 */
	[System.Serializable]
	public class PassiveSkill 	
	{ 
		public Image 	ProgressBar;					
		public float 	CoolDown; 			
	}
	/**
	 *  Indicates the chat panel
	 */
	[System.Serializable]
	public class Chat			
	{ 
		// Input field 
		public InputField input;
		// Container box for chat messages
		public GameObject Container; 	
		public GameObject AllyMessage;			
		public GameObject EnemyMessage; 	
	}
	/**
	 * Items that can be built in the shop
	 */
	[System.Serializable]
	public class Buildable		
	{ 
		public Button Option;						
		public GameObject Building;			
	}
	/**
	 *  Shop panel
	 */
	[System.Serializable]
	public class Shop			
	{ 
		public GameObject 		MainPanel;
		// Purchasable items
		public List<Buildable>	Items;	
		
		[HideInInspector]
		// Currently selected item
		public Buildable		Selected = null;										
	}
	[System.Serializable]
	public class PlayerDP
	{
		public Sprite			GunnerDP;
		public Sprite			NinjaDP;
		public Sprite			MageDP;
		public Image			Container;
	}
	[System.Serializable]
	public class ChargeBar
	{
		public GameObject		Holder;
		public Image			Bar;
	}
	[System.Serializable]
	public class Stamina
	{
		public Image			ProgressBar;
		public float			FillTimer;
	}
	[System.Serializable]
	public class WinScreen
	{
		public Animator			Parent;
		public Animator			Child;
	}
    [System.Serializable]
    public class LoseScreen
    {
        public Animator         Parent;
        public Animator         Child;
    }
    #endregion
}
