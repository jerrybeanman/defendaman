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
--
--  DESIGNERS:      Hank Lo
--
--  PROGRAMMER:     Hank Lo
--
--  NOTES:
--  This class contains the logic that relates to the Magic Buff Class.
---------------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MagicBuff : Buff {

    int speedbuff = 5;
    int atkbuff = 5;
    int defbuf = 8;
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
            player.ClassStat.AtkPower -= (magnitude*atkbuff);
            player.ClassStat.Defense -= (magnitude*defbuf);
            player.ClassStat.MoveSpeed -= speedbuff;
            Destroy(this);
        }
    }
}

