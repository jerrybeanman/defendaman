using UnityEngine;
using System.Collections;
using System;

public class MagicCircle : Projectile
{
    private Vector2 startPos;
    public int maxDistance;

    //projectile speed

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // this calculates how long the circle stays for
        maxDistance--;
        if (maxDistance <= 0)
        {
            Destroy(gameObject);
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID == other.gameObject.GetComponent<BaseClass>().team)
        {
            // gameObject.AddComponent("MagicalBuff");
            return;
        }
    }

    protected void onTriggerStay2D(Collider2D other) {
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID == other.gameObject.GetComponent<BaseClass>().team)
        {
            // gameObject.AddComponent("MagicalBuff");
            return;
        }
    }
}
