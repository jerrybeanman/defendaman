using UnityEngine;
using System.Collections;

public class WizardClass : BaseClass
{
	public WizardClass()
	{
        this._className = "Wizard";
        this._classDescription = "Test class Wizard";
        this._classStat.CurrentHp = 50;
        this._classStat.MaxHp = 150;

        //placeholder numbers
        this._classStat.MoveSpeed = 10;
        this._classStat.AtkPower = 15;
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
