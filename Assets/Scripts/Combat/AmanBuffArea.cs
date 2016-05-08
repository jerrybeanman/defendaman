/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    AmanBuffArea.cs
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
--  This class is the Aman's area of effect buff class. It inherits from Area to apply 
--  the team buff to valid players.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System;

public class AmanBuffArea : Area
{
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
    -- Function to check for targets that we can apply the buff to.
    ---------------------------------------------------------------------------------------------------------------------*/
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        var target = other.GetComponent<BaseClass>();
        if (target != null && teamID == target.team)
        {
            var buff = other.gameObject.GetComponent<AmanTeamBuff>();
            if (buff != null)
            {
                buff.duration = 75;
            }
            else
            {
                other.gameObject.AddComponent<AmanTeamBuff>();
            }
        }
    }
}
