using UnityEngine;
using System.Collections;
using System;

public class Building:MonoBehaviour {
	
	public enum BuildingType{Empty,Armory, Wall};

	public BuildingType type;

	public int X {get; set;}
	public int Y {get;  set;}
	public float health = 100;

	public int team;
	public bool placing = false;

	public Sprite allyBuilding;
	public Sprite enemyBuilding;

	SpriteRenderer spriteRenderer;
	public Building(int x, int y)
	{
		this.X=x;
		this.Y=y;
	}

	// Use this for initialization
	void Start () 
	{
		if(!placing)
			gameObject.GetComponent<Animator>().SetTrigger("Create");

		if(GameData.MyPlayer.TeamID == team)
			gameObject.GetComponent<SpriteRenderer>().sprite = allyBuilding;
		else
			gameObject.GetComponent<SpriteRenderer>().sprite = enemyBuilding;


		notifycreation();
    }


	void OnTriggerEnter2D(Collider2D other) 
	{
		if(health<=0)
			Destroy(gameObject);
		var attack = other.gameObject.GetComponent<Trigger>();
		if (attack != null)
		{
			if (attack.teamID == team)
				return;
			health-=10;
		}
		notifydeath();
	}


	public void notifycreation(){
		//????
	}
	public void	notifydeath()
	{
		//?
	}
	
}
