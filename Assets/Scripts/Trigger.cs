using UnityEngine;
using System.Collections;

public abstract class Trigger : MonoBehaviour
{
    public int teamID;
    public float damage;

    public Trigger()
    {
        
    }

    protected abstract void OnTriggerEnter2D(Collider2D other);
}
