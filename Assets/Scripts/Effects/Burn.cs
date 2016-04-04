using UnityEngine;
using System.Collections;

public class Burn : Debuff
{
    float damage;
    Resource tree;

    new void Start()
    {
        base.Start();
        damage = 10;
        duration = 150;
        if (player == null)
        {
            tree = gameObject.GetComponent<Resource>();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(duration % 25 == 0) {
            if (player != null)
            {
                player.doDamage(damage, true);
            }
            else if (tree != null)
            {
                tree.SendResourceTakenMessage((int)damage);
            }
        }
    }
}
