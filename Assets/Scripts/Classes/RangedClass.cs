/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    RangedClass.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
--      float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
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
--  This class is the parent class for the gunner and mage classes.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public abstract class RangedClass : BaseClass
{
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
    public override float basicAttack(Vector2 dir, Vector2 playerLoc)
    {
        base.basicAttack(dir,playerLoc);
        StartAttackAnimation();
        CancelInvoke("EndAttackAnimation");
        Invoke("EndAttackAnimation", cooldowns[0] * 1.1f);

        return cooldowns[0];
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: specialAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: April 4, 2016
    --              Hank: Added Albert Easter egg
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe, Hank Lo
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe, Hank Lo
    --
    -- INTERFACE: float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    --              Vector2 dir: A Vector2 representing the direction of the attack
    --              Vector2 playerLoc: The player location represented by a Vector2
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Used to run the code that all classes will execute when doing a special attack
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float specialAttack(Vector2 dir, Vector2 playerLoc)
    {
        base.specialAttack(dir,playerLoc);
        StartAttackAnimation();
        CancelInvoke("EndAttackAnimation");
        Invoke("EndAttackAnimation", cooldowns[1] * 1.1f);
        return cooldowns[1];
    }
}
