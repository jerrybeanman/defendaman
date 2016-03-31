using UnityEngine;
using System.Collections;
using System;

public class Building:MonoBehaviour {
	
	public enum BuildingType{Empty,Armory,Wall,Watchtower,Turret, Alchemist};

	public BuildingType type;

	public float health = 100;

	public int team;

	public Sprite allyBuilding;
	public Sprite enemyBuilding;

	public float ConstructionTime = 2f;

	SpriteRenderer spriteRenderer;

	[HideInInspector]
	public bool placing = false;
	[HideInInspector]
	public bool placeble = true;
	[HideInInspector]
	public bool constructed = false;

	// Use this for initialization
	void Start () 
	{
		if(!placing)
			//gameObject.GetComponent<Animator>().SetTrigger("Create");
			StartCoroutine(Construct());

		if(GameData.MyPlayer.TeamID == team)
			gameObject.GetComponent<SpriteRenderer>().sprite = allyBuilding;
		else
			gameObject.GetComponent<SpriteRenderer>().sprite = enemyBuilding;

    }

	IEnumerator Construct()
	{
		float elapsedTime = 0.0f;
		Vector3 startingScale = new Vector3(0, 0, 0); // have a startingRotation as well
		Vector3 targetScale = transform.localScale;

		while (elapsedTime < ConstructionTime) 
		{
			elapsedTime += Time.deltaTime; // <- move elapsedTime increment here
			// Scale
			transform.localScale = Vector3.Slerp(startingScale, targetScale, (elapsedTime / ConstructionTime));
			yield return new WaitForEndOfFrame ();
		}
		constructed = true;
	}

	void OnTriggerEnter2D(Collider2D other) 
	{
		if(other.gameObject.tag == "Bullet")
			return;
		if(placing)
		{
			print ("dont place plz");
			placeble = false;
		}
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

	void OnTriggerExit2D(Collider2D other)
	{
		if(placing)
		{
			placeble = true;
			print ("place plzzz");
		}
	}

	public void notifycreation(){
		//????
	}
	public void	notifydeath()
	{
		//?
	}
	
}
