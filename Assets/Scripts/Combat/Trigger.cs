using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;

public abstract class Trigger : MonoBehaviour
{
    //player ID
    public int playerID;
    public float damage;
    public int teamID = 0;
    public static int currentTriggerID = 1;
    public int triggerID;

    public Trigger()
    {
        triggerID = currentTriggerID++;
    }

    protected abstract void OnTriggerEnter2D(Collider2D other);
}
