using UnityEngine;
using System.Collections;
using SimpleJSON; //namespace for the JSON parser plugin
using System.Collections.Generic;
using System.IO;

/*-----------------------------------------------------------------------------
-- ItemManager.cs - Script attached to the Inventory and the GameManager 
--                  game objects. Responsible for creating Item objects 
--                  from a JSON file and storing them in a list.
--
-- FUNCTIONS:
--		void Start()
--		void construct_item_list()
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class ItemManager : MonoBehaviour {
    private List<Item> _item_database = new List<Item>();
    private JSONNode _item_data;

    /*
     * Reads data from a JSON file and creates the item list.
     */
    void Start()
    {
        _item_data = JSON.Parse(File.ReadAllText(Application.dataPath + 
            "/StreamingAssets/Items.json"));
        construct_item_list();
    }

    /* 
     * Searches the list of Items for an item using it item id.
     * Returns the Item if found, NULL otherwise.
     */
    public Item FetchItemById(int id)
    {
        for (int i = 0; i < _item_database.Count; i++)
        {
            if (_item_database[i].id == id)
                return _item_database[i];
        }
        return null;
    }

    /* 
     * Populates the item_database using data retrieved from a JSON object
     */
    void construct_item_list()
    {
        for (int i = 0; i < _item_data.Count; i++)
        {
            _item_database.Add(new Item(_item_data[i]["id"].AsInt, 
                                        _item_data[i]["title"], 
                                        _item_data[i]["value"].AsInt,
                                        bool.Parse(_item_data[i]["stackable"]),
                                        _item_data[i]["slug"]));
        }
    }

}

/*-----------------------------------------------------------------------------
-- Item: class
--
-- FUNCTIONS:
--		void Start()
--		public Item(int id, string title, int value, bool stackable, string slug)
--      public Item()
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
public class Item
{
    public int id { get; set; }
    public string title { get; set; }
    public int value { get; set; }
    public bool stackable { get; set; }
    public string slug { get; set; }
    public Sprite sprite { get; set; }

    /* 
     * Constructor
     */
    public Item(int id, string title, int value, bool stackable, string slug)
    {
        this.id = id;
        this.title = title;
        this.value = value;
        this.stackable = stackable;
        this.slug = slug;
        this.sprite = Resources.Load<Sprite>("Sprites/Items/" + slug);
    }

    /* 
     * Constructor: Default item objects that are placed into empty slots
     */
    public Item()
    {
        this.id = -1;
    }
}
