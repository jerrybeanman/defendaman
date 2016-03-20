using UnityEngine;
using System.Collections;
using System;

public class BasicRanged : Projectile
{
    private Vector2 startPos;
    public int maxDistance;

    //projectile speed

    new void Start()
    {
        base.Start();
        startPos = transform.position;
    }

    void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /*
    //Test burning projectile script
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID != other.gameObject.GetComponent<BaseClass>().team)
        {
            var burn = other.gameObject.GetComponent<Burn>();
            if (burn == null)
            {
                other.gameObject.AddComponent<Burn>();
            }
            else
            {
                burn.duration = 600;
            }
        }
    }
    */
}
