using UnityEngine;
using System.Collections.Generic;

public abstract class Melee : Trigger
{
    public HashSet<GameObject> targets = new HashSet<GameObject>();

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //cleaving attack, deactivate collider at end of swing in animator
    }
}
