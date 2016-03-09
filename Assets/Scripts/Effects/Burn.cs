using UnityEngine;
using System.Collections;

public class Burn : Debuff
{
    void Start()
    {
        duration = 60;
    }

    protected override void Update()
    {
        base.Update();
        player.doDamage(0.5f);
    }
}
