/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    AmanSelfBuff.cs
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
--  This class contains the logic that relates to the Aman Self Buff Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AmanSelfBuff : Buff {
    
    int hpBuff = 200;
    int attackBuff = 5;
    int defenseBuff = 5;
    int speedBuff = 1;
    BaseClass target;
    Rigidbody2D area;

    void Start() {
        target = gameObject.GetComponent<BaseClass>();
        target.ClassStat.MaxHp += hpBuff;
        target.ClassStat.CurrentHp += hpBuff;
        target.ClassStat.AtkPower += attackBuff;
        target.ClassStat.Defense += defenseBuff;
        target.ClassStat.MoveSpeed += speedBuff;

        area = (Rigidbody2D)Resources.Load("Prefabs/AmanBuffArea", typeof(Rigidbody2D));
    }
    
    // Called every physics update
    protected override void FixedUpdate() {
        //Non-standard duration, counts up
        duration++;
        if (duration % 50 == 0) 
        {
            //Do AoE buff
            Rigidbody2D instance = (Rigidbody2D)Instantiate(area, target.gameObject.transform.position, target.gameObject.transform.rotation);
            instance.GetComponent<AmanBuffArea>().playerID = target.playerID;
            instance.GetComponent<AmanBuffArea>().teamID = target.team;
            instance.GetComponent<AmanBuffArea>().damage = 0;
            Destroy(instance.gameObject, 0.1f);
        }
    }
}

