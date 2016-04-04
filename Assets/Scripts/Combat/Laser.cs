using UnityEngine;
using System.Collections;
using System;

public class Laser : Projectile
{
    GameObject explosion;

    new void Start()
    {
        base.Start();
        explosion = (GameObject)Resources.Load("Prefabs/LaserExplosion", typeof(GameObject));
    }

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
            //If it collided with a player
            return;
        }

        var trigger = other.gameObject.GetComponent<Trigger>();
        if (trigger != null && (teamID == trigger.teamID || trigger is Area))
        {
            //If it collided with another projectile or a sword
            return;
        }

        var ai = other.gameObject.GetComponent<AI>();
        if (ai != null && teamID == ai.team)
        {

            //If it collided with AI
            return;
        }

        if (other.gameObject.GetComponent<WorldItemData>() != null)
        {
            //If it collided with items
            return;
        }
        
        var instance = (GameObject)Instantiate(explosion, transform.position, transform.rotation);
        Destroy(instance, 1);

        //Otherwise, its a wall or some solid
        if (--pierce < 0 || other.name == "obstacleTiles(Clone)")
        {
            Destroy(gameObject);
        }
    }
}

