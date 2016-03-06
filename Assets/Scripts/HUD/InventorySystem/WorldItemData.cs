using UnityEngine;
using System.Collections;

public class WorldItemData : MonoBehaviour
{
    Inventory _inventory;
    public Item item;
    public int amount;
    bool trigger_entered = false;
    int player_id;
    //var player = GameObject.Find("GameManager").GetComponent<NetworkingManager>().player.transform.position;
    void Start ()
    {
        _inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
        player_id = GameObject.Find("GameManager").GetComponent<NetworkingManager>().myPlayer;
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.F) && trigger_entered)
        {
            Debug.Log("trigger detected from worlditemdata");
            // data to send to server indicating that the player wants to pick up an item
            // player_id    
            // item.id
            // this.transform.position.x
            // this.transform.position.y
            // amount
            //_inventory.AddItem(item.id, amount);

        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("collision detected");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("trigger enter");

        if (other.gameObject.tag == "Player" && 
            other.gameObject.GetComponent<BaseClass>().playerID == player_id)
        {
            trigger_entered = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        trigger_entered = false;
        Debug.Log("trigger exit");
    }
    
}
