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
        /*
        foreach (Item item in _item_database)
        {
            Debug.Log("item id: " + item.id);
            Debug.Log("item title: " + item.title);
            Debug.Log("item desc: " + item.description);
            Debug.Log("item dmg: " + item.damage);
            Debug.Log("item armor: " + item.armor);
            Debug.Log("item health: " + item.health);
            Debug.Log("item speed: " + item.speed);
            Debug.Log("item duration: " + item.duration);
            Debug.Log("item classType: " + item.classType);
            Debug.Log("item stackable: " + item.stackable);
            Debug.Log("item type: " + item.type);
        }*/
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
                                        _item_data[i]["description"],
                                        _item_data[i]["stats"]["damage"].AsInt,
                                        _item_data[i]["stats"]["armor"].AsInt,
                                        _item_data[i]["stats"]["health"].AsInt,
                                        _item_data[i]["stats"]["speed"].AsInt,
                                        _item_data[i]["duration"].AsInt,
                                        _item_data[i]["classType"].AsInt,
                                        bool.Parse(_item_data[i]["stackable"]),
                                        _item_data[i]["type"],
                                        _item_data[i]["cost"].AsInt,
                                        _item_data[i]["slug"],
                                        _item_data[i]["worldSlug"]));
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
    public string description { get; set; }
    public int damage { get; set; }
    public int armor { get; set; }
    public int health { get; set; }
    public int speed { get; set; }
    public int duration { get; set; }
    public int classType { get; set; }
    public bool stackable { get; set; }
    public string type { get; set; }
    public int cost { get; set; }
    public Sprite sprite { get; set; }
    public Sprite world_sprite { get; set; }

    /* 
     * Constructor
     */
    public Item(int id, string title, string description, int damage, int armor, 
        int health, int speed, int duration, int classType, bool stackable, 
        string type, int cost, string slug, string world_slug)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.damage = damage;
        this.armor = armor;
        this.health = health;
        this.speed = speed;
        this.duration = duration;
        this.classType = classType;
        this.stackable = stackable;
        this.type = type;
        this.cost = cost;
        this.sprite = Resources.Load<Sprite>("Sprites/Items/" + slug);
        this.world_sprite = Resources.Load<Sprite>("Sprites/Items/" + world_slug);
    }

    /* 
     * Constructor: Default item objects that are placed into empty slots
     */
    public Item()
    {
        this.id = -1;
    }
}
