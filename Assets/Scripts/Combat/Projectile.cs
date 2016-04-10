/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Projectile.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      override void Update()
--      void start()
--      override void OnTriggerEnter2D(Collider2D other)
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
--  This class is the parent class for all projectiles - it handles how a projectile
--  generally behaves.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;

public abstract class Projectile : Trigger
{
    private Vector2 startPos;
    public int maxDistance;
    public int pierce = 0;
    
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
        startPos = transform.position;
        if(GameData.MyPlayer == null)
        {
            Debug.Log("caught null");
            return;
        }
        if (teamID != GameData.MyPlayer.TeamID)
        {
            Material hiddenMat = (Material)Resources.Load("Stencil_01_Diffuse Sprite", typeof(Material));
            gameObject.layer = LayerMask.NameToLayer("HiddenThings");
            gameObject.GetComponent<SpriteRenderer>().material = hiddenMat;
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Update
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void Update(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Called every frame. We deal with checking whether or not the mouse is pressed, or which mouse key is pressed.
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
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: override void OnTriggerEnter2D(Collider2D other)
    --                  Collider2D other: The object's collider that this projectile hits
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Called when an object collides with this projectile. We check what hits it here, and does the appropriate action.
    ---------------------------------------------------------------------------------------------------------------------*/
    protected override void OnTriggerEnter2D(Collider2D other)
    {
        // ignore health bar
        if (other.gameObject.tag == "HealthBar")
        {
            return;
        }
        //If its a player or an AI, ignore it, otherwise destroy itself
        var player = other.gameObject.GetComponent<BaseClass>();
        if (player != null && teamID == player.team)
        {
            //Ignore team players
            return;
        }

        var trigger = other.gameObject.GetComponent<Trigger>();
        if (trigger != null && (teamID == trigger.teamID || trigger is Area))
        {
            //Ignore team attacks or areas
            return;
        }

        var ai = other.gameObject.GetComponent<AI>();
        if (ai != null && teamID == ai.team)
        {
            //Ignore team AIs
            return;
        }

        if (other.gameObject.GetComponent<WorldItemData>() != null)
        {
            //Ignore items
            return;
        }
        //Otherwise, its a wall or some solid

        if (--pierce < 0 || other.name == "obstacleTiles(Clone)" || other.name == "tron_obstacle(Clone)") 
		{
            Destroy(gameObject);
        }
    }
}
