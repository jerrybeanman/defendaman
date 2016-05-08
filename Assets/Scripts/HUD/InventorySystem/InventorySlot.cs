using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/*-----------------------------------------------------------------------------
-- InventorySlot.cs - Script attached to a Slot game object that store its 
--                    position in the inventory. Also handles the event that
--                    is triggered when an inventory item is dragged onto it
--
-- FUNCTIONS:
--      void Start()
--		public void OnDrop(PointerEventData eventData)
--      
-- DATE:		17/02/2016
-- REVISIONS:	
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public int slot_pos;
    private Inventory _inventory;

    /*-----------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Start()
    -- RETURNS: 	void
    -- NOTES:
    -- Rettrives the Inventory script.
    ----------------------------------------------------------------------------------*/
    void Start()
    {
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnDrop
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnDrop(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Trigger when an inventory item is dragged and dropped to another or the
    -- the same inventory slot.
    -- If dropped to an inventory slot with an item in it, the items swap
    -- slot positions. Otherwise, the item is moved to the new position
    -------------------------------------------------------------------------------------------------*/
    public void OnDrop(PointerEventData eventData)
    {
        ItemData _dropped_item = eventData.pointerDrag.GetComponent<ItemData>();

        if (_inventory.inventory_item_list[slot_pos].id == -1)
        {
            _inventory.inventory_item_list[_dropped_item.item_pos] = new Item();
            _inventory.inventory_item_list[slot_pos] = _dropped_item.item;
            _dropped_item.item_pos = slot_pos;
        }
        else if (_dropped_item.item_pos != slot_pos)
        {
            Transform item = this.transform.GetChild(0);

            _inventory.inventory_item_list[_dropped_item.item_pos] = item.GetComponent<ItemData>().item;
            _inventory.inventory_item_list[slot_pos] = _dropped_item.item;

            item.GetComponent<ItemData>().item_pos = _dropped_item.item_pos;
            item.transform.SetParent(_inventory.slot_list[_dropped_item.item_pos].transform);
            item.transform.position = _inventory.slot_list[_dropped_item.item_pos].transform.position;
            _dropped_item.item_pos = slot_pos;
            _dropped_item.transform.SetParent(this.transform);
            _dropped_item.transform.position = this.transform.position;
            _inventory.inventory_item_list[_dropped_item.item_pos] = item.GetComponent<ItemData>().item;
            _inventory.inventory_item_list[slot_pos] = _dropped_item.item;
        }
    }
}
