using UnityEngine;
using System;

public class Upgrader : MonoBehaviour {
	
	private Building b;
	
	void Start()
	{
		b = gameObject.GetComponent<Building>();
	}
	
	void OnMouseDown() 
	{
		if(b.constructed)
		{
            if (b.team == GameData.MyPlayer.TeamID)
            {
                if (b.type == Building.BuildingType.Armory)
                {
                    GameObject.Find("HUD").transform.FindChild("Upgrade Overlay").gameObject.SetActive(true);
					Invoke("SetInActiveUpgrade", 8f);
                }
                if (b.type == Building.BuildingType.Alchemist)
                {
                    GameObject.Find("HUD").transform.FindChild("Potion Overlay").gameObject.SetActive(true);
					Invoke("SetInActivePotion", 8f);
                }
            }
		}
	}

	/*----------------------------------------------------------------------------
    --	DeActivate potion overlay
    --
    --	Interface:  void SetInActivePotion()
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void SetInActivePotion()
	{
		GameObject.Find("HUD").transform.FindChild("Potion Overlay").gameObject.SetActive(false);
	}

	/*----------------------------------------------------------------------------
    --	Activate Deactivate upgrade overlay
    --
    --	Interface: void SetInActiveUpgrade()
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	void SetInActiveUpgrade()
	{
		GameObject.Find("HUD").transform.FindChild("Upgrade Overlay").gameObject.SetActive(false);
	}
}

