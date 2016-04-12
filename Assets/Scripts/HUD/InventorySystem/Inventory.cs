using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/*----------------------------------------------------------------------------------------
-- Constants: class
-- 
-- DATE:		17/02/2016
-- REVISIONS:	
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
---------------------------------------------------------------------------------------*/
static class Constants
{
    public const int SLOT_AMOUNT            = 4;
    public const string WORLD_ITEM_TAG      = "WorldItem";
    public const string INV_ITEM_TAG        = "InventoryItem";
    public const string PLAYER_TAG          = "Player";
    public const string RESOURCE_TYPE       = "Resource";
    public const string WEAPON_TYPE         = "Weapon";
    public const string CONSUMABLE_TYPE     = "Consumable";
    public const int WEAPON_SLOT            = 0;
    public const string GOLD_RES            = "Gold";
    public const string DAMAGE_STAT         = "damage";
    public const string ARMOR_STAT          = "armor";
    public const string SPEED_STAT          = "speed";
    public const int NOT_APPLICABLE         = -1;
    public const string INCOMPATIBLE_MSG    = "Incompatible weapon";
    public const string NO_EQUIPPED         = "No weapon equipped";
    public const string INV_FULL            = "Inventory is full";
    public const int SLOT_1                 = 0;
    public const int SLOT_2                 = 1;
    public const int SLOT_3                 = 2;
    public const int SLOT_4                 = 3;
}

/*----------------------------------------------------------------------------------------
-- Inventory.cs - Script attached to the Inventory game object that stores a 
--                list of Slot game objects and Item game objects. 
--                Responsible for managing the weapons/items of a player
--
-- FUNCTIONS:
--		void Start()
--		public void AddItem(int id)
--		int check_if_item_in_inventory(Item item)
--      public void DestroyInventoryItem(int inv_pos)
--      public void UseConsumable(int inv_pos, int amount = 1)
--      public void UseResources(string resource_type, int amount)
--      public bool CheckIfItemCanBeAdded(bool stackable, int item_id)
--      public void UpdateWeaponStats()
--      public void DisplayWeaponError(string msg)
--      public IEnumerator DisplayInventoryFullError()
--
-- DATE:		17/02/2016
-- REVISIONS:	03/04/2016 - Add function to apply weapon stats to the player stats
--              30/03/2016 - Add functions to use items and resources
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
---------------------------------------------------------------------------------------*/
public class Inventory : MonoBehaviour
{
    GameObject _inventory_panel;
    GameObject _slot_panel;
    GameObject _inv_full_text;

    ItemManager _item_manager;
    public GameObject inventory_slot;
    public GameObject inventory_item;

    public List<Item> inventory_item_list = new List<Item>();
    public List<GameObject> slot_list = new List<GameObject>();

    public static Inventory instance;

    void Awake()
    {
        if (instance == null)         
            instance = this;              
        else if (instance != this)         
            Destroy(gameObject);          
    }

