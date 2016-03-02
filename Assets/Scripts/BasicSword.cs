using UnityEngine;
using System.Collections;
using System;

public class BasicSword : Melee
{
    private Vector2 startPos;
    //public int maxDistance;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        /*
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
        */
    }
}
