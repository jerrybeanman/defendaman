using UnityEngine;
using System.Collections;

public abstract class Melee : Trigger
{
    public Melee()
    {
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //cleaving attack, deal with destruction in animator
        Debug.Log("Melee hit");
    }

    void Update()
    {
        //transform.position = transform.parent.position;
    }
}
