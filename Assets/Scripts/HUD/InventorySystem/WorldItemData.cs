using UnityEngine;
using System.Collections;

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
    public string world_item_id;
    bool trigger_entered = false;
    int _player_id;
 
    /*
     * Retrieve the player id
     */
    void Start ()
    {
        _player_id = GameData.MyPlayerID;
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
        if (Input.GetKeyDown(KeyCode.F) && trigger_entered)
        {
            // data to send to server indicating that the player wants to pick up an item
            // world_item_id
            // _player_id    
            // item.id
            // amount

            // Temporary call for testing
            WorldItemManager _world_item_manager = GameObject.Find("GameManager").GetComponent<WorldItemManager>();
            _world_item_manager.ProcessPickUpEvent(world_item_id, _player_id, item.id, amount);
        }
    }

    /* 
     * The trigger_entered flag is enabled when the player comes into contact with the
     * collider box of the world item for the first time.
     */
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player" && 
            other.gameObject.GetComponent<BaseClass>().playerID == _player_id)
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
            other.gameObject.GetComponent<BaseClass>().playerID == _player_id)
        {
            trigger_entered = false;
        }
    }
}
