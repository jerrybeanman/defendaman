using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
public class DamagePotionTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Inventory _inventory;
    private Tooltip _tooltip;
    private ItemManager _item_manager;
    // Use this for initialization
    void Start()
    {
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        _tooltip = _inventory.GetComponent<Tooltip>();
        _item_manager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ItemManager>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        print("Enter Test");
        Item i1 = _item_manager.FetchItemById(5);
        _tooltip.Activate(i1, 1, true);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltip.Deactivate();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
