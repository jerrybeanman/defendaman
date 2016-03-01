using UnityEngine;
using System.Collections;
using System;

public class BasicRanged : MonoBehaviour
{
    private Vector2 startPos;
    public int maxDistance;
    public int teamID;
    public float damage;
    
    //projectile speed

    void Start()
    {
        maxDistance = 10;
        startPos = transform.position;
    }

    void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.GetComponent<BaseClass>() != null && teamID == other.gameObject.GetComponent<BaseClass>().team)
        {
            return;
        }
        if(other.gameObject.GetComponent<BaseClass>() != null)
        {
            BaseClass target = other.gameObject.GetComponent<BaseClass>();
            if (target.damaged(damage) <= 0.0f) {
                Destroy(other.gameObject);
            }
        }
        Destroy(gameObject);
    }
}
