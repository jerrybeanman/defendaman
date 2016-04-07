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
                    GameObject.Find("HUD").transform.GetChild(10).gameObject.SetActive(true);
                    print("Armory testing");
                }
                if (b.type == Building.BuildingType.Alchemist)
                {
                    GameObject.Find("HUD").transform.GetChild(11).gameObject.SetActive(true);
                }
            }
		}
	}
}

