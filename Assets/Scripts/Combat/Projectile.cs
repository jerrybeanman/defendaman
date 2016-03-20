using UnityEngine;
using System.Collections;

public abstract class Projectile : Trigger
{
    private Vector2 startPos;
    public int maxDistance;
    public int pierce = 0;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //If its a player or an AI, ignore it, otherwise destroy itself
        if (other.gameObject.GetComponent<BaseClass>() != null)
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
