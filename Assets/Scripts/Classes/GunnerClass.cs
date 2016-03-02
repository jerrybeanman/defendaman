using UnityEngine;
using System.Collections;

public class GunnerClass : BaseClass
{
    Rigidbody2D bullet = (Rigidbody2D)Resources.Load("Bullet", typeof(Rigidbody2D));
    Rigidbody2D bullet2 = (Rigidbody2D)Resources.Load("Bullet2", typeof(Rigidbody2D));

    public GunnerClass()
	{
        this._className = "Gunner";
        this._classDescription = "Test class Gunner";
        this._classStat.CurrentHp = 50;
        this._classStat.MaxHp = 150;

        //placeholder numbers
        this._classStat.MoveSpeed = 5;
        this._classStat.AtkPower = 20;


    }

    //attacks return time it takes to execute
    public override float basicAttack()
    {
        float speed = 100;

        var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
        attack.AddForce(dir * speed);
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower;
        attack.GetComponent<BasicRanged>().maxDistance = 10;

        return 0.5f;
    }

    public override float[] specialAttack()
    {
        //parameters: dir, team, damage, range
        float speed = 200;

        var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet2, transform.position, transform.rotation);
        attack.AddForce(dir * speed);
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower * 3;
        attack.GetComponent<BasicRanged>().maxDistance = 20;

        return new float[2] { 0.5f, 2 };
    }
} 
