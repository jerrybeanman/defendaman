/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    WizardClass.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start(void)
--      override float basicAttack(Vector2 dir)
--      override float specialAttack(Vector2 dir)
--
--  DATE:           March 9, 2016
--
--  REVISIONS:      (Date and Description)
--                   April 4th: Hank Lo
--                      - Numbers balancing
--
--  DESIGNERS:      Hank Lo, Allen Tsang
--
--  PROGRAMMER:     Hank Lo, Allen Tsang
--
--  NOTES:
--  This class contains the logic that relates to the Wizard Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;

public class WizardClass : RangedClass
{
    int[] distance = new int[2]{ 15, 0 };
    int[] speed = new int[2] { 100, 0 };
    Rigidbody2D fireball;
    Rigidbody2D magicCircle;

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Start
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void Start(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when the script is first executed - it initializes all required values
    ---------------------------------------------------------------------------------------------------------------------*/
    new void Start()
    {
        cooldowns = new float[2] { 0.75f, 15 };

        healthBar = transform.GetChild(0).gameObject.GetComponent<HealthBar>();
        _classStat = new PlayerBaseStat(playerID, healthBar);
        _classStat.MaxHp = 950;
        _classStat.MoveSpeed = 5;
        _classStat.AtkPower = 20;
        _classStat.Defense = 20;

        base.Start();

        fireball = (Rigidbody2D)Resources.Load("Prefabs/Fireball", typeof(Rigidbody2D));
        magicCircle = (Rigidbody2D)Resources.Load("Prefabs/MagicCircle", typeof(Rigidbody2D));

        var controller = Resources.Load("Controllers/magegirl") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;

        //Player specific initialization
        if (playerID == GameData.MyPlayer.PlayerID)
        {
            //Starting item kit
            Inventory.instance.AddItem(17);
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
        var startPosition = new Vector3(transform.position.x + (dir.x * 1.75f), transform.position.y + (dir.y * 1.75f), -5);

        Rigidbody2D instance = (Rigidbody2D)Instantiate(fireball, startPosition, transform.rotation);
        instance.AddForce(dir * speed[0]);
        var attack = instance.GetComponent<Fireball>();
        attack.playerID = playerID;
        attack.teamID = team;
        attack.damage = ClassStat.AtkPower;
        attack.maxDistance = distance[0];
        attack.pierce = 2;

        return cooldowns[0];
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: specialAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS:
    --      - March 17, 2016: Fixed instantiation to work through networking - Carson
    --      - April 4, 2016: Added check for silence for magic circle - Hank
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
        
        if (!silenced) {
            Rigidbody2D instance = (Rigidbody2D)Instantiate(magicCircle, dir, Quaternion.identity);
            var attack = instance.GetComponent<MagicCircle>();
            attack.playerID = playerID;
            attack.teamID = team;
            attack.damage = 0;
            attack.duration = 5;
        }
        return cooldowns[1];
    }
}
