using UnityEngine;
using System.Collections;
using System;

public class MagicCircle : Area
{
    private Vector2 startPos;
    public int duration;

    //projectile speed

    void Start()
    {
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        // this calculates how long the circle stays for
        if (--duration < 0)
        {
            Destroy(gameObject);
        }
    }

    protected void OnTriggerStay2D(Collider2D other) 
    {
        // if same team
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID == other.gameObject.GetComponent<BaseClass>().team)
        {
            var b = other.gameObject.GetComponent<MagicBuff>();
            if (b == null)
            {
                other.gameObject.AddComponent<MagicBuff>();
            }
            else
            {
                b.duration = 150;
            }
            return;
        } 
        else if (other.gameObject.GetComponent<BaseClass>() != null && teamID != other.gameObject.GetComponent<BaseClass>().team)
        {
            var db = other.gameObject.GetComponent<MagicDebuff>();
            if (db == null)
            {
                other.gameObject.AddComponent<MagicDebuff>();
            }
            else
            {
                db.duration = 150;
            }
            return;
        }
    }

    protected void OnTriggerExit2D(Collider2D other) {
        // if same team
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID == other.gameObject.GetComponent<BaseClass>().team)
        {
            var b = other.gameObject.GetComponent<MagicBuff>();
            if (b != null)
            {
                b.duration = -1;
            }
            return;
        } 
        else if (other.gameObject.GetComponent<BaseClass>() != null && teamID != other.gameObject.GetComponent<BaseClass>().team)
        {
            var db = other.gameObject.GetComponent<MagicDebuff>();
            if (db != null)
            {
                db.duration = -1;
            }
            return;
        }
    }
}
