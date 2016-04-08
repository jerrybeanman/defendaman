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
                }
                if (b.type == Building.BuildingType.Alchemist)
                {
                    GameObject.Find("HUD").transform.FindChild("Potion Overlay").gameObject.SetActive(true);
					Invoke("SetInActive", 10f);
                }
            }
		}
	}

	void SetInActive()
	{
		GameObject.Find("HUD").transform.FindChild("Potion Overlay").gameObject.SetActive(false);
	}
}

