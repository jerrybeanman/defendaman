using UnityEngine;
using System.Collections;
using System;

public class Building:MonoBehaviour {
	
	public enum BuildingType{Empty,Armory, Wall};

	public BuildingType type;

	public int X {get; set;}
	public int Y {get;  set;}
	public float health = 100;
	GameObject building;

	public int team;

	public GameObject progress_bar;
	public Sprite ally_completion_progress_bar;
	public Sprite enemy_completion_progress_bar;

	private SpriteRenderer spriteRenderer;

	public Building(int x, int y)
	{
		this.X=x;
		this.Y=y;
	}

	// Use this for initialization
	void Start () 
	{
		if(team == GameManager.instance.player.GetComponent<BaseClass>().team)
			progress_bar.GetComponent<SpriteRenderer>().sprite = ally_completion_progress_bar;
		else
			progress_bar.GetComponent<SpriteRenderer>().sprite = enemy_completion_progress_bar;
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
	public void notifycreation()
	{
		//????
		
	}
	public void	notifydeath()
	{
		//?
	}
	
}
