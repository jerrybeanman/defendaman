using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*-----------------------------------------------------------------------------
-- Tooltip.cs - Script attached to the Tooltip game object resposible for
--              displaying a tool tip when the mouse pointer hovers over
--              an item in the inventory.
--
-- FUNCTIONS:
--		void Start()
--		void Update()
--      public void Activate(Item item)
--      public void Deactivate()
--      public void ConstructDataString()
--
-- DATE:		25/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class Tooltip : MonoBehaviour
{ 
    private Item _item;
    private int _amount;
    private bool _cost;
    private string _data;
    private GameObject _tooltip;

    /*
     * Retrives the Tooltip game object and sets it to inactive
     */
    void Start()
    {
        _tooltip = GameObject.Find("Tooltip");
        _tooltip.SetActive(false);
    }

    /*
     * Makes the tooltip follow the mouse pointer while its active
     */
    void Update()
    {
        if (_tooltip.activeSelf)
        {
            _tooltip.transform.position = Input.mousePosition;
        }
    }

    /*
     * Creates a string to display on the tooltip specific to the item passed 
     * and sets the Tooltip to active
     */
    public void Activate(Item item, int amount = -1, bool cost = false)
    {
        this._item = item;
        this._amount = amount;
        this._cost = cost;
        ConstructDataString();
        _tooltip.SetActive(true);
    }

    /*
     * Sets the Tooltip to inactive
     */
    public void Deactivate()
    {
        _tooltip.SetActive(false);
    }

    /*
     * Formats the string to be displayed on the Tooltip based on the item 
     * passed.
     */
    public void ConstructDataString()
    {
        _data = "<color=#ffffff>" + _item.title + "</color>\n" + _item.description;
        if (_item.type == Constants.WEAPON_TYPE)
        {
            _data += "\n<color=#ffffff>Damage: </color>" + _item.damage +
                "\n<color=#ffffff>Armor: </color>" + _item.armor;
        }
        
        if (_amount >= 0 && !_cost)
        {
            _data += "\n<color=#ffffff>Amount: </color>" + _amount;
        }

        if (_cost)
        {
            _data += "\n<color=#ffffff>Cost: </color> " + _item.cost;
        }

        _tooltip.transform.GetChild(0).GetComponent<Text>().text = _data;
    }
}
