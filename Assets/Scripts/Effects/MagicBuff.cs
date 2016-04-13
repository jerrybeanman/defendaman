/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    MagicBuff.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      override void FixedUpdate()
--      void start()
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
--  This class contains the logic that relates to the Magic Buff Class.
---------------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MagicBuff : Buff {

    int speedbuff = 1;
    int atkbuff = 3;
    int defbuf = 3;
    int applyrate;

    bool appliedspeedbuff = false;

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
        magnitude = 0;
        duration = 150;
        applyrate = 0;
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
    protected override void FixedUpdate() {
        applyrate++;
        if (!appliedspeedbuff) 
        {
            player.ClassStat.MoveSpeed += speedbuff;
            appliedspeedbuff = true;
        }
        if ((applyrate % 30) == 0) 
        {
            if (magnitude < 10) {
                player.ClassStat.AtkPower += (atkbuff);
                player.ClassStat.Defense += (defbuf);
                magnitude++;
            }
        }
        if(--duration < 0)
        {
			print ("[DEBUG] MAGIC BUFF: " + player.ClassStat.MoveSpeed);
            player.ClassStat.AtkPower -= (magnitude*atkbuff);
            player.ClassStat.Defense -= (magnitude*defbuf);
            player.ClassStat.MoveSpeed -= speedbuff;
            Destroy(this);
        }
    }
}

