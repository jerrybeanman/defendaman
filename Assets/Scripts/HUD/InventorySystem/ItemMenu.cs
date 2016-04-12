using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

/*--------------------------------------------------------------------------------------
-- ItemMenu.cs - Script attached to ItemMenu game object resposible for 
--               displaying buttons for using and dropping items
--
-- FUNCTIONS:
--		void Start()
--      void Update()
--		public void Activate(Item item, int amt, int inv_pos)
--		public void Deactivate()
--      public void UseItemOnClick()
--      public void DropItemOnClick()
--      public void CancelOnClick()
--
-- DATE:		05/03/2016
-- REVISIONS: 	25/03/2016 - Add Update() that deactivates the item menu when clicking
--                           anywhere but the item menu.
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
--------------------------------------------------------------------------------------*/
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

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		05/03/2016
    -- REVISIONS:
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Start()
    -- RETURNS: 	void
    -- NOTES:
    -- Retrieves the ItemMenu game object and sets it to inactive.
    -- Retrieves the WorldItemManager script.
    ----------------------------------------------------------------------------------------*/
    void Start ()
    {
        _item_menu = GameObject.Find("Item Menu"); 
        _item_menu.SetActive(false);
		_world_item_manager = 
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<WorldItemManager>();
        _amount_panel = GameObject.Find("Inventory").GetComponent<AmountPanel>();
        _use_btn = _item_menu.transform.FindChild("Use Button").gameObject;
        _item_menu_rt = _item_menu.GetComponent<RectTransform>();
        _item_menu_height = _item_menu_rt.rect.height;
    }

    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	Update
    -- DATE: 		25/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Update()
    -- RETURNS: 	void
    -- NOTES:
    -- Deactivate item menu when there is mouse click outside the item menu
    ----------------------------------------------------------------------------------*/
    void Update()
    {
        if (!GameData.MouseBlocked && (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse0))) 
        {
            Deactivate();
        }
    }
    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	Activate
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void Activate(Item item, int amt, int inv_pos)
    --                  Item item: The item in the inventory slot
    --                  int amt: The amount of the item
    --                  int inv_pos: The inventory position
    -- RETURNS: 	void
    -- NOTES:
    -- Sets the position of the ItemMenu to the position of the mouse cursor
    -- and sets the item menu to active. Stores the item, amount and inventory
    -- position of the inventory item.
    ----------------------------------------------------------------------------------*/
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

    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	Deactivate
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void Deactivate()
    -- RETURNS: 	void
    -- NOTES:
    -- Sets the ItemMenu to inactive
    ----------------------------------------------------------------------------------*/
    public void Deactivate()
    {
        _item_menu.SetActive(false);
    }

    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	UseItemOnClick
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void UseItemOnClick()
    -- RETURNS: 	void
    -- NOTES:
    -- Uses the item and sets the MenuItem to inactive.
    ----------------------------------------------------------------------------------*/
    public void UseItemOnClick()
    {
        Debug.Log("use item: " + _item.id);
        GameData.MouseBlocked = false;
        Inventory.instance.UseConsumable(_inv_pos);
        Deactivate();
    }

    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	DropItemOnClick
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void DropItemOnClick()
    -- RETURNS: 	void
    -- NOTES:
    -- Sends an ItemDrop network message if there is only one item or activates the 
    -- AmountPanel if there are more that 1 item stack.
    -- Deactivates the ItemMenu
    ----------------------------------------------------------------------------------*/
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
            NetworkingManager.send_next_packet(DataType.Item, (int)ItemUpdate.Drop, msg, Protocol.TCP);

            // Pretend that a drop message was received for local testing
			if (Application.platform != RuntimePlatform.LinuxPlayer) {
				_world_item_manager.ReceiveItemDropPacket(_world_item_manager.ConvertListToJSONClass(msg));
			}
        }
        GameData.MouseBlocked = false;
        Deactivate();
    }

    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	CancelOnClick
    -- DATE: 		05/03/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void CancelOnClick()
    -- RETURNS: 	void
    -- NOTES:
    -- Deactivates the ItemMenu
    ----------------------------------------------------------------------------------*/
    public void CancelOnClick()
    {
        GameData.MouseBlocked = false;
        Deactivate();
    }
}

