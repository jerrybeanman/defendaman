using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/*-----------------------------------------------------------------------------
-- InventorySlot.cs - Visual item slot for the inventory system
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class InventorySlot : MonoBehaviour, IDropHandler
{
    public int slot_pos;
    private Inventory _inventory;

    void Start()
    {
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
    }

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
