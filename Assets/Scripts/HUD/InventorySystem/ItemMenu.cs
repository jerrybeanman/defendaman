using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

/*-----------------------------------------------------------------------------
-- ItemMenu.cs - Script attached to ItemMenu game object resposible for 
--               displaying buttons for using and dropping items
--
-- FUNCTIONS:
--		void Start()
--		public void Activate(Item item, int amt, int inv_pos)
--		public void Deactivate()
--      public void UseItemOnClick()
--      public void CancelOnClick()
--      public void DropItemOnClick()
--
-- DATE:		05/03/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class ItemMenu : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //private int _dropped_item_instance_id;
    private Item _item;
    private int _amt;
    private int _inv_pos;
    private GameObject _item_menu;
    private WorldItemManager _world_item_manager;
    /*
     * Retrieves the ItemMenu game object and sets it to inactive.
     * Retrieves the WorldItemManager script.
     */
    void Start ()
    {
        _item_menu = GameObject.Find("Item Menu"); 
        _item_menu.SetActive(false);
        _world_item_manager = GameObject.Find("GameManager").GetComponent<WorldItemManager>();    
    }

    /*
     * Sets the position of the ItemMenu to the position of the mouse cursor
     * and sets the item menu to active. Stores the item, amount and inventory
     * position of the inventory item.
     */
    public void Activate(Item item, int amt, int inv_pos)
    {
        _item_menu.transform.position = Input.mousePosition;
        _item_menu.SetActive(true);
        _item = item;
        _amt = amt;
        _inv_pos = inv_pos;
    }

    /*
     * Sets the ItemMenu to inactive
     */
    public void Deactivate()
    {
        _item_menu.SetActive(false);
    }

    /*
     * Detects that the use button was clicked and sets the MenuItem to inactive.
     * Does nothign for now
     */
    public void UseItemOnClick()
    {
        Debug.Log("use item: " + _item.id);
        Deactivate();
    }

    /* 
     * Detects that the drop button was clicked. 
     * A message is sent to the server to signal an item drop event and the 
     * ItemMenu is set to inactive.
     * The message contains the following information:
     * - the world item id
     * - the player id
     * - the item id
     * - the amount
     * - the inventory position
     * - the position of the player in the world
     */
    public void DropItemOnClick()
    {
        Vector3 position = GameObject.Find("GameManager").GetComponent<NetworkingManager>().player.transform.position;
        int _player_id = GameData.MyPlayerID;
        int _world_item_id = _world_item_manager.GenerateWorldItemId();
        
        /*
        foreach (var id in GameData.LobbyData.Keys)
        {
            var classtype = GameData.LobbyData[id].ClassType;
        }*/

        // Data to send to server indicating that the player wants to drop an item from his inventory
        // _player_id
        // _world_item_id
        // _item.id
        // _amt
        // _inv_pos
        // position

        // Temporary call for testing
        _world_item_manager.ProcessDropEvent(_world_item_id, _player_id, _item.id, 
            _amt, _inv_pos, (int)position.x, (int)position.y);
        Deactivate();
    }

    /*
     * Detects that the cancel button was clicked and sets the MenuItem to inactive.
     * Does nothign for now
     */
    public void CancelOnClick()
    {
        Debug.Log("cancel: " + _item.id);
        Deactivate();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameData.MouseBlocked = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameData.MouseBlocked = false;
    }
}

