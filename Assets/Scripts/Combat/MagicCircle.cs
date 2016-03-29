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
		transform.position = new Vector3(transform.position.x, transform.position.y, -2);
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
        // if same team
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID == other.gameObject.GetComponent<BaseClass>().team)
        {
            var b = other.gameObject.GetComponent<MagicBuff>();
            if (b == null)
            {
                other.gameObject.AddComponent<MagicBuff>();
            }
            else
            {
                b.duration = 150;
            }
            return;
        } 
        else if (other.gameObject.GetComponent<BaseClass>() != null && teamID != other.gameObject.GetComponent<BaseClass>().team)
        {
            var db = other.gameObject.GetComponent<MagicDebuff>();
            if (db == null)
            {
                other.gameObject.AddComponent<MagicDebuff>();
            }
            else
            {
                db.duration = 150;
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
        // if same team
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID == other.gameObject.GetComponent<BaseClass>().team)
        {
            var b = other.gameObject.GetComponent<MagicBuff>();
            if (b != null)
            {
                b.duration = -1;
            }
            return;
        } 
        else if (other.gameObject.GetComponent<BaseClass>() != null && teamID != other.gameObject.GetComponent<BaseClass>().team)
        {
            var db = other.gameObject.GetComponent<MagicDebuff>();
            if (db != null)
            {
                db.duration = -1;
            }
            return;
        }
    }
}
