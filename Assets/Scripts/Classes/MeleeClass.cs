/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    MeleeClass.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
--
--  DATE:           March 9, 2016
--
--  REVISIONS:
--
--  DESIGNERS:      Hank Lo, Allen Tsang
--
--  PROGRAMMER:     Hank Lo, Allen Tsang
--
--  NOTES:
--  This class is the parent class for the ninja class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public abstract class MeleeClass : BaseClass
{
    protected Melee attackMelee;

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: basicAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe
    --
    -- INTERFACE: float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    --              Vector2 dir: A Vector2 representing the direction of the attack
    --              Vector2 playerLoc: The player location represented by a Vector2
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Used to run the code that all classes will execute when doing a basic attack
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        //Clear melee attack list
        attackMelee.targets.Clear();

        base.basicAttack(dir, playerLoc);
        return cooldowns[0];
    }
}
