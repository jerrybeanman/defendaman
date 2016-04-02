using UnityEngine;
using System.Collections;

/*-----------------------------------------------------------------------------
-- Resource.cs - Script attached to Gold item game objects
--                       responsible for pulling gold towards the player.
--
-- FUNCTIONS:
--		void Start()
--		void Update()
--
-- DATE:		02/04/2016
-- REVISIONS:	N/A
-- DESIGNER:	Bulalakaw
-- PROGRAMMER:  Krystle Bulalakaw
-----------------------------------------------------------------------------*/
public class CoinMagnetize : MonoBehaviour {
	GameObject target;
	Vector3 currentPosition;
	float dist = 15f;
	float velocity = 10f;
	float acceleration = 1.1f;

	// Use this for initialization
	void Start () {
	}
	
	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	Update
    -- DATE: 		April 2, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	Update(void)
    -- RETURNS: 	void.
    -- NOTES:
    -- Called once per frame. Moves the object towards the player at an exponentially increasing velocity if they 
    -- enter a certain distance to the object and their inventory is not full.
    ----------------------------------------------------------------------------------------------------------------------*/
	void Update () {
		Vector3 playerPosition = GameData.PlayerPosition[GameData.MyPlayer.PlayerID];

		if (Inventory.instance.CheckIfItemCanBeAdded(true, 2)) {
			if (Vector3.Distance(transform.position, playerPosition) < dist) { 	
				transform.position = Vector3.MoveTowards(transform.position, playerPosition, Time.deltaTime * velocity);
				velocity *= acceleration;
			}
		}
	}
}
