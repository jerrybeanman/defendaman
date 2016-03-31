using UnityEngine;
using System.Collections;

public abstract class Melee : Trigger
{
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //cleaving attack, deactivate collider at end of swing in animator
    }
}
