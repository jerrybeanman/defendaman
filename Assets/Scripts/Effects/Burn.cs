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
        if ((duration % 10) == 0)
        player.doDamage(1f);
    }
}
