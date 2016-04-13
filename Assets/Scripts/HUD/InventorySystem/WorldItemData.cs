using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;

/*------------------------------------------------------------------------------------
-- WorldItemData.cs - Script attached to WorldItem game objects that store 
--                    information about world items such as the Item object it 
--                    holds, the amount and its unique world item id. Also
--                    Handles the events that get triggered when the player 
--                    come into contact with AmountPanelit.
--
-- FUNCTIONS:
--      void Start()
--      void Update ()
--      void OnTriggerEnter2D(Collider2D other)
--      void OnTriggerExit2D(Collider2D other)
--
-- DATE:        05/03/2016
-- REVISIONS:   03/04/2016 - Conditions for pickup (gold vs non-gold) - Krystle
--              27/03/2016 - Add guard to prevent more than more unique item from 
--                           being picked up at once - Joseph
--              15/03/2016 - Add tooltip to world items - Joseph
-- DESIGNER:    Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang, Krystle Bulalakaw
-----------------------------------------------------------------------------------*/

public class WorldItemData : MonoBehaviour
{
	public Item item;
	public int amount;
	public int world_item_id;
    public bool autolootable = false;
    bool _trigger_entered = false;
	int _player_id;
	bool _first_collision = false;
	WorldItemManager _world_item_manager;
	private Tooltip _tooltip;

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		05/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Start()
    -- RETURNS: 	void
    -- NOTES:
    -- Retrieves the HiddenThings layer mask.
    -- Retrieves the WorldItemManager script.
    -- Retrieves the Tooltip GameObject.
    ----------------------------------------------------------------------------------------*/
    void Start()
	{
		gameObject.layer = LayerMask.NameToLayer("HiddenThings");
		gameObject.transform.Translate(0, 0, 9);
		// set material to stencil masked
		gameObject.GetComponent<SpriteRenderer>().material = 
            (Material)Resources.Load("Stencil_01_Diffuse Sprite", typeof(Material));
		_player_id = GameData.MyPlayer.PlayerID;
		_world_item_manager = 
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<WorldItemManager>();
		_tooltip = GameObject.Find("Inventory").GetComponent<Tooltip>();
	}

    /*--------------------------------------------------------------------------------------
    -- FUNCTION: 	Update
    -- DATE: 		05/03/2016
    -- REVISIONS: 	27/03/2016 - Add gurad to prevent mulitple unique items from being 
    --                           picked up at once.
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Update()
    -- RETURNS: 	void
    -- NOTES:
    -- If the 'F' key is pressed while the player is in the world item object's collider box,
    -- an message is sent to the server to signal a pick up event.
    -- Cannot pick up more than one item even when their collider boxes overlap.
    -----------------------------------------------------------------------------------------*/
    void Update()
    {
        // Manually pick up items that are not autolootable
        if (_trigger_entered && !autolootable && Input.GetKeyDown(KeyCode.F)) {
            if (Inventory.instance.CheckIfItemCanBeAdded(item.stackable, item.id)) {
                int player_id = GameData.MyPlayer.PlayerID;
                List<Pair<string, string>> msg = new List<Pair<string, string>>();
                    				
                if (!GameData.ItemCollided) {
	                GameData.ItemCollided = true;
	                _first_collision = true;
                }

                if (_first_collision) {
	                _first_collision = false;
                    if (_world_item_manager == null)
                    {
                        Debug.Log("[ERROR] WorldItemData's Update says _world_item_manager is null");
                        return;
                    }
	                msg = _world_item_manager.CreatePickupItemNetworkMessage(world_item_id, player_id, item.id, amount);
	                NetworkingManager.send_next_packet(DataType.Item, (int)ItemUpdate.Pickup, msg, Protocol.TCP);
	                StartCoroutine(NeverReceivedPickupMessageBack());

                    // Pretend that a pickup event was received
                    if (Application.platform != RuntimePlatform.LinuxPlayer) {
                        StartCoroutine(WorldItemManager.Instance.WaitSmallDelayBeforeReceivePickupPacket(WorldItemManager.Instance.ConvertListToJSONClass(msg)));
                    }
                }
                _tooltip.Deactivate();
            } else {
                StartCoroutine(Inventory.instance.DisplayInventoryFullError());
            }
        }
    }

