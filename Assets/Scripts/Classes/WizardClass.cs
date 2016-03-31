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
--  PROGRAMMER:     Hank Lo, Allen Tsang
--
--  NOTES:
--  This class contains the logic that relates to the Wizard Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public class WizardClass : RangedClass
{
    int[] distance = new int[2]{ 15, 0 };
    int[] speed = new int[2] { 120, 0 };
    Rigidbody2D fireball;
    Rigidbody2D magicCircle;

    new void Start()
    {
        cooldowns = new float[2] { 0.5f, 6 };
        base.Start();

        _classStat.MaxHp = 100;
        _classStat.CurrentHp = _classStat.MaxHp;
        _classStat.MoveSpeed = 8;
        _classStat.AtkPower = 3;
        _classStat.Defense = 5;
        
        fireball = (Rigidbody2D)Resources.Load("Prefabs/Fireball", typeof(Rigidbody2D));
        magicCircle = (Rigidbody2D)Resources.Load("Prefabs/MagicCircle", typeof(Rigidbody2D));

        var controller = Resources.Load("Controllers/magegirl") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;

        //Starting item kit
        if (playerID == GameData.MyPlayer.PlayerID)
        {
            Inventory.instance.AddItem(1);
            Inventory.instance.AddItem(5, 5);
            Inventory.instance.AddItem(6);
            Inventory.instance.AddItem(7);
        }
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
        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
        base.basicAttack(dir);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(fireball, transform.position, transform.rotation);
        attack.AddForce(dir * speed[0]);
        attack.GetComponent<Fireball>().playerID = playerID;
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
    -- REVISIONS:
    --      - March 17, 2016: Fixed instantiation to work through networking
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

        //Vector2 mousePos = Input.mousePosition;
        //mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        //var distance = (dir - (Vector2) transform.position).magnitude;
        //Vector2 endp = (Vector2) transform.position + (distance * dir);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(magicCircle, dir, Quaternion.identity);
        attack.GetComponent<MagicCircle>().playerID = playerID;
        attack.GetComponent<MagicCircle>().teamID = team;
        attack.GetComponent<MagicCircle>().damage = ClassStat.AtkPower * 0;

        return cooldowns[1];
    }
}
