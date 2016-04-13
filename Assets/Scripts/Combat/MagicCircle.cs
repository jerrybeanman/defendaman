/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    MagicCircle.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start()
--      protected override void OnTriggerEnter2D(Collider2D other)
--      protected override void OnTriggerStay2D(Collider2D other)
--      protected override void OnTriggerExit2D(Collider2D other)
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
	public Sprite allyCircle;
	public Sprite enemyCircle;

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
    -- Function that's called when the circle is created - this function initializes the start position of the circle, and 
    -- removes the circle after 6 seconds
    ---------------------------------------------------------------------------------------------------------------------*/
    new void Start()
    {
        duration = 6;
		transform.position = new Vector3(transform.position.x, transform.position.y, -2);
        startPos = transform.position;
        Destroy(gameObject, duration);
		if(teamID == GameData.MyPlayer.TeamID)
		{
			gameObject.GetComponent<SpriteRenderer>().sprite = allyCircle;
			gameObject.GetComponent<ParticleSystem>().startColor = new Color(0f, 0f, 1f, 0.4f);
		}else
		{
			gameObject.GetComponent<SpriteRenderer>().sprite = enemyCircle;
			gameObject.GetComponent<ParticleSystem>().startColor = new Color(1f, 0f, 0f, 0.4f);
		}
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerEnter2D
    --
    -- DATE: April 2, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Allen Tsang
    --
    -- PROGRAMMER: Allen Tsang
    --
    -- INTERFACE: protected override void OnTriggerStay2D(Collider2D other)
    --                  Collider2D other: The collider of the object that we hit
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Empty method, implemented because of inherited abstract method.
    ---------------------------------------------------------------------------------------------------------------------*/
    protected override void OnTriggerEnter2D(Collider2D other) { }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerStay2D
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
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
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
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
