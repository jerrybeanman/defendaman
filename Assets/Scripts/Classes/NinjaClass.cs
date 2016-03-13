using UnityEngine;
using System.Collections;

public class NinjaClass : BaseClass
{
    Rigidbody2D sword = (Rigidbody2D)Resources.Load("Prefabs/NinjaSword", typeof(Rigidbody2D));

    void Start()
    {
        Debug.Log("test start");
        Rigidbody2D attack = (Rigidbody2D)Instantiate(sword, transform.position, transform.rotation);
        attack.transform.parent = transform;
    }

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
<<<<<<< HEAD
        // colins blink
        base.specialAttack(dir);
        gameObject.GetComponent<Movement>().doBlink(20f);
        return cooldowns[1];
=======
        base.specialAttack(dir);
        if (gameObject.GetComponent<MagicDebuff>() == null) {
            gameObject.GetComponent<Movement>().doBlink(20f);
        }

        return 2;
>>>>>>> 5e8a82a7d8dfa847982103a37441048fdaac9386
    }
}
