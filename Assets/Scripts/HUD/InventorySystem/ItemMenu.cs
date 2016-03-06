using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ItemMenu : MonoBehaviour
{
    private Item _item;
    private GameObject _item_menu;
    private WorldItemManager _world_item_manager;

	void Start ()
    {
        _item_menu = GameObject.Find("Item Menu");
        _world_item_manager = GameObject.Find("GameManager").GetComponent<WorldItemManager>();
        _item_menu.SetActive(false);	
	}

    public void Activate(Item item)
    {
        _item_menu.SetActive(true);
        _item_menu.transform.position = Input.mousePosition;
        _item = item;
    }

    public void Deactivate()
    {
        _item_menu.SetActive(false);
    }

    public void UseItemOnClick()
    {
        Debug.Log("use item: " + _item.id);
        Deactivate();
    }

    public void CancelOnClick()
    {
        Deactivate();
        Debug.Log("cancel: " + _item.id);
    }

    public void DropItemOnClick()
    {
        Debug.Log("drop item: " + _item.id);
        Vector3 position = GameObject.Find("GameManager").GetComponent<NetworkingManager>().player.transform.position;
        _world_item_manager.CreateWorldItem(_item.id, 11, position);
        Deactivate();
    }
}
