using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/*-----------------------------------------------------------------------------
-- Tooltip.cs - Script to make a tooltip when you mouse over an item
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class Tooltip : MonoBehaviour
{ 
    private Item _item;
    private string _data;
    private GameObject _tooltip;

    void Start()
    {
        _tooltip = GameObject.Find("Tooltip");
        _tooltip.SetActive(false);
    }

    void Update()
    {
        if (_tooltip.activeSelf)
        {
            _tooltip.transform.position = Input.mousePosition;
        }
    }
     
    public void Activate(Item item)
    {
        this._item = item;
        ConstructDataString();
        _tooltip.SetActive(true);
    }

    public void Deactivate()
    {
        _tooltip.SetActive(false);
    }

    public void ConstructDataString()
    {
        _data = "<color=#ffffff>" + _item.title + "</color>";
        _tooltip.transform.GetChild(0).GetComponent<Text>().text = _data;
    }
}
