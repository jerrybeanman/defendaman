using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

/*-----------------------------------------------------------------------------
-- WorldItemData.cs - Script attached to WorldItem game objects that store 
--                    information about world items such as the Item object it 
--                    holds, the amount and its unique world item id. Also
--                    Handles the events that get triggered when the player 
--                    come into contact with it.
--
-- FUNCTIONS:
--		void Start()
--		void Update ()
--		void OnTriggerEnter2D(Collider2D other)
--      void OnTriggerExit2D(Collider2D other)
--
-- DATE:		05/03/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/

public class WorldItemData : MonoBehaviour
{
    public Item item;
    public int amount;
    public int world_item_id;
    bool trigger_entered = false;
    int _player_id;
    WorldItemManager _world_item_manager;
    private Tooltip _tooltip;

    /*
     * Retrieve the player id
     */
    void Start ()
    {
		gameObject.layer = LayerMask.NameToLayer ("HiddenThings");
		gameObject.transform.Translate(0,0,9);
		// set material to stencil masked
		gameObject.GetComponent<SpriteRenderer> ().material = (Material)Resources.Load("Stencil_01_Diffuse Sprite", typeof(Material));
        _player_id = GameData.MyPlayer.PlayerID;
        _world_item_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<WorldItemManager>();
        _tooltip = GameObject.Find("Inventory").GetComponent<Tooltip>();
    }
	
	/* 
     * Update is called once per frame.
     * If the 'F' key is pressed while the player is in the world item object's collider box,
     * an message is sent to the server to signal a pick up event.
     * The message contains the following information:
     * - the world item id
     * - the player id
     * - the item id
     * - the amount
     */
	void Update () {
        if (trigger_entered)
        {
            if (Inventory.instance.CheckIfItemCanBeAdded(item.stackable, item.id))
            {
                // Send Network message
                List<Pair<string, string>> msg = _world_item_manager.CreatePickupItemNetworkMessage(world_item_id, item.id, amount);
                NetworkingManager.send_next_packet(DataType.Item, (int)ItemUpdate.Pickup, msg, Protocol.UDP);

            _tooltip.Deactivate();

            // Prevent that a pickup event was received
            //_world_item_manager.ReceiveItemPickupPacket(_world_item_manager.ConvertListToJSONClass(msg));
                // Pretend that a pickup event was received
                _world_item_manager.ReceiveItemPickupPacket(_world_item_manager.ConvertListToJSONClass(msg));
            }
            else
            {
                StartCoroutine(Inventory.instance.DisplayInventoryFullError());
            }
            
        }
    }

    /* 
     * The trigger_entered flag is enabled when the player comes into contact with the
     * collider box of the world item for the first time.
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" &&
            // other.gameObject.GetComponent<BaseClass>().playerID == _player_id) // For testing, since GameData.MyPlayerID not set b4 Start is called
            other.gameObject.GetComponent<BaseClass>().playerID == GameData.MyPlayer.PlayerID)
        {
            trigger_entered = true;
        }
    }

    /* 
     * The trigger_entered flag is disabled when the player leaves the
     * collider box of the world item.
     */
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" &&
            //other.gameObject.GetComponent<BaseClass>().playerID == _player_id) // For testing, since GameData.MyPlayerID not set b4 Start is called
            other.gameObject.GetComponent<BaseClass>().playerID == GameData.MyPlayer.PlayerID)
        {
            trigger_entered = false;
        }
    }

    public void OnMouseEnter()
    {
        _tooltip.Activate(item, amount);
    }

    public void OnMouseExit()
    {
        _tooltip.Deactivate();
    }
}
