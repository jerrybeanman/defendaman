﻿/*---------------------------------------------------------------------------------------
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
--  REVISIONS:      March 29, 2016
--                      Added teleportation animation
--                  March 31, 2016
--                      Add attack sound   - Eunwon Moon
--
--  DESIGNERS:      Hank Lo, Allen Tsang
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
    Rigidbody2D sword;
    Rigidbody2D attack;
    BasicSword attackSword;
    GameObject teleport;
    GameObject teleportInstance;

    new void Start()
    {
        cooldowns = new float[2] { 0.95f, 2 };

        healthBar = transform.GetChild(0).gameObject.GetComponent<HealthBar>();
        _classStat = new PlayerBaseStat(playerID, healthBar);
        _classStat.MaxHp = 150;
        _classStat.MoveSpeed = 12;
        _classStat.AtkPower = 20;
        _classStat.Defense = 5;

        base.Start();

        sword = (Rigidbody2D)Resources.Load("Prefabs/NinjaSword", typeof(Rigidbody2D));
        teleport = (GameObject)Resources.Load("Prefabs/NinjaTeleport", typeof(GameObject));

        attack = (Rigidbody2D)Instantiate(sword, transform.position, transform.rotation);
        attack.GetComponent<BoxCollider2D>().enabled = false;

        attackSword = attack.GetComponent<BasicSword>();
        attackSword.playerID = playerID;
        attackSword.teamID = team;
        attackSword.damage = ClassStat.AtkPower;
        attack.transform.parent = transform;

        var controller = Resources.Load("Controllers/ninjaboi") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;

        //Player specific initialization
        if (playerID == GameData.MyPlayer.PlayerID)
        {
            //Starting item kit
            Inventory.instance.AddItem(1);
            Inventory.instance.AddItem(5, 5);
            Inventory.instance.AddItem(6);
            Inventory.instance.AddItem(7);
        }

        //ninja attack sound
        au_simple_attack = Resources.Load("Music/Weapons/ninjaboi_sword_primary") as AudioClip;
        au_special_attack = Resources.Load("Music/Weapons/ninjaboi_teleport") as AudioClip;
    }

    //attacks return time it takes to execute
    public override float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        if (playerLoc == default(Vector2))
            playerLoc = dir;
        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
        base.basicAttack(dir, playerLoc);

        attackSword.damage = ClassStat.AtkPower;

        StartAttackAnimation();
        Invoke("EndAttackAnimation", cooldowns[0] / 2);

        return cooldowns[0];
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: specialAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: March 29, 2016
    --              Added teleportation animation
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
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
    public override float specialAttack(Vector2 dir,Vector2 playerLoc = default(Vector2))
    {
        if(playerLoc == default(Vector2))
            playerLoc = dir;

        base.specialAttack(dir, playerLoc);

        teleportInstance = (GameObject)Instantiate(teleport, transform.position, transform.rotation);
        Destroy(teleportInstance, 1);

        if (gameObject.GetComponent<MagicDebuff>() == null) {
            var movement = gameObject.GetComponent<Movement>();
            if (movement != null)
                movement.doBlink(10f);
        }

        return cooldowns[1];
    }
}
