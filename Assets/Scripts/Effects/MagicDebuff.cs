/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    MagicDebff.cs
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
--
--  DESIGNERS:      Hank Lo
--
--  PROGRAMMER:     Hank Lo
--
--  NOTES:
--  This class contains the logic that relates to the Magic Debuff Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MagicDebuff : Buff {

    int speeddebuff = 5;
    int atkdebuff = 4;
    int defdebuf = 6;
    int applyrate;

    bool appliedspeedbuff = false;

    void Start() {
        magnitude = 0;
        duration = 150;
        applyrate = 0;
    }
    
    //Called every frame
    protected override void FixedUpdate() {
        applyrate++;
        if (!appliedspeedbuff) 
        {
            player.ClassStat.MoveSpeed -= speeddebuff;
            appliedspeedbuff = true;
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
            Destroy(this);
        }
    }
}