    /*---------------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		17/02/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	void Start()
    -- RETURNS: 	void
    -- NOTES:
    -- Retrieves the ItemManageer script, the InventoryPanel GameObject, the Slot Panel 
    -- GameObject and the Inventory Full GameObject.
    -- Deactivates the Inventory Full GameObject.
    -- Initializes empty inventory slot containing empty item objects.
    ----------------------------------------------------------------------------------------*/
    void Start()
    {
        _item_manager = GetComponent<ItemManager>();
        _inventory_panel = GameObject.Find("Inventory Panel");
        _slot_panel = _inventory_panel.transform.FindChild("Slot Panel").gameObject;
        _inv_full_text = _inventory_panel.transform.FindChild("Inventory Full").gameObject;
        _inv_full_text.SetActive(false);

        for (int i = 0; i < Constants.SLOT_AMOUNT; i++)
        {
            inventory_item_list.Add(new Item());
            slot_list.Add(Instantiate(inventory_slot));
            slot_list[i].GetComponent<InventorySlot>().slot_pos = i;
            slot_list[i].transform.SetParent(_slot_panel.transform);

            // Sets scaling factor of gridlayout component to 1 to work with
            // the unity "Scale With Screen Size" ui scale mode
            slot_list[i].transform.localScale = new Vector3(1, 1, 1);
        }
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	AddItem
    -- DATE: 		17/02/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void AddItem(int id, int amt = 1)
    --                  int id: The item id
    --                  int amt: The amount to add
    -- RETURNS: 	void
    -- NOTES:
    -- Takes an item id and the amount and adds an Item object into an open slot in the inventory or stacks 
    -- it on a slot containing the same item.
    -- Updates the attributes of the Item: item, amount, and item_pos
    -------------------------------------------------------------------------------------------------------*/
    public void AddItem(int id, int amt = 1)
    {
        Item _item_to_add = _item_manager.FetchItemById(id);
        int item_idx;

        if (_item_to_add.type == Constants.RESOURCE_TYPE)
        {
            GameData.MyPlayer.Resources[_item_to_add.title] += amt;
        }

        if (_item_to_add.stackable && (item_idx = check_if_item_in_inventory(_item_to_add)) != -1)
        {
            ItemData data = slot_list[item_idx].transform.GetChild(0).GetComponent<ItemData>();
            data.amount += amt;
            data.transform.GetChild(0).GetComponent<Text>().text = data.amount.ToString();
        }
        else
        {
            for (int i = 0; i < inventory_item_list.Count; i++)
            {
                if (inventory_item_list[i].id == -1)
                {
                    inventory_item_list[i] = _item_to_add;
                    GameObject _item_obj = Instantiate(inventory_item);
                    ItemData data = _item_obj.GetComponent<ItemData>();
                    data.item = _item_to_add;
                    data.item_pos = i;
                    data.amount += amt;
                    if (_item_to_add.stackable)
                        data.transform.GetChild(0).GetComponent<Text>().text = data.amount.ToString();
                    _item_obj.transform.SetParent(slot_list[i].transform);
                    // Sets scaling factor of gridlayout component to 1 to work with
                    // the unity "Scale With Screen Size" ui scale mode
                    _item_obj.transform.localScale = new Vector3(1, 1, 1);
                    _item_obj.transform.localPosition = Vector2.zero; //centers item relative to parent
                    _item_obj.GetComponent<Image>().sprite = _item_to_add.sprite;
                    _item_obj.name = _item_to_add.title; //name shown in the inspector

                    if (i == Constants.WEAPON_SLOT)
                    {
                        UpdateWeaponStats();
                    }
                    break;
                }
            }
        }
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	UseConsumable
    -- DATE: 		30/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void UseConsumable(int inv_pos, int amount = 1)
    --                  int inv_pos: The inventory position
    --                  int amt: The amount to consume
    -- RETURNS: 	void
    -- NOTES:
    -- Applies a boost to the stats specified by the item in the inventory slot. The stat boost applied
    -- is communicated to the server.
    -------------------------------------------------------------------------------------------------------*/
    public void UseConsumable(int inv_pos, int amount = 1)
    {
        Item _item = inventory_item_list[inv_pos];
        int _damage_buff = _item.damage;
        int _armor_buff = _item.armor;
        int _healing_amt = _item.health; // Assuming healing and not an increase in max health
        int _speed_buff = _item.speed;
        int _duration_of_buff = _item.duration;
        if (_duration_of_buff == Constants.NOT_APPLICABLE)
        {
            // Permament stats boost
        }

        Debug.Log("item id: " + _item.id);
        Debug.Log("damage buff: " + _item.damage);
        Debug.Log("armor buff: " + _item.armor);
        Debug.Log("healing amt: " + _item.health);
        Debug.Log("speed buff: " + _item.speed);
        Debug.Log("duration of buff" + _item.duration);

        GameManager.instance.player.GetComponent<BaseClass>().UsePotion(
            _item.damage, _item.armor, _item.health, _item.speed, _item.duration);

        NetworkingManager.send_next_packet(
            DataType.Potion, 
            GameData.MyPlayer.PlayerID, 
            new List<Pair<string, string>> {
                new Pair<string, string>("Damage", _item.damage.ToString()),
                new Pair<string, string>("Armour", _item.armor.ToString()),
                new Pair<string, string>("Health", _item.health.ToString()),
                new Pair<string, string>("Speed", _item.speed.ToString()),
                new Pair<string, string>("Duration", _item.duration.ToString())
            }, 
            Protocol.UDP);

        DestroyInventoryItem(inv_pos, 1);
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	UseResources
    -- DATE: 		30/03/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void UseResources(string resource_type, int amount)
    --                  string resource_type: The resource type
    --                  int amount: The amount used
    -- RETURNS: 	void
    -- NOTES:
    -- Updates the amount of resources the player has.
    -------------------------------------------------------------------------------------------------------*/
    public void UseResources(string resource_type, int amount)
    {
        for (int i = 0; i < Constants.SLOT_AMOUNT; i++)
        {
            Item _item = inventory_item_list[i];
            if (_item.type == Constants.RESOURCE_TYPE && _item.title == resource_type)
            {
                DestroyInventoryItem(i, amount);
                break;
            }
        }
    }
    
    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	DestroyInventoryItem
    -- DATE: 		17/02/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void DestroyInventoryItem(int inv_pos, int amount)
    --                  int inv_pos: The inventory position
    --                  int amount: The amount to destroy
    -- RETURNS: 	void
    -- NOTES:
    -- Destroys the item in the specified inventory slot position.
    -------------------------------------------------------------------------------------------------------*/
    public void DestroyInventoryItem(int inv_pos, int amount)
    {
        GameObject item_to_remove = slot_list[inv_pos].transform.GetChild(0).gameObject;
        ItemData _data = item_to_remove.GetComponent<ItemData>();

        if ((_data.amount -= amount) <= 0)
        {
            Destroy(item_to_remove);
            inventory_item_list[inv_pos] = new Item();
        }
        else
        {
            _data.transform.GetChild(0).GetComponent<Text>().text = _data.amount.ToString();
        }

        if (_data.item.type == Constants.RESOURCE_TYPE)
        {
            GameData.MyPlayer.Resources[_data.item.title] -= amount;
        }

        if (inv_pos == Constants.WEAPON_SLOT)
        {
            UpdateWeaponStats();
        }
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CheckIfItemCanBeAdded
    -- DATE: 		17/02/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	CheckIfItemCanBeAdded(bool stackable, int item_id)
    --                  bool stackable: Whether the item is stackable or not
    --                  int item_id: The item id
    -- RETURNS: 	bool: true if the item can be added to the inventory, false if not
    -- NOTES:
    -- Checks if an item can be added to the inventory.
    -------------------------------------------------------------------------------------------------------*/
    public bool CheckIfItemCanBeAdded(bool stackable, int item_id)
    {
        foreach (Item item in inventory_item_list)
        {
            if (item.id == -1)
            {
                return true;
            }
            if (stackable && item.id == item_id)
            {
                return true;
            }
        }
        return false;
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	check_if_item_in_inventory
    -- DATE: 		17/02/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	int check_if_item_in_inventory(Item item)
    --                  Item item: The item object
    -- RETURNS: 	int: The inventory position of the item if found, -1 if not found
    -- NOTES:
    -- Checks whether the item being added is already present in the inventory.
    -- Used to see if an item should be stacked or placed on a new slot when being added.
    -------------------------------------------------------------------------------------------------------*/
    int check_if_item_in_inventory(Item item)
    {
        for (int i = 0; i < inventory_item_list.Count; i++)
        {
            if (inventory_item_list[i].id == item.id)
            {
                return i;
            }
        }
        return -1;
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	UpdateWeaponStats
    -- DATE: 		03/04/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void UpdateWeaponStats()
    -- RETURNS: 	void
    -- NOTES:
    -- Updates the weapon stat bonus the player receives for equipping a weapon
    -------------------------------------------------------------------------------------------------------*/
    public void UpdateWeaponStats()
    {
        string _weapon_error_msg = Constants.NO_EQUIPPED;
        int damage = 0;
        int armor = 0;
        int speed = 0;

        var classStat = GameManager.instance.player.GetComponent<BaseClass>().ClassStat;

        if(GameData.MyPlayer.WeaponStats[Constants.DAMAGE_STAT] != 0)
            classStat.AtkPower -= GameData.MyPlayer.WeaponStats[Constants.DAMAGE_STAT];
        if(GameData.MyPlayer.WeaponStats[Constants.ARMOR_STAT] != 0)
            classStat.Defense -= GameData.MyPlayer.WeaponStats[Constants.ARMOR_STAT];
        if (GameData.MyPlayer.WeaponStats[Constants.SPEED_STAT] != 0)
            classStat.MoveSpeed -= GameData.MyPlayer.WeaponStats[Constants.SPEED_STAT];

        if (inventory_item_list[Constants.WEAPON_SLOT].id != -1)
        {
            ItemData _data = slot_list[Constants.WEAPON_SLOT].transform.GetChild(0).GetComponent<ItemData>();
            if (_data.item.type != Constants.WEAPON_TYPE)
            {
                _weapon_error_msg = Constants.NO_EQUIPPED;
            } else if (_data.item.classType == (int)GameData.MyPlayer.ClassType ||
                _data.item.classType == Constants.NOT_APPLICABLE)
            {
                _weapon_error_msg = "";
                damage = _data.item.damage;
                armor = _data.item.armor;
                speed = _data.item.speed;
            }
            else
            {
                _weapon_error_msg = Constants.INCOMPATIBLE_MSG;
            }
        }
        DisplayWeaponError(_weapon_error_msg);

        GameData.MyPlayer.WeaponStats[Constants.DAMAGE_STAT] = damage;
        GameData.MyPlayer.WeaponStats[Constants.ARMOR_STAT] = armor;
        GameData.MyPlayer.WeaponStats[Constants.SPEED_STAT] = speed;

        if (damage != 0)
            classStat.AtkPower += damage;
        if(armor != 0)
            classStat.Defense += armor;
        if (speed != 0)
            classStat.MoveSpeed += speed;
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	DisplayWeaponError
    -- DATE: 		03/04/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public void DisplayWeaponError(string msg)
    --                  string msg: the message to display
    -- RETURNS: 	void
    -- NOTES:
    -- Display an error message when an incompatible weapon is equipped or if there is no weapon equiped.
    -------------------------------------------------------------------------------------------------------*/
    public void DisplayWeaponError(string msg)
    {
        _inventory_panel.transform.FindChild("Weapon Error").GetComponent<Text>().text = msg;
    }

    /*-------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	DisplayInventoryFullError
    -- DATE: 		03/04/2016
    -- REVISIONS:   
    -- DESIGNER:  	Joseph Tam-Huang
    -- PROGRAMMER: 	Joseph Tam-Huang
    -- INTERFACE: 	public IEnumerator DisplayInventoryFullError()
    -- RETURNS: 	IEnumerator
    -- NOTES:
    -- Displays the inventory full message for 3 seconds.
    -------------------------------------------------------------------------------------------------------*/
    public IEnumerator DisplayInventoryFullError()
    {
        Debug.Log("display inv full error called");
        _inv_full_text.SetActive(true);
        yield return new WaitForSeconds(3);
        _inv_full_text.SetActive(false);
    }
}
