using UnityEngine;
using System.Collections;
using System;

public class Building:MonoBehaviour {
	
	public enum BuildingType{Empty,Armory,Wall,Watchtower,Turret};

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
		if (type == Building.BuildingType.Turret) 
		{
			// Calling this method:
			// instantTurret(float reload, int speed, int teamToIgnore, int range)
			// Suggested values: 1.5 - 3 reload, 35-40 speed, 15 range
			// our team # = GameData.myPlayer.TeamID
			gameObject.GetComponent<AI>().instantTurret(1.5f, 35, 111, 15);
			
			gameObject.layer = LayerMask.NameToLayer("Default");
		}

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
