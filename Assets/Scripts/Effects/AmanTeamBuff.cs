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
--
--  DESIGNERS:      Allen Tsang
--
--  PROGRAMMER:     Allen Tsang
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
    int attackBuff = 5;
    int defenseBuff = 5;
    int speedBuff = 1;
    BaseClass target;

    new void Start()
    {
        base.Start();
        target = gameObject.GetComponent<BaseClass>();
        target.ClassStat.AtkPower += attackBuff;
        target.ClassStat.Defense += defenseBuff;
        target.ClassStat.MoveSpeed += speedBuff;
        duration = 75;
    }

    // Called every physics update
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

