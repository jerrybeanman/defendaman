/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Fireball.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start()
--      void Update()
--      protected override void OnTriggerStay2D(Collider2D other)
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
--  This class contains the logic that relates to the Fireball projectile that the 
--  wizard class shoots
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System;

public class Fireball : Projectile
{
    private Vector2 startPos;
    public int maxDistance;

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
    -- Function that's called when the projectile is created - this function initializes the start position of the projectile
    ---------------------------------------------------------------------------------------------------------------------*/
    void Start()
    {
        startPos = transform.position;
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Update
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: void Update(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called every frame - it checks if the projectile has traveled the max distance before removing it
    -- from the scene
    ---------------------------------------------------------------------------------------------------------------------*/
    void Update()
    {
        if (Vector2.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerEnter2D
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo
    --
    -- PROGRAMMER: Hank Lo
    --
    -- INTERFACE: protected override void OnTriggerEnter2D(Collider2D other)
    --                  Collider2D other: The collider of the object that we hit
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when this projectile collides with something - the fireball applies a burn debuff to the 
    -- enemy it hits
    ---------------------------------------------------------------------------------------------------------------------*/
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if (other.gameObject.GetComponent<BaseClass>() != null && teamID != other.gameObject.GetComponent<BaseClass>().team)
        {
            var burn = other.gameObject.GetComponent<Burn>();
            if (burn == null)
            {
                other.gameObject.AddComponent<Burn>();
            }
            else
            {
                burn.duration = 600;
            }
        }
    }
}
