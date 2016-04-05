﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Building:MonoBehaviour {
	
	public enum BuildingType{Empty,Armory,Wall,Watchtower,Turret, Alchemist};

	public BuildingType type;

	public float MaxHp  = 100;
	public float health;
	public int 	 cost;
	public float Health
	{
		get
		{
			return health;
		}
		set
		{
			health = value;
			healthBar.UpdateHealth(MaxHp, health);
		}
	}

	public int team;
	public int collidercounter = 0;
	public Sprite allyBuilding;
	public Sprite enemyBuilding;
	public HealthBar healthBar;
	
	public float ConstructionTime = 2f;

	private SpriteRenderer spriteRenderer;

	[HideInInspector]
	public bool placing = true;
	[HideInInspector]
	public bool placeble;
	[HideInInspector]
	public bool constructed = false;
	public bool playerlocker =false;

	// Use this for initialization
	void Start () 
	{
		health = MaxHp;
		playerlocker=false;
		collidercounter=0;
		if(!placing)
			//gameObject.GetComponent<Animator>().SetTrigger("Create");
			StartCoroutine(Construct());

		placeble = true;
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

	/*----------------------------------------------------------------------------
    --	Called when an collidable object enters the bounding box
    --
    --	Interface: 	void OnTriggerEnter2D(Collider2D other)
    --					-Collider2D other: Other collider that entered 
    --
    --	programmer: Jerry Jia, Thomas Yu
    --	@return: void
	------------------------------------------------------------------------------*/
	void OnTriggerEnter2D(Collider2D other) 
	{
		if(other.gameObject.tag == "Bullet")
			return;
		if(placing && other.gameObject.tag != "Untagged" && other.gameObject.tag != "Player" )
		{

			collidercounter++;

			print ("Enter Tag is :" + other.gameObject.tag + "increasedm Counter= " + collidercounter);
			placeble = false;

		}

		var attack = other.gameObject.GetComponent<Trigger>();
		if (attack != null)
		{
			if (attack.teamID == team)
				return;
			float damage = other.GetComponent<Trigger>().damage;
			Health -= damage;
		}

		if(health<=0)
		{
			if (Application.platform == RuntimePlatform.LinuxPlayer)
			{
				notifydeath();
			}else
				Destroy(gameObject);
		}
	}

	/*----------------------------------------------------------------------------
    --	Called when an collidable object leaves the bounding box of it's BoxCollider2D
    --
    --	Interface: 	void OnTriggerExit2D(Collider2D other)
    --					-Collider2D other: Other collider that left 
    --
    --	programmer: Jerry Jia, Thomas Yu
    --	@return: void
	------------------------------------------------------------------------------*/
	void OnTriggerExit2D(Collider2D other)
	{
		if(placing && other.gameObject.tag != "Untagged" && other.gameObject.tag != "Player" )
		{
			collidercounter--;
			placeble = true;
			print ("Exit Tag is :" + other.gameObject.tag + "Decreased, counter= " + collidercounter);
		}
	}

	public void notifycreation()
	{
		GameData.Buildings.Add(transform.position, this);
	}

	public void	notifydeath()
	{
		// Send the packet, with Team ID, user name, and the message input
		List<Pair<string, string>> packetData = new List<Pair<string, string>>();
		packetData.Add(new Pair<string, string>(NetworkKeyString.XPos, transform.position.x.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.YPos, transform.position.y.ToString()));
		packetData.Add(new Pair<string, string>(NetworkKeyString.ZPos, transform.position.z.ToString()));
		
		var packet = NetworkingManager.send_next_packet(DataType.UI, (int)UICode.BuildingDestruction, packetData, Protocol.TCP);
        NetworkingManager.send_next_packet(DataType.UI, (int)UICode.BuildingDestruction, packetData, Protocol.TCP);
    }
}
