using UnityEngine;
using System.Collections;

public class WorldItemManager : MonoBehaviour
{
    ItemManager _item_manager;
    public GameObject world_item;

	// Use this for initialization
	void Start ()
    {
        _item_manager = GetComponent<ItemManager>();
        CreateWorldItem(0, 10, new Vector3(55, 55, 0));
    }

    public void CreateWorldItem(int id, int amt, Vector3 pos)
    {
        Item item = _item_manager.FetchItemById(id);
        GameObject _item = Instantiate(world_item);
        _item.GetComponent<WorldItemData>().item = item;
        _item.GetComponent<WorldItemData>().amount = amt;
        _item.transform.position = pos;
        
    }
}
