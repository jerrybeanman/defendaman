/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Trigger.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Trigger()
--      abstract void OnTriggerEnter2D(Collider2D other)
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
--  This class is an abstract class for Triggers (collision handling objects in Unity).
--  This class is basically empty because it's meant to be overrided.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System;

public abstract class Trigger : MonoBehaviour
{
    //player ID
    public int playerID;
    public float damage;
    public int teamID = 0;
    public static int currentTriggerID = 1;
    public int triggerID;

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Trigger
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: Trigger()
    --
    -- RETURNS: N/A
    --
    -- NOTES:
    -- Constructor for the Trigger object
    ---------------------------------------------------------------------------------------------------------------------*/
    public Trigger()
    {
        triggerID = currentTriggerID++;
    }

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
    -- Abstract function meant to be overridden
    ---------------------------------------------------------------------------------------------------------------------*/
    protected abstract void OnTriggerEnter2D(Collider2D other);
}
