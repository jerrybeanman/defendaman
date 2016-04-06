using UnityEngine;
using System.Collections;

public abstract class MeleeClass : BaseClass
{
    protected Melee attackMelee;

    public override float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        //Clear melee attack list
        attackMelee.targets.Clear();

        base.basicAttack(dir, playerLoc);
        return cooldowns[0];
    }
}
