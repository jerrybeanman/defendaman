/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    NinjaClass.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      override float basicAttack(Vector2 dir)
--      override float specialAttack(Vector2 dir)
--
--  DATE:           March 9, 2016
--
--  REVISIONS:      (Date and Description)
--
--  DESIGNERS:      Hank Lo
--
--  PROGRAMMER:     Hank Lo, Allen Tsang
--
--  NOTES:
--  This class contains the logic that relates to the Ninja Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public class NinjaClass : MeleeClass
{
	public NinjaClass()
	{
        this._className = "Ninja";
        this._classDescription = "You'll never see him coming.";
        this._classStat.CurrentHp = 150;
        this._classStat.MaxHp = 150;

        //placeholder numbers
        this._classStat.MoveSpeed = 20;
        this._classStat.AtkPower = 20;
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

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: specialAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: float specialAttack(Vector2 dir)
    --              dir: a vector2 object which shows the direction of the attack
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Function that's called when the Ninja uses the right click special attack (Teleport). If the ninja is debuffed he
    -- cannot teleport, but the cooldown will still be used since he tried to.
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);
        if (gameObject.GetComponent<MagicDebuff>() == null) {
            gameObject.GetComponent<Movement>().doBlink(15f);
        }
        return cooldowns[1];
    }
}
