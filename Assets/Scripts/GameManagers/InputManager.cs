using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {

		// Only detects game inputs when we are not in chat mode
		if(!GameData.InputBlocked)
		{
			VirtualStartGame();
			ConsumeItem();
			HUDToggles();
		}
	}

	/*----------------------------------------------------------------------------
    --	Check for start game on windows
    --
    --	Interface:  void VirtualStartGame()
    --
    --	progrbammer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	private void VirtualStartGame()
	{
		// Simulate non-networked start of game as Gunner class on client 
		if (Input.GetKeyDown(KeyCode.Z))
		{
			GameStart_Testing.instance.myID = 1;
			if (Application.platform != RuntimePlatform.LinuxPlayer)
				GameStart_Testing.instance.StartOfGame();
		}
		
		// Simulate non-networked start of game as mage class on client
		if (Input.GetKeyDown(KeyCode.X))
		{
			GameStart_Testing.instance.myID = 2;
			if (Application.platform != RuntimePlatform.LinuxPlayer)
				GameStart_Testing.instance.StartOfGame();
		}
		
		// Simulate non-networked start of game as ninja class on client
		if (Input.GetKeyDown(KeyCode.C))
		{
			GameStart_Testing.instance.myID = 3;
			if (Application.platform != RuntimePlatform.LinuxPlayer)
				GameStart_Testing.instance.StartOfGame();
		}
	}


	/*----------------------------------------------------------------------------
    --	Check items hotkey
    --
    --	Interface:  void ConsumeItem()
    --
    --	progrbammer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	private void ConsumeItem()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			if (Inventory.instance.inventory_item_list[Constants.SLOT_1].type == Constants.CONSUMABLE_TYPE)
			{
				Debug.Log("1 is consumable");
				Inventory.instance.UseConsumable(Constants.SLOT_1);
			}
		}
		
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			if (Inventory.instance.inventory_item_list[Constants.SLOT_2].type == Constants.CONSUMABLE_TYPE)
			{
				Debug.Log("2 is consumable");
				Inventory.instance.UseConsumable(Constants.SLOT_2);
			}
		}
		
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			if (Inventory.instance.inventory_item_list[Constants.SLOT_3].type == Constants.CONSUMABLE_TYPE)
			{
				Debug.Log("3 is consumable");
				Inventory.instance.UseConsumable(Constants.SLOT_3);
			}
		}
		
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			if (Inventory.instance.inventory_item_list[Constants.SLOT_4].type == Constants.CONSUMABLE_TYPE)
			{
				Debug.Log("4 is consumable");
				Inventory.instance.UseConsumable(Constants.SLOT_4);
			}
		}
	}

	/*----------------------------------------------------------------------------
    --	Check for HUD event toogles
    --
    --	Interface:  void HUDToggles()
    --
    --	progrbammer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	private void HUDToggles()
	{
		
		if(Input.GetKeyDown(KeyCode.M))
		{
			HUD_Manager.instance.DisplayShop();
		}
	}
}
