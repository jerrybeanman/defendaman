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
--  REVISIONS:      March 29, 2016
--                      Added teleportation animation
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
    GameObject teleport;
    GameObject teleportInstance;

    new void Start()
    {
        base.Start();
        sword = (Rigidbody2D)Resources.Load("Prefabs/NinjaSword", typeof(Rigidbody2D));
        teleport = (GameObject)Resources.Load("Prefabs/NinjaTeleport", typeof(GameObject));

        attack = (Rigidbody2D)Instantiate(sword, transform.position, transform.rotation);
        attack.GetComponent<BoxCollider2D>().enabled = false;
        attack.GetComponent<BasicSword>().playerID = playerID;
        attack.GetComponent<BasicSword>().teamID = team;
        attack.GetComponent<BasicSword>().damage = ClassStat.AtkPower;
        attack.transform.parent = transform;

        var controller = Resources.Load("Controllers/ninjaboi") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
    }

    public NinjaClass()
    {
        this._className = "Ninja";
        this._classDescription = "You'll never see him coming.";
        this._classStat.MaxHp = 150;
        this._classStat.CurrentHp = this._classStat.MaxHp;

        //placeholder numbers
        this._classStat.MoveSpeed = 12;
        this._classStat.AtkPower = 20;
        this._classStat.Defense = 5;

        cooldowns = new float[2] { 0.95f, 2 };
    }

    //attacks return time it takes to execute
    public override float basicAttack(Vector2 dir)
    {
        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
        base.basicAttack(dir);

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
    public override float specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);

        teleportInstance = (GameObject)Instantiate(teleport, transform.position, transform.rotation);
        Invoke("cleanupTeleport", 1f);

        if (gameObject.GetComponent<MagicDebuff>() == null) {
            var movement = gameObject.GetComponent<Movement>();
            if (movement != null)
                movement.doBlink(15f);
        }

        return cooldowns[1];
    }

    private void cleanupTeleport()
    {
        Destroy(teleportInstance.gameObject);
    }
}
