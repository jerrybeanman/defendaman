using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

/*-----------------------------------------------------------------------------
-- ItemData.cs - C# class to represent and hold a item
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

    void Start()
    {
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _tooltip = _inventory.GetComponent<Tooltip>();
        _item_menu = _inventory.GetComponent<ItemMenu>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            this.transform.SetParent(this.transform.parent.parent);
            this.transform.position = eventData.position - _offset;
            GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (item != null)
        {
            this.transform.position = eventData.position - _offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(_inventory.slot_list[item_pos].transform);
        this.transform.position = _inventory.slot_list[item_pos].transform.position;
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (item != null)
        {
            _offset = eventData.position - new Vector2(this.transform.position.x, this.transform.position.y);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _tooltip.Activate(item);
    } 

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.Deactivate();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.pointerId == -2)
        {
            Debug.Log("right click");
            _item_menu.Activate(item);
        }
    }
}
