﻿using UnityEngine;
using System.Collections;

public class GunnerClass : RangedClass
{
    int[] distance = new int[2]{ 10, 20};
    int[] speed = new int[2] { 100, 200 };
    Rigidbody2D bullet = (Rigidbody2D)Resources.Load("Prefabs/Bullet", typeof(Rigidbody2D));
    Rigidbody2D bullet2 = (Rigidbody2D)Resources.Load("Prefabs/Bullet2", typeof(Rigidbody2D));

    //(add hank to revisions for whoever wrote this class)
    public GunnerClass()
	{
        this._className = "Gunner";
        this._classDescription = "Boom - Headshot.";
        this._classStat.CurrentHp = 100;
        this._classStat.MaxHp = 100;

        //placeholder numbers
        this._classStat.MoveSpeed = 15;
        this._classStat.AtkPower = 15;
        this._classStat.Defense = 5;

        var controller = Resources.Load("Controllers/gunboi") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
        cooldowns = new float[2] { 0.5f, 2 };
    }

    //attacks return time it takes to execute
    public override float basicAttack(Vector2 dir)
    {
        base.basicAttack(dir);
        //float speed = 100;

        //var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
        attack.AddForce(dir * speed[0]);
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower;
        attack.GetComponent<BasicRanged>().maxDistance = distance[0];

        return cooldowns[0];
    }

    public override float specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);
        //parameters: dir, team, damage, range
        //float speed = 200;

        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet2, transform.position, transform.rotation);
        attack.AddForce(dir * speed[1]);
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower * 3;
        attack.GetComponent<BasicRanged>().maxDistance = distance[1];

        return cooldowns[1];
    }
} 
