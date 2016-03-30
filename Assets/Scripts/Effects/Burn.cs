using UnityEngine;
using System.Collections;

public class Burn : Debuff
{
    void Start()
    {
        duration = 150;
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(duration % 25 == 0) {
            player.doDamage(10f, true);
        }
    }
}
