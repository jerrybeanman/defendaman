using UnityEngine;
using System.Collections;

public class WizardClass : BaseClass
{
	public WizardClass()
	{

	}

    //attacks return time it takes to execute
    public override float basicAttack()
    {

        return 0;
    }

    public override float[] specialAttack()
    {

        return new float[2] { 0.5f, 2 };
    }
}
