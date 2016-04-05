﻿/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Fireball.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      protected override void OnTriggerStay2D(Collider2D other)
--
--  DATE:           March 9, 2016
--
--  REVISIONS:      March 18, 2016
--                      Refactored out duplicate code, edited burn script
--
--  DESIGNERS:      Hank Lo, Allen Tsang
--
--  PROGRAMMER:     Hank Lo, Allen Tsang
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
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:        OnTriggerEnter2D
    --
    -- DATE:            March 9, 2016
    --
    -- REVISIONS:       March 18, 2016
    --                      Edited burn script
    --
    -- DESIGNER:        Hank Lo, Allen Tsang
    --
    -- PROGRAMMER:      Hank Lo, Allen Tsang
    --
    -- INTERFACE:       protected override void OnTriggerEnter2D(Collider2D other)
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
        var target = other.gameObject.GetComponent<BaseClass>();
        var tree = other.gameObject.GetComponent<Resource>();
        if ((target != null && teamID != target.team) || tree != null)
        {
            var burn = other.gameObject.GetComponent<Burn>();
            if (burn == null)
            {
                other.gameObject.AddComponent<Burn>();
            }
            else
            {
                burn.duration = 150;
            }
        }
    }
}