    /*--------------------------------------------------------------------------------------
    -- FUNCTION: 	OnTriggerEnter2D
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void OnTriggerEnter2D(Collider2D other)
    --                  Collider2D other: The collider box of the other GameObject
    -- RETURNS: 	void
    -- NOTES:
    -- The trigger_entered flag is enabled when the player comes into contact with the
    -- collider box of the world item for the first time.
    -----------------------------------------------------------------------------------------*/
    void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player" && 
            other.gameObject.GetComponent<BaseClass>().playerID == GameData.MyPlayer.PlayerID)
        {
            _trigger_entered = true;
			
            // Send item pickup message once if gold 
            if (Inventory.instance.CheckIfItemCanBeAdded(item.stackable, item.id))
            {
                if (autolootable)
                {
                    int player_id = GameData.MyPlayer.PlayerID;
                    List<Pair<string, string>> msg = new List<Pair<string, string>>();
                    msg = _world_item_manager.CreatePickupItemNetworkMessage(world_item_id, player_id, item.id, amount);
                    NetworkingManager.send_next_packet(DataType.Item, (int)ItemUpdate.Pickup, msg, Protocol.TCP);

                    _tooltip.Deactivate();

                    // Pretend that a pickup event was received
                    if (Application.platform != RuntimePlatform.LinuxPlayer)
                    {
                        StartCoroutine(WorldItemManager.Instance.WaitSmallDelayBeforeReceivePickupPacket(WorldItemManager.Instance.ConvertListToJSONClass(msg)));
                    }
                }
            }
            else
            {
                StartCoroutine(Inventory.instance.DisplayInventoryFullError());
            }
        }
    }

    /*--------------------------------------------------------------------------------------
    -- FUNCTION: 	OnTriggerExit2D
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void OnTriggerExit2D(Collider2D other)
    --                  Collider2D other: The collider box of the other GameObject
    -- RETURNS: 	void
    -- NOTES:
    -- The trigger_entered flag is disabled when the player leaves the
    -- collider box of the world item.
    -----------------------------------------------------------------------------------------*/
    void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.tag == "Player" &&
		    other.gameObject.GetComponent<BaseClass>().playerID == GameData.MyPlayer.PlayerID)
		{
			_trigger_entered = false;
		}
	}

    /*--------------------------------------------------------------------------------------
    -- FUNCTION: 	OnMouseEnter
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnMouseEnter()
    -- RETURNS: 	void
    -- NOTES:
    -- Enable the tooltip when the mouse pointer hovers over the world item.
    -----------------------------------------------------------------------------------------*/
    public void OnMouseEnter()
	{
		_tooltip.Activate(item, amount);
	}

    /*--------------------------------------------------------------------------------------
    -- FUNCTION: 	OnMouseExit
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnMouseEnter()
    -- RETURNS: 	void
    -- NOTES:
    -- Disable the tooltip when the mouse pointer exits the world item collider box.
    -----------------------------------------------------------------------------------------*/
    public void OnMouseExit()
	{
		_tooltip.Deactivate();
	}

    /*--------------------------------------------------------------------------------------
    -- FUNCTION: 	NeverReceivedPickupMessageBack
    -- DATE: 		15/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public IEnumerator NeverReceivedPickupMessageBack()
    -- RETURNS: 	IEnumerator
    -- NOTES:
    -- Disable the item collision flags if no echo message is received back within 1/10 of a
    -- second after sending the pickup network message.
    -----------------------------------------------------------------------------------------*/
    public IEnumerator NeverReceivedPickupMessageBack()
	{
		yield return new WaitForSeconds(0.1f);
		GameData.ItemCollided = false;
		_first_collision = false;
	}
}
