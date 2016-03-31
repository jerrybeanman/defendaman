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
			if(b.type == Building.BuildingType.Armory)
			{
				print ("Asdfasdf");
				GameObject.Find("HUD").transform.GetChild(12).gameObject.SetActive (true);
			}
			if(b.type == Building.BuildingType.Alchemist)
			{
				GameObject.Find("HUD").transform.GetChild(11).gameObject.SetActive (true);
			}
		}
	}
}

