/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Melee.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      override void OnTriggerEnter2D(Collider2D other)
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
--  This class is the melee trigger class used for the ninja. 
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections.Generic;

public abstract class Melee : Trigger
{
    public HashSet<GameObject> targets = new HashSet<GameObject>();

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerEnter2D
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: OnTriggerEnter2D(Collider2D other)
    --
    -- RETURNS: N/A
    --
    -- NOTES:
    -- Empty function overriden because the parent is an abstract class.
    ---------------------------------------------------------------------------------------------------------------------*/
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        //cleaving attack, deactivate collider at end of swing in animator
    }
}
