/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    Attack.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      override void Update()
--      void start()
--      void enableAttack()
--      void enableSpecial()
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
--  This class is the attack script that we attach to players.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Attack : MonoBehaviour {

    BaseClass player;

    bool attackReady = true, specialReady = true;
    float InvLeftEdge;
    float InvTopEdge;

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
    void Start() {
        player = gameObject.GetComponent<BaseClass>();
        InvLeftEdge = GameObject.Find("Inventory Panel").transform.position.x;
        InvTopEdge = GameObject.Find("Title Panel").transform.position.y;
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
    void Update() {
        if (Input.GetKey(KeyCode.Mouse0) && attackReady && !GameData.MouseBlocked
            && (Input.mousePosition.x < InvLeftEdge || Input.mousePosition.y > InvTopEdge))
        {
            //if (Input.GetKey(KeyCode.Mouse0) && attackReady && !GameData.MouseBlocked) {

            //left click attack

            HUD_Manager.instance.UseMainSkill(player.cooldowns[0]);
            attackReady = false;
            //var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            var dir = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector2 playerLoc = (Vector2)GameData.PlayerPosition[GameData.MyPlayer.PlayerID];
            float delay = player.basicAttack(dir, playerLoc);
            Invoke("enableAttack", delay);
            NetworkingManager.send_next_packet(DataType.Trigger, player.playerID, new List<Pair<string, string>> {
                new Pair<string, string>("Attack", "0"),
                new Pair<string, string>("DirectionX", dir.x.ToString()),
                new Pair<string, string>("DirectionY", dir.y.ToString())
            }, Protocol.UDP);
        } 

        if (Input.GetKey(KeyCode.Mouse1) && specialReady && !GameData.MouseBlocked
            && (Input.mousePosition.x < InvLeftEdge || Input.mousePosition.y > InvTopEdge))
        {
            //right click attack
            HUD_Manager.instance.UseSubSkill(player.cooldowns[1]);
            specialReady = false;
            //var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            var dir = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 playerLoc = (Vector2)GameData.PlayerPosition[GameData.MyPlayer.PlayerID];
            float delay = player.specialAttack(dir, playerLoc);
            Invoke("enableSpecial", delay);
            NetworkingManager.send_next_packet(DataType.Trigger, player.playerID, new List<Pair<string, string>> {
                new Pair<string, string>("Attack", "1"),
                new Pair<string, string>("DirectionX", dir.x.ToString()),
                new Pair<string, string>("DirectionY", dir.y.ToString()),
            }, Protocol.UDP);
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: enableAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void enableAttack(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function to enable the normal attack
    ---------------------------------------------------------------------------------------------------------------------*/
    private void enableAttack()
    {
        attackReady = true;
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: enableSpecial
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void enableSpecial(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function to enable the special attack
    ---------------------------------------------------------------------------------------------------------------------*/
    private void enableSpecial()
    {
        specialReady = true;
    }
}

