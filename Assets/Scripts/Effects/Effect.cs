/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Effect.cs
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
--  This class is the parent class of all effects (Buffs and debuffs) and is meant to be
--  overridden
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public abstract class Effect : MonoBehaviour
{
    public int source;  //source player ID
    public BaseClass player;
    public int magnitude;
    public int duration;

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
    protected void Start()
    {
        player = gameObject.GetComponent<BaseClass>();
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
    protected virtual void FixedUpdate()
    {
        if(--duration < 0)
        {
            Destroy(this);
        }
    }
}
