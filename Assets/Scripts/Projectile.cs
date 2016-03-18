using UnityEngine;
using System.Collections;

public abstract class Projectile : Trigger
{
    public int pierce = 0;

    public Projectile()
    {

    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //If its a player or an AI, ignore it, otherwise destroy itself
        if (other.gameObject.GetComponent<BaseClass>() != null)
        {
            //If it collided with a player
            return;
        }

        if (other.gameObject.GetComponent<Trigger>() != null && teamID == other.gameObject.GetComponent<Trigger>().teamID)
        {
            //If it collided with another projectile or a sword
            return;
        }

        if (other.gameObject.GetComponent<AI>() != null && teamID == other.gameObject.GetComponent<AI>().team)
        {
            //If it collided with AI
            return;
        }

        //Otherwise, its a wall or some solid
        if (--pierce < 0) {
            //Debug.Log("Projectile Collided with: " + other);
            Destroy(gameObject);
        }
    }
}
