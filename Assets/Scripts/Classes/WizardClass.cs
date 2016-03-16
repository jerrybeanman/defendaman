/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    WizardClass.cs
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
--  PROGRAMMER:     Hank Lo
--
--  NOTES:
--  This class contains the logic that relates to the Wizard Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public class WizardClass : BaseClass
{
    int[] distance = new int[2]{ 20, 0 };
    int[] speed = new int[2] { 80, 0 };
    Rigidbody2D fireball = (Rigidbody2D)Resources.Load("Prefabs/Fireball", typeof(Rigidbody2D));
    Rigidbody2D magicCircle = (Rigidbody2D)Resources.Load("Prefabs/magic_circle", typeof(Rigidbody2D));

	public WizardClass()
	{
        this._className = "Wizard";
        this._classDescription = "Wingardium Leviosa. No, not leviosAA, leviOsa.";
        this._classStat.CurrentHp = 75;
        this._classStat.MaxHp = 75;

        //placeholder numbers
        this._classStat.MoveSpeed = 10;
        this._classStat.AtkPower = 3;
        this._classStat.Defense  = 5;

        var controller = Resources.Load("Controllers/magegirl") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
        cooldowns = new float[2] { 0.5f, 6 };
	}

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: basicAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: float basicAttack(Vector2 dir)
    --              dir: a vector2 object which shows the direction of the attack
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Function that's called when the wizard uses the left click attack
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float basicAttack(Vector2 dir)
    {
        base.basicAttack(dir);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(fireball, transform.position, transform.rotation);
        attack.AddForce(dir * speed[0]);
        attack.GetComponent<Fireball>().teamID = team;
        attack.GetComponent<Fireball>().damage = ClassStat.AtkPower;
        attack.GetComponent<Fireball>().maxDistance = distance[0];

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
    -- Function that's called when the wizard uses the right click special attack (magic circle)
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10.0f;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(magicCircle, mousePos, Quaternion.identity);
        attack.GetComponent<MagicCircle>().teamID = team;
        attack.GetComponent<MagicCircle>().damage = ClassStat.AtkPower * 0;
        attack.GetComponent<MagicCircle>().duration = 200;

        return cooldowns[1];
    }
}
