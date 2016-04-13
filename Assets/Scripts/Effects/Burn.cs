/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Burn.cs
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
--  DESIGNERS:      Allen Tsang, Hank Lo
--
--  PROGRAMMER:     Allen Tsang, Hank Lo
--
--  NOTES:
--  This class is the burn debuff that is applied by the fireballs that mages throw
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public class Burn : Debuff
{
    public float damage;
    Resource tree;

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
        // .8 scaling ratio based on the attack value of the mage - this attack does true damage, apr4th, hank
        damage *= 0.8f;

        duration = 150;
        if (player == null)
        {
            tree = gameObject.GetComponent<Resource>();
        }
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
        base.FixedUpdate();
        if(duration % 25 == 0) {
            if (player != null)
            {
                player.doDamage(damage, true);
            }
            else if (tree != null)
            {
                tree.SendResourceTakenMessage((int)damage);
            }
        }
    }
}
