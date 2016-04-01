using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

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
public class ItemMenu : MonoBehaviour
{
    private Item _item;
    private int _amt;
    private int _inv_pos;
    private GameObject _item_menu;
    private AmountPanel _amount_panel;
    private WorldItemManager _world_item_manager;
    private GameObject _use_btn;
    RectTransform _item_menu_rt;
    private float _item_menu_height;

    /*
     * Retrieves the ItemMenu game object and sets it to inactive.
     * Retrieves the WorldItemManager script.
     */
    void Start ()
    {
        _item_menu = GameObject.Find("Item Menu"); 
        _item_menu.SetActive(false);
        _world_item_manager = GameObject.Find("GameManager").GetComponent<WorldItemManager>();
        _amount_panel = GameObject.Find("Inventory").GetComponent<AmountPanel>();
        _use_btn = _item_menu.transform.FindChild("Use Button").gameObject;
        _item_menu_rt = _item_menu.GetComponent<RectTransform>();
        _item_menu_height = _item_menu_rt.rect.height;
    }

    /*
     * Deactivate item menu when there is mouse click outside the item menu
     */
    void Update()
    {
        if (!GameData.MouseBlocked && (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0))) 
        {
            Deactivate();
        }
    }

    /*
     * Sets the position of the ItemMenu to the position of the mouse cursor
     * and sets the item menu to active. Stores the item, amount and inventory
     * position of the inventory item.
     */
    public void Activate(Item item, int amt, int inv_pos)
    {
        GameObject _use_btn = _item_menu.transform.FindChild("Use Button").gameObject;
        _item_menu.transform.position = Input.mousePosition;
        _item_menu.SetActive(true);
        _item = item;
        _amt = amt;
        _inv_pos = inv_pos;
        if (item.type == Constants.CONSUMABLE_TYPE)
        {
            _use_btn.SetActive(true);
            _item_menu_rt.sizeDelta = new Vector2(_item_menu_rt.rect.width, _item_menu_height);
        }
        else
        {
            _use_btn.SetActive(false);
            _item_menu_rt.sizeDelta =  new Vector2(_item_menu_rt.rect.width, _item_menu_height * 2 / 3);
        }
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
        GameData.MouseBlocked = false;
        Inventory.instance.UseConsumable(_inv_pos);
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
        if (_amt > 1 && _item.stackable)
        {
            _amount_panel.Activate(_item, _amt, _inv_pos);
        }
        else
        {
            // Send Network message
            List<Pair<string, string>> msg = 
                _world_item_manager.CreateDropItemNetworkMessage(_item.id, _amt, _inv_pos);
            NetworkingManager.send_next_packet(DataType.Item, (int)ItemUpdate.Drop, msg, Protocol.UDP);

            // Pretend that a drop message was received
            _world_item_manager.ReceiveItemDropPacket(_world_item_manager.ConvertListToJSONClass(msg));
        }
        GameData.MouseBlocked = false;
        Deactivate();
    }

    /*
     * Detects that the cancel button was clicked and sets the MenuItem to inactive.
     * Does nothign for now
     */
    public void CancelOnClick()
    {
        Debug.Log("cancel: " + _item.id);
        GameData.MouseBlocked = false;
        Inventory.instance.UseResources(Constants.GOLD_RES, 1);
        Deactivate();
    }
}

