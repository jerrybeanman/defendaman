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
-- DATE:		17/02/2016
-- REVISIONS:	26/02/2016 - Add handling for click events on inventory items
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

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		17/02/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Start()
    -- RETURNS: 	void
    -- NOTES:
    -- Retrieves the Inventory script, the Tooltip script and the ItemMenu script.
    ----------------------------------------------------------------------------------------*/
    void Start()
    {
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _tooltip = _inventory.GetComponent<Tooltip>();
        _item_menu = _inventory.GetComponent<ItemMenu>();
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnBeginDrag
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnBeginDrag(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Sets the position of the Item to be the same as the position of the mouse cursor with an offset
    -- when the drag event is initiated.
    -------------------------------------------------------------------------------------------------*/
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            this.transform.SetParent(this.transform.parent.parent);
            this.transform.position = eventData.position - _offset;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnDrag
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnDrag(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Item follows the mouse pointer while being dragged.
    -------------------------------------------------------------------------------------------------*/
    public void OnDrag(PointerEventData eventData)
    {
        if (item != null) // && item_pos != Constants.WEAPON_SLOT) // Uncomment to make weapon slot static
        {
            this.transform.position = eventData.position - _offset;
        }
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnEndDrag
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnEndDrag(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Sets the parent of the Item to be the inventory slot at the end of the drag event.
    -------------------------------------------------------------------------------------------------*/
    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(_inventory.slot_list[item_pos].transform);
        this.transform.position = _inventory.slot_list[item_pos].transform.position;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        _inventory.UpdateWeaponStats();
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnPointerDown
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnPointerDown(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Calculates the offset when clicking on item.
    -------------------------------------------------------------------------------------------------*/
    public void OnPointerDown(PointerEventData eventData)
    {
        if (item != null) // && item_pos == Constants.WEAPON_SLOT) // Uncomment to make weapon slot static
        {
            _offset = eventData.position - new Vector2(this.transform.position.x, this.transform.position.y);
        }
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnPointerEnter
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnPointerEnter(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Activates the tooltip when the mouse pointer enters the collider box of the item.
    -------------------------------------------------------------------------------------------------*/
    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.Activate(item);
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnPointerExit
    -- DATE: 		17/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnPointerExit(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Deactivates the tooltip when the mouse pointer leaves the collider box of the item.
    -------------------------------------------------------------------------------------------------*/
    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.Deactivate();        
    }

    /*-------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnPointerClick
    -- DATE: 		26/02/2016
    -- REVISIONS: 	
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void OnPointerClick(PointerEventData eventData)
    --                  PointerEventData eventData: The event payload associated with pointer events.
    -- RETURNS: 	void
    -- NOTES:
    -- Handles click event on inventory items. Right click activates the item menu, left click uses
    -- the item if it is a consumable.
    -------------------------------------------------------------------------------------------------*/
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -2)
        {
            _item_menu.Activate(item, amount, item_pos);
        }

        // Left click: call useConsumable() if the item is of the consumable types
        if (eventData.pointerId == -1)
        {
            if (item.type == Constants.CONSUMABLE_TYPE)
            {
                _inventory.UseConsumable(item_pos);
            }
        }
    }
}
