/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    AmanTeamBuff.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start()
--      override void FixedUpdate()
--
--  DATE:           April 2, 2016
--
--  REVISIONS:      (Date and Description)
--                  April 4th: Hank Lo
--                      - Numbers balancing
--
--  DESIGNERS:      Allen Tsang, Hank Lo
--
--  PROGRAMMER:     Allen Tsang, Hank Lo
--
--  NOTES:
--  This class contains the logic that relates to the Aman Team Buff Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AmanTeamBuff : Buff
{
    int attackBuff = 7;
    int defenseBuff = 7;
    int speedBuff = 1;
    BaseClass target;

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
    -- Start of scripts creation. Used to instantiate variables in our case.
    ---------------------------------------------------------------------------------------------------------------------*/
    new void Start()
    {
        base.Start();
        target = gameObject.GetComponent<BaseClass>();
        target.ClassStat.AtkPower += attackBuff;
        target.ClassStat.Defense += defenseBuff;
        target.ClassStat.MoveSpeed += speedBuff;
        duration = 75;
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: FixedUpdate
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void FixedUpdate(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Called every physics update.
    ---------------------------------------------------------------------------------------------------------------------*/
    protected override void FixedUpdate()
    {
        if (--duration < 0)
        {
            player.ClassStat.AtkPower -= attackBuff;
            player.ClassStat.Defense -= defenseBuff;
            player.ClassStat.MoveSpeed -= speedBuff;
            Destroy(this);
        }
    }
}

