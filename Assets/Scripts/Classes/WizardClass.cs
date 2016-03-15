using UnityEngine;
using System.Collections;

public class WizardClass : BaseClass
{
    // note that max distance for this special attack is meant to be the duration that the magic circle stays 
    int[] distance = new int[2]{ 20, 0 };
    int[] speed = new int[2] { 80, 0 };
    Rigidbody2D fireball = (Rigidbody2D)Resources.Load("Prefabs/Fireball", typeof(Rigidbody2D));
    Rigidbody2D magicCircle = (Rigidbody2D)Resources.Load("Prefabs/magic_circle", typeof(Rigidbody2D));

	public WizardClass()
	{
        this._className = "Wizard";
        this._classDescription = "Test class Wizard";
        this._classStat.CurrentHp = 50;
        this._classStat.MaxHp = 150;

        //placeholder numbers
        this._classStat.MoveSpeed = 10;
        this._classStat.AtkPower = 3;
        this._classStat.Defense  = 5;

        var controller = Resources.Load("Controllers/magegirl") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
        cooldowns = new float[2] { 0.5f, 6 };
	}

    //attacks return time it takes to execute
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
        attack.GetComponent<MagicCircle>().duration = 200;

        return cooldowns[1];
    }
}
