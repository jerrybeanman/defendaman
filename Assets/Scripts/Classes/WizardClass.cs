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
--  DESIGNERS:      Hank Lo, Allen Tsang
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
    int[] distance = new int[2]{ 25, 0 };
    int[] speed = new int[2] { 100, 0 };
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

        //Player specific initialization
        if (playerID == GameData.MyPlayer.PlayerID)
        {
            //Starting item kit
            Inventory.instance.AddItem(1);
            Inventory.instance.AddItem(5, 5);
            Inventory.instance.AddItem(6);
            Inventory.instance.AddItem(7);
        }

        //add wizard attack sound clip
        au_simple_attack = Resources.Load("Music/Weapons/magegirl_staff_primary") as AudioClip;
        au_special_attack = Resources.Load("Music/Weapons/magegirl_staff_secondary") as AudioClip;
    }


    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: basicAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: float basicAttack(Vector2 dir)
    --              dir: a vector2 object which shows the direction of the attack
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Function that's called when the wizard uses the left click attack
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        if (playerLoc == default(Vector2))
            playerLoc = dir;
        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
        base.basicAttack(dir, playerLoc);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(fireball, transform.position, transform.rotation);
        attack.AddForce(dir * speed[0]);
        attack.GetComponent<Fireball>().playerID = playerID;
        attack.GetComponent<Fireball>().teamID = team;
        attack.GetComponent<Fireball>().damage = ClassStat.AtkPower;
        attack.GetComponent<Fireball>().maxDistance = distance[0];

		print (attack.GetComponent<Fireball>().playerID);
		
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
    -- Function that's called when the wizard uses the right click special attack (magic circle)
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        if (playerLoc == default(Vector2))
            playerLoc = dir;
        base.specialAttack(dir,playerLoc);

        //Vector2 mousePos = Input.mousePosition;
        //mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        //var distance = (dir - (Vector2) transform.position).magnitude;
        //Vector2 endp = (Vector2) transform.position + (distance * dir);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(magicCircle, dir, Quaternion.identity);
        attack.GetComponent<MagicCircle>().playerID = playerID;
        attack.GetComponent<MagicCircle>().teamID = team;
        attack.GetComponent<MagicCircle>().damage = 0;

        return cooldowns[1];
    }
}
