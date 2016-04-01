using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/*-----------------------------------------------------------------------------
-- ItemData.cs - Script attached to an Item object (inventory item) that holds
--               information about the inventory items such as the Item object
--               it holds, the amount and its position in the inventory. Also
--               handles item drag events and pointer events
--
-- FUNCTIONS:
--      void Start()
--		public void OnBeginDrag(PointerEventData eventData)
--		public void OnDrag(PointerEventData eventData)
--      public void OnEndDrag(PointerEventData eventData)
--      public void OnPointerDown(PointerEventData eventData)
--      public void OnPointerEnter(PointerEventData eventData)
--      public void OnPointerExit(PointerEventData eventData)
--      public void OnPointerClick(PointerEventData eventData)
--      
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class ItemData : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Item item;
    public int amount;
    public int item_pos;

    private Vector2 _offset;
    private Inventory _inventory;
    private Tooltip _tooltip;
    private ItemMenu _item_menu;

    /*
     * Retrives the Inventory, Tooltip and ItemMenu scripts
     */
    void Start()
    {
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _tooltip = _inventory.GetComponent<Tooltip>();
        _item_menu = _inventory.GetComponent<ItemMenu>();
    }

    /*
     * Begins dragging the item
     */
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null && item_pos != Constants.WEAPON_SLOT)
        {
            this.transform.SetParent(this.transform.parent.parent);
            this.transform.position = eventData.position - _offset;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    /*
     * Item follows the mouse pointer while being dragged
     */
    public void OnDrag(PointerEventData eventData)
    {
        if (item != null && item_pos != Constants.WEAPON_SLOT)
        {
            this.transform.position = eventData.position - _offset;
        }
    }

    /*
     * The item is dropped and centered in the new slot
     */
    public void OnEndDrag(PointerEventData eventData)
    {   
        if (item_pos != Constants.WEAPON_SLOT)
        {
            this.transform.SetParent(_inventory.slot_list[item_pos].transform);
            this.transform.position = _inventory.slot_list[item_pos].transform.position;
            GetComponent<CanvasGroup>().blocksRaycasts = true;
            _inventory.UpdateWeaponStats();
        }
    }

    /*
     * Calculates the offset
     */
    public void OnPointerDown(PointerEventData eventData)
    {
        if (item != null && item_pos == Constants.WEAPON_SLOT)
        {
            _offset = eventData.position - new Vector2(this.transform.position.x, this.transform.position.y);
        }
    }

    /*
     * Sets the tooltip game object to active
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.Activate(item);
    }

    /*
     * Sets the Tooltip game object to inactive
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.Deactivate();        
    }

    /*
     * Handles click events on the inventory items
     */
    public void OnPointerClick(PointerEventData eventData)
    {
        if (item_pos != Constants.WEAPON_SLOT)
        {
            // Right click: set the ItemMenu to active and its position in the event of a right mouse click
            if (eventData.pointerId == -2)
            {
                _item_menu.Activate(item, amount, item_pos);
            }

            // Left click: call userConsumable() if the item is of the consumable types
            if (eventData.pointerId == -1)
            {
                if (item.type == Constants.CONSUMABLE_TYPE)
                {
                    //Debug.Log("left click and consumable type");
                    _inventory.UseConsumable(item_pos);
                }
            }

        }
    }
}
