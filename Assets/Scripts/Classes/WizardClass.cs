using UnityEngine;
using System.Collections;

public class WizardClass : BaseClass
{
	public WizardClass()
	{

	}

    //attacks return time it takes to execute
    public override float basicAttack(Vector2 dir)
    {
        base.basicAttack(dir);
        return 0;
    }

    public override float[] specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);
        return new float[2] { 0.5f, 2 };
    }
}
