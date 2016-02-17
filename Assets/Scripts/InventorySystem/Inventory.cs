using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

static class Constants
{
    public const int SLOT_AMOUNT = 4;
}

/*-----------------------------------------------------------------------------
-- Inventory.cs - Responsible for storing the weapons/items of the players
--
-- FUNCTIONS:
--		void Start()
--		public void AddItem(int id)
--		int check_if_item_in_inventory(Item item)
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class Inventory : MonoBehaviour
{
    GameObject _inventory_panel;
    GameObject _slot_panel;
    ItemManager _item_manager;
    public GameObject inventory_slot;
    public GameObject inventory_item;

    //int _slot_amount;
    public List<Item> inventory_item_list = new List<Item>();
    public List<GameObject> slot_list = new List<GameObject>();

    void Start()
    {
        _item_manager = GetComponent<ItemManager>();
        _inventory_panel = GameObject.Find("Inventory Panel");
        _slot_panel = _inventory_panel.transform.FindChild("Slot Panel").gameObject;
        for (int i = 0; i < Constants.SLOT_AMOUNT; i++)
        {
            inventory_item_list.Add(new Item());
            //Debug.Log(i);
            slot_list.Add(Instantiate(inventory_slot));
            slot_list[i].GetComponent<InventorySlot>().slot_pos = i;
            slot_list[i].transform.SetParent(_slot_panel.transform);
        }
        //AddItem(0);
        //AddItem(1);
        //AddItem(1);
    }


    /* 
     * Assuming to be adding one item at the time.
     * Takes an item id and adds an Item object into an open slot in the inventory or stacks 
     * it on a slot containing the same item.
     * Updates the attributes of the Item: item, amount, and item_pos  
     */
    public void AddItem(int id)
    {
        Item _item_to_add = _item_manager.FetchItemById(id);
        int item_idx;
        if (_item_to_add.stackable && (item_idx = check_if_item_in_inventory(_item_to_add)) != -1)
        {
            ItemData data = slot_list[item_idx].transform.GetChild(0).GetComponent<ItemData>();
            if (data.amount == 0)
                data.amount++;
            data.amount++;
            data.transform.GetChild(0).GetComponent<Text>().text = data.amount.ToString();
        }
        else
        {
            for (int i = 0; i < inventory_item_list.Count; i++)
            {
                if (inventory_item_list[i].id == -1)
                {
                    inventory_item_list[i] = _item_to_add;
                    GameObject _item_obj = Instantiate(inventory_item);
                    _item_obj.GetComponent<ItemData>().item = _item_to_add;
                    _item_obj.GetComponent<ItemData>().item_pos = i;
                    _item_obj.GetComponent<ItemData>().amount++;
                    _item_obj.transform.SetParent(slot_list[i].transform);
                    _item_obj.transform.position = Vector2.zero; //centers item relative to parent
                    _item_obj.GetComponent<Image>().sprite = _item_to_add.sprite;
                    _item_obj.name = _item_to_add.title; //name shown in the inspector
                    break;
                }
            }
        }
    }

    /* 
     * Checks whether the item being added is already present in the inventory.
     * Used to see if an item should be stacked or placed on a new slot when being added
     */
    int check_if_item_in_inventory(Item item)
    {
        for (int i = 0; i < inventory_item_list.Count; i++)
        {
            if (inventory_item_list[i].id == item.id)
            {
                return i;
            }
        }
        return -1;
    }
}
