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
	
	public Building(int x, int y){
		this.X=x;
		this.Y=y;
	}
	// Use this for initialization
	void Start () {
		this.health=100;
		notifycreation();
		Debug.Log ("X Position is:" + X + "Y position is : " + Y );

	}
	void OnTriggerEnter2D(Collider2D other) {
		//		if (coll.gameObject.tag == "Enemy")
		//			coll.gameObject.SendMessage("ApplyDamage", 10);
		Debug.Log (this.health);
		if(health<=0){
			Destroy(gameObject);
		}
		var attack = other.gameObject.GetComponent<Trigger>();
		Debug.Log("Projectile hit");
		if (attack != null)
		{
			Debug.Log("Attack was not null");
			if (attack.teamID == team)
			{
				//health-=10;
				Debug.Log("Test health" + health);
				Debug.Log("Same team");
				return;
			}
			health-=10;
		}
		else
		{
			Debug.Log("Attack was null");
		}


		notifydeath();
	}
	// Update is called once per frame
	void Update () {
		
	}
	public void notifycreation(){
		//????
		
	}
	public void	notifydeath(){
		//?
	}
	
}
