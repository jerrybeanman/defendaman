/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    MagicCircle.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start()
--      void FixedUpdate()
--      protected override void OnTriggerStay2D(Collider2D other)
--      protected override void OnTriggerExit2D(Collider2D other)
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
--  This class contains the logic that relates to the Magic Circle that the 
--  wizard class creates
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System;

public class MagicCircle : Area
{
    private Vector2 startPos;
    public int duration;

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Start
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: void Start(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when the circle is created - this function initializes the start position of the circle, and 
    -- removes the circle after 3 seconds
    ---------------------------------------------------------------------------------------------------------------------*/
    void Start()
    {
        startPos = transform.position;
        Invoke("removeCircle", duration);
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: removeCircle
    --
    -- DATE: March 16, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: void removeCircle(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when the object needs to be removed - this function removes the magic circle.
    ---------------------------------------------------------------------------------------------------------------------*/
    void removeCircle() {
        Destroy(gameObject);
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerStay2D
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: protected override void OnTriggerStay2D(Collider2D other)
    --                  Collider2D other: The collider of the object that we hit
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when this projectile collides with something - The magic circle applies a debuff/buff to 
    -- enemies/allies 
    ---------------------------------------------------------------------------------------------------------------------*/
    protected void OnTriggerStay2D(Collider2D other) 
    {
        var target = other.gameObject.GetComponent<BaseClass>();
        // if same team
        if (target != null && teamID == target.team)
        {
            var buff = other.gameObject.GetComponent<MagicBuff>();
            if (buff == null)
            {
                other.gameObject.AddComponent<MagicBuff>();
            }
            else
            {
                buff.duration = 150;
            }
            return;
        } 
        //if other team
        else if (target != null && teamID != target.team)
        {
            var debuff = other.gameObject.GetComponent<MagicDebuff>();
            if (debuff == null)
            {
                other.gameObject.AddComponent<MagicDebuff>();
            }
            else
            {
                debuff.duration = 150;
            }
            return;
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerExit2D
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: protected override void OnTriggerExit2D(Collider2D other)
    --                  Collider2D other: The collider of the object that we hit
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when this projectile detects that something has stopped colliding with it - 
    -- The magic circle removes the debuff/buff to enemies/allies if it was applied before
    ---------------------------------------------------------------------------------------------------------------------*/
    protected void OnTriggerExit2D(Collider2D other) {
        var target = other.gameObject.GetComponent<BaseClass>();
        // if same team
        if (target != null && teamID == target.team)
        {
            var buff = other.gameObject.GetComponent<MagicBuff>();
            if (buff != null)
            {
                buff.duration = -1;
            }
            return;
        } 
        // if other team
        else if (target != null && teamID != target.team)
        {
            var debuff = other.gameObject.GetComponent<MagicDebuff>();
            if (debuff != null)
            {
                debuff.duration = -1;
            }
            return;
        }
    }
}
