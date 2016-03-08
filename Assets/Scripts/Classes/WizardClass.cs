using UnityEngine;
using System.Collections;

public class WizardClass : BaseClass
{
    // note that max distance for the special attack is meant to be the duration that the magic circle stays 
    int[] distance = new int[2]{ 10, 100};
    int[] speed = new int[2] { 100, 0 };
    Rigidbody2D fireball = (Rigidbody2D)Resources.Load("Prefabs/Bullet", typeof(Rigidbody2D));
    Rigidbody2D magicCircle = (Rigidbody2D)Resources.Load("Prefabs/magic_circle", typeof(Rigidbody2D));

	public WizardClass()
	{
        this._className = "Wizard";
        this._classDescription = "Test class Wizard";
        this._classStat.CurrentHp = 50;
        this._classStat.MaxHp = 150;

        //placeholder numbers
        this._classStat.MoveSpeed = 10;
        this._classStat.AtkPower = 15;

        var controller = Resources.Load("Controllers/magegirl") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
        cooldowns = new float[2] { 0.5f, 2 };
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
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10.0f;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        // mousePos.y = 0.4f;

        Rigidbody2D attack = (Rigidbody2D)Instantiate(magicCircle, mousePos, Quaternion.identity);
        attack.GetComponent<MagicCircle>().teamID = team;
        attack.GetComponent<MagicCircle>().damage = ClassStat.AtkPower * 0;
        attack.GetComponent<MagicCircle>().maxDistance = distance[1];

        return cooldowns[1];
    }
}
