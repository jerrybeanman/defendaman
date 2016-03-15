using UnityEngine;
using System.Collections;

public class NinjaClass : MeleeClass
{
	public NinjaClass()
	{
        this._className = "Ninja";
        this._classDescription = "Test class Ninja";
        this._classStat.CurrentHp = 50;
        this._classStat.MaxHp = 150;

        //placeholder numbers
        this._classStat.MoveSpeed = 25;
        this._classStat.AtkPower = 15;
        this._classStat.Defense = 5;

        var controller = Resources.Load("Controllers/ninjaboi") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;

        cooldowns = new float[2] { 0.95f, 2 };
    }

    //attacks return time it takes to execute
    public override float basicAttack(Vector2 dir)
    {
        base.basicAttack(dir);
        return cooldowns[0];
    }

    public override float specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);
        gameObject.GetComponent<Movement>().doBlink(15f);
        return cooldowns[1];
    }
}
