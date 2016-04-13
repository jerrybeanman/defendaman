/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Laser.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void start()
--      override void OnTriggerEnter2D(Collider2D other)
--
--  DATE:           March 9, 2016
--
--  REVISIONS:      (Date and Description)
--
--  DESIGNERS:      Hank Lo, Allen Tsang
--
--  PROGRAMMER:     Hank Lo, Allen Tsang
--
--  NOTES:
--  This class is the laser for the gunner - it inherits from Projectile.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System;

public class Laser : Projectile
{
    GameObject explosion;

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Start
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void Start(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Start of scripts creation. Used to instantiate variables in our case.
    ---------------------------------------------------------------------------------------------------------------------*/
    new void Start()
    {
        base.Start();
        explosion = (GameObject)Resources.Load("Prefabs/LaserExplosion", typeof(GameObject));
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerEnter2D
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: override void OnTriggerEnter2D(Collider2D other)
    --             Collider2D other: The object's collider that this projectile hits
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Called when an object collides with this projectile. We check what hits it here, and does the appropriate action.
    ---------------------------------------------------------------------------------------------------------------------*/
    protected override void OnTriggerEnter2D(Collider2D other)
    {
		// ignore health bar
		if(other.gameObject.tag == "HealthBar")
		{
			return;
		}
        //If its a player or an AI, ignore it, otherwise destroy itself
        var player = other.gameObject.GetComponent<BaseClass>();
        if (player != null && teamID == player.team)
        {
            //Ignore team players
            return;
        }

        var trigger = other.gameObject.GetComponent<Trigger>();
        if (trigger != null && (teamID == trigger.teamID || trigger is Area))
        {
            //Ignore team attacks or areas
            return;
        }

        var ai = other.gameObject.GetComponent<AI>();
        if (ai != null && teamID == ai.team)
        {
            //Ignore team AIs
            return;
        }

        if (other.gameObject.GetComponent<WorldItemData>() != null)
        {
            //Ignore items
            return;
        }
        
        var instance = (GameObject)Instantiate(explosion, transform.position, transform.rotation);
        Destroy(instance, 1);

        //Otherwise, its a wall or some solid
        if (--pierce < 0 || other.name == "obstacleTiles(Clone)" || other.name == "tron_obstacle(Clone)")
        {
            Destroy(gameObject);
        }
    }
}

