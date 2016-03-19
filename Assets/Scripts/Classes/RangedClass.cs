using UnityEngine;
using System.Collections;

public abstract class RangedClass : BaseClass
{
    public override float basicAttack(Vector2 dir)
    {
        base.basicAttack(dir);
        StartAttackAnimation();
        CancelInvoke("EndAttackAnimation");
        Invoke("EndAttackAnimation", cooldowns[0] * 1.1f);

        return cooldowns[0];
    }

    public override float specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);
        StartAttackAnimation();
        CancelInvoke("EndAttackAnimation");
        Invoke("EndAttackAnimation", cooldowns[1] * 1.1f);
        return cooldowns[1];
    }
}
