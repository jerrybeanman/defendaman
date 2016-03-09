using UnityEngine;
using System.Collections;

public class Burn : Debuff
{
    void Start()
    {
        duration = 60;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        player.doDamage(0.5f);
    }
}
