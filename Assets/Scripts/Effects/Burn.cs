using UnityEngine;
using System.Collections;

public class Burn : Debuff
{
    public float damage;
    Resource tree;

    new void Start()
    {
        base.Start();
        // .8 scaling ratio based on the attack value of the mage - this attack does true damage, apr4th, hank
        damage *= 0.8f;

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
