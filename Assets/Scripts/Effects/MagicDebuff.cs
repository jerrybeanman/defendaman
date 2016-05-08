/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    MagicDebuff.cs
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
--                   April 4, 2016: Hank Lo
--                      - Numbers balancing, silence implemention
--
--  DESIGNERS:      Hank Lo, Allen Tsang
--
--  PROGRAMMER:     Hank Lo, Allen Tsang
--
--  NOTES:
--  This class contains the logic that relates to the Magic Debuff Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MagicDebuff : Buff {

    int speeddebuff = 2;
    int atkdebuff = 3;
    int defdebuf = 3;
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
            player.ClassStat.MoveSpeed -= speeddebuff;
            appliedspeedbuff = true;
            player.silenced = true;
        }
        if ((applyrate % 30) == 0) 
        {
            if (magnitude < 10) {
                if ((player.ClassStat.AtkPower > 10) && (player.ClassStat.Defense > 10)) {
                    player.ClassStat.AtkPower -= (atkdebuff);
                    player.ClassStat.Defense -= (defdebuf);
                    magnitude++;
                }
            }
        }
        if(--duration < 0)
        {
            player.ClassStat.AtkPower += (magnitude * atkdebuff);
            player.ClassStat.Defense += (magnitude * defdebuf);
            player.ClassStat.MoveSpeed += speeddebuff;
            player.silenced = false;
            Destroy(this);
        }
    }
}

