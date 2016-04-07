using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*-----------------------------------------------------------------------------
-- Magnetize.cs - Script attached to game objects
--                       responsible for pulling items towards the player.
--
-- FUNCTIONS:
--      void Start()
--      void Update()
--      void CheckIfInRange()
--
-- DATE:		April 2, 2016
-- REVISIONS:   April 3 - Tweaked velocity
--              April 5 - Add networking logic based on WorldItem & PlayerID  
-- DESIGNER:	Krystle Bulalakaw
-- PROGRAMMER:  Krystle Bulalakaw
-----------------------------------------------------------------------------*/
public class Magnetize : MonoBehaviour {
    public int playerId = -1;
    public int world_item_id { get; set; }
	GameObject target;
	float dist = 15f;
	float velocity = 4f;
	float acceleration = 1.1f;
    bool inRange = false;

	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	Update
    -- DATE: 		April 2, 2016
    -- REVISIONS: 	April 5 - Add networking logic based on player ID
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	Update(void)
    -- RETURNS: 	void.
    -- NOTES:
    -- Called once per frame. Checks if the player has come within range of the item.
    -- If a player ID has been assigned from WorldItemManager, moves the object towards the player specified at an 
    -- exponentially increasing velocity.
    ----------------------------------------------------------------------------------------------------------------------*/
	void Update() {
        if (!inRange) {
            CheckIfInRange();
        }

        // Only magnetize the item if playerID is set and is equal to MyPlayerID
        if (playerId != -1 && GameData.MyPlayer.PlayerID == playerId) {
            Vector3 playerPosition;
            if (GameData.PlayerPosition.TryGetValue(playerId, out playerPosition)) {
                transform.position = Vector3.MoveTowards(transform.position, playerPosition, Time.deltaTime * velocity);
                velocity *= acceleration;
            }
        }
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CheckIfInRange
    -- DATE: 		April 5, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	CheckIfInRange(void)
    -- RETURNS: 	void.
    -- NOTES:
    -- Sends a message to the server if the player comes within range of the item and their inventory is not full.
    ----------------------------------------------------------------------------------------------------------------------*/
    void CheckIfInRange() {
        Vector3 playerPosition;
        int playerId = GameData.MyPlayer.PlayerID; 

        if (GameData.PlayerPosition.TryGetValue(playerId, out playerPosition)) {
            if (Inventory.instance.CheckIfItemCanBeAdded(true, 2)) {
                if (Vector3.Distance(transform.position, playerPosition) < dist) {
                    inRange = true;
                    List<Pair<string, string>> msg = CreateMagnetizeMessage(playerId);
                    SendMessageToServer(msg, (int)ItemUpdate.Magnetize);
                }
            }
        }
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreateMagnetizeMessage
    -- DATE: 		April 5, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE: 	List<Pair<string, string>> CreateMagnetizeMessage()
    -- RETURNS: 	List<Pair<string, string>>   - List of magnetize event data 
    -- NOTES:
    -- Creates the message to magnetize an item towards a player using their ID.
    ----------------------------------------------------------------------------------------------------------------------*/
    public List<Pair<string, string>> CreateMagnetizeMessage(int playerId) {
        List<Pair<string, string>> _message = new List<Pair<string, string>>();
        _message.Add(new Pair<string, string>(NetworkKeyString.PlayerID, playerId.ToString()));
        _message.Add(new Pair<string, string>(NetworkKeyString.WorldItemID, world_item_id.ToString()));
        return _message;
    }

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	SendMessageToServer
    -- DATE: 		April 5, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE: 	void SendMessageToServer(List<Pair<string, string>> msg, int eventType)
    --                      List<Pair<string, string>> msg - the message to send
    --                      int eventType                  - the item event type
    -- RETURNS: 	List<Pair<string, string>>   - List of map event data 
    -- NOTES:
    -- Sends a message to the server via Networking Manager's send_next_packet call.
    ----------------------------------------------------------------------------------------------------------------------*/
    void SendMessageToServer(List<Pair<string, string>> msg, int eventType) {
        var packet = NetworkingManager.send_next_packet(DataType.Item, eventType, msg, Protocol.TCP);
        
        if (Application.platform != RuntimePlatform.LinuxPlayer) {
            string temp = "[" + packet + "]"; 
            NetworkingManager.instance.update_data(temp);
        }
    }
}
