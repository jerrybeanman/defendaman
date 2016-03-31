using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
    public const int NOT_APPLICABLE         = -1;
    public const string INCOMPATIBLE_MSG    = "Incompatible weapon";
    public const string NO_EQUIPPED         = "No weapon equipped";
    public const string INV_FULL            = "Inventory is full";
    public const int SLOT_1                 = 0;
    public const int SLOT_2                 = 1;
    public const int SLOT_3                 = 2;
    public const int SLOT_4                 = 3;
}

/*-----------------------------------------------------------------------------
-- Inventory.cs - Script attached to the Inventory game object that stores a 
--                list of Slot game objects and Item game objects. 
--                Responsible for managing the weapons/items of a player
--
-- FUNCTIONS:
--		void Start()
--		public void AddItem(int id)
--		int check_if_item_in_inventory(Item item)
--      public void DestroyInventoryItem(int inv_pos)
--
-- DATE:		17/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Joseph Tam-Huang
-- PROGRAMMER:  Joseph Tam-Huang
-----------------------------------------------------------------------------*/
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

    /*
     * Initializes empty inventory slot containing empty item objects
     */
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
        // TODO: Remove this. Adding initial items to the inventory (testing)
        //AddItem(1);
        //AddItem(2);
        //AddItem(2, 200);
        //AddItem(3, 10);
        //AddItem(0);

        //AddItem(1);
        //AddItem(5, 5);
        //AddItem(6);
        //AddItem(7);
    }

    /*
     * Binds inventory items to number keys. The keys 1, 2, 3, 4 will call useConsumable()
     * on the items in matching inventory slots if the item is a consumable.
     */
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (inventory_item_list[Constants.SLOT_1].type == Constants.CONSUMABLE_TYPE)
            {
                Debug.Log("1 is consumable");
                UseConsumable(Constants.SLOT_1);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (inventory_item_list[Constants.SLOT_2].type == Constants.CONSUMABLE_TYPE)
            {
                Debug.Log("2 is consumable");
                UseConsumable(Constants.SLOT_2);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (inventory_item_list[Constants.SLOT_3].type == Constants.CONSUMABLE_TYPE)
            {
                Debug.Log("3 is consumable");
                UseConsumable(Constants.SLOT_3);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            if (inventory_item_list[Constants.SLOT_4].type == Constants.CONSUMABLE_TYPE)
            {
                Debug.Log("4 is consumable");
                UseConsumable(Constants.SLOT_4);
            }
        }
    }

    /* 
     * Takes an item id and the amount and adds an Item object into an open slot in the inventory or stacks 
     * it on a slot containing the same item.
     * Updates the attributes of the Item: item, amount, and item_pos  
     */
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

    // Need to do a check before calling ie: make sure GameData.MyPlayer.Resources[resource_type] >= amount
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

    /* 
     * Destroys the item in the specified inventory slot position
     */
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


    /* 
     * Checks whether the item being added is already present in the inventory.
     * Used to see if an item should be stacked or placed on a new slot when being added
     */
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

    /*
     * Updates the weapon stat bonus the player receives for equipping a weapon
     */
    public void UpdateWeaponStats()
    {
        string _weapon_error_msg = Constants.NO_EQUIPPED;
        int damage = 0;
        int armor = 0;

        var classStat = GameManager.instance.player.GetComponent<BaseClass>().ClassStat;

        classStat.AtkPower -= GameData.MyPlayer.WeaponStats[Constants.DAMAGE_STAT];
        classStat.Defense -= GameData.MyPlayer.WeaponStats[Constants.ARMOR_STAT];

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
            }
            else
            {
                _weapon_error_msg = Constants.INCOMPATIBLE_MSG;
            }
        }
        DisplayWeaponError(_weapon_error_msg);

        GameData.MyPlayer.WeaponStats[Constants.DAMAGE_STAT] = damage;
        GameData.MyPlayer.WeaponStats[Constants.ARMOR_STAT] = armor;

        classStat.AtkPower += damage;
        classStat.Defense += armor;
    }

    public void DisplayWeaponError(string msg)
    {
        _inventory_panel.transform.FindChild("Weapon Error").GetComponent<Text>().text = msg;
    }

    public IEnumerator DisplayInventoryFullError()
    {
        Debug.Log("display inv full error called");
        _inv_full_text.SetActive(true);
        yield return new WaitForSeconds(3);
        _inv_full_text.SetActive(false);

    }
}
