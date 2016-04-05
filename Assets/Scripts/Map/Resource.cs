﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*-----------------------------------------------------------------------------
-- Resource.cs - Script attached to GameManager game object
--                       responsible for resources objects and behaviour.
--
-- FUNCTIONS:
--		void Start()
--		void Update()
--		void DecreaseAmount(int amount)
--      void OnCollisionEnter2D(Collision2D other)
--		IEnumerator ExplodeAndDestroy()
--		List<Pair<string, string>> 
--                CreateResourceTakenMessage(int x, int y, int amt)
--
-- DATE:		05/03/2016
-- REVISIONS:	March 31 - Add networking logic
-- DESIGNER:	Jaegar Sarauer, Krystle Bulalakaw
-- PROGRAMMER:  Krystle Bulalakaw
-----------------------------------------------------------------------------*/
class Resource : MonoBehaviour {
	public int x {get; set;}
	public int y {get; set;}
	public int amount {get; set;}
	public int instanceId {get; set;}
	public bool trigger_entered {get; set;}
	public Animator animator {get; set;}
	
    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	Start
    -- DATE: 		March 24, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	Start(void)
    -- RETURNS: 	void.
    -- NOTES:
    -- Initialization of the resource game object's components.
    ----------------------------------------------------------------------------------------------------------------------*/
	void Start () {
		animator = GetComponent<Animator>();
	}
	
    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	Update
    -- DATE: 		March 24, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	Update(void)
    -- RETURNS: 	void.
    -- NOTES:
    -- Called once per frame. If resource amount is 0, triggers the resource depletion animation and disables collision
    -- on the game object.
    ----------------------------------------------------------------------------------------------------------------------*/
	void Update () {
	}
	
    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	DecreaseAmount
    -- DATE: 		March 24, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	DecreaseAmount(int damage)
    --					int damage: the amount of damage dealt by the player
    -- RETURNS: 	void.
    -- NOTES:
    -- Decreases the amount of a resource object by the amount of damage done to it.
    -- On resource depletion, send a message to the server to deplete and respawn the resource.
    ----------------------------------------------------------------------------------------------------------------------*/
	public void DecreaseAmount(int damage) {
		int decreaseAmount = damage;
		if ((this.amount - damage) < 0) { // 0 amount left
			decreaseAmount = this.amount;
		}

		this.amount -= decreaseAmount;
		DropGold(decreaseAmount);

		if (this.amount == 0) {
			SendResourceDepletedMessage();
			SendResourceRespawnMessage();
		} 
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	DropGold
    -- DATE: 		March 30, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	DropGold(int amount)
    --					int amount - amount of gold to drop
    -- RETURNS: 	void.
    -- NOTES:
    -- Creates a gold world item with some amount.
    -- Its X and Y position is offset so that it doesn't drop in the same spot every time, and so it is easier to pick
    -- up (not right in the center of the tree).
    ----------------------------------------------------------------------------------------------------------------------*/
    private void DropGold(int amount) {
		float offset = 1.5f;
		float offsetX = Random.Range (-offset, offset);
		float offsetY = Random.Range (-offset, offset);
        int instance_id = WorldItemManager.Instance.GenerateWorldItemId();
        int gold_id = 2;

        GameObject go = WorldItemManager.Instance.CreateWorldItem(instance_id, gold_id, amount, x + offsetX, y + offsetY);
		go.AddComponent<CoinMagnetize>();
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreateResourceTakenMessage
    -- DATE: 		March 30, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	List<Pair<string, string>> CreateResourceTakenMessage(int x, int y, int amt)
    --					int x      - resource X position
    --                  int y      - resource Y position
    --					int amount - amount of gold to drop
    -- RETURNS: 	List<Pair<string, string>>   - List of map event data 
    -- NOTES:
    -- Creates the message to send to the server that a resource was taken.
    ----------------------------------------------------------------------------------------------------------------------*/
    public List<Pair<string, string>> CreateResourceTakenMessage(int amt) {
		List<Pair<string, string>> _message = CreateResourcePositionMessage();
		_message.Add(new Pair<string, string>(NetworkKeyString.Amount, amt.ToString()));
		
		return _message;
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreateResourceDepletedMessage
    -- DATE: 		April 1, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE: 	List<Pair<string, string>> CreateResourcePositionMessage()
    -- RETURNS: 	List<Pair<string, string>>   - List of map event data 
    -- NOTES:
    -- Creates the message to send to the server of its X and Y position.
    ----------------------------------------------------------------------------------------------------------------------*/
    public List<Pair<string, string>> CreateResourcePositionMessage() {
		List<Pair<string, string>> _message = new List<Pair<string, string>>();
		
		_message.Add(new Pair<string, string>(NetworkKeyString.XPos, x.ToString()));
		_message.Add(new Pair<string, string>(NetworkKeyString.YPos, y.ToString()));

		return _message;
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	SendMessageToServer
    -- DATE: 		April 1, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE: 	void SendMessageToServer(List<Pair<string, string>> msg, int eventType)
    --                      List<Pair<string, string>> msg - the message to send
    --                      int eventType                  - the map event type
    -- RETURNS: 	List<Pair<string, string>>   - List of map event data 
    -- NOTES:
    -- Sends a message to the server via Networking Manager's send_next_packet call.
    ----------------------------------------------------------------------------------------------------------------------*/
    void SendMessageToServer(List<Pair<string, string>> msg, int eventType) {
		var packet = NetworkingManager.send_next_packet(DataType.Environment, eventType, msg, Protocol.TCP);
		string temp = "[" + packet + "]"; // Wrap JSON child into array
		// Fakes network data updates for local testing. Comment this line when actually testing on network.
		if (Application.platform != RuntimePlatform.LinuxPlayer) {
		    NetworkingManager.instance.update_data(temp);
		}
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	SendResourceTakenMessage
    -- DATE: 		April 1, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE: 	void SendResourceTakenMessage(int amount) 
    --					int amount - the amount of resource that was taken
    -- RETURNS: 	void.
    -- NOTES:
    -- Creates a message to send to the server to indicate that a resource was taken.
    ----------------------------------------------------------------------------------------------------------------------*/
    public void SendResourceTakenMessage(int amount) {
		List<Pair<string, string>> msg = CreateResourceTakenMessage(amount);
		SendMessageToServer(msg, (int)MapManager.EventType.RESOURCE_TAKEN);
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	SendResourceDepletedMessage
    -- DATE: 		April 1, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE: 	void SendResourceDepletedMessage()
    -- RETURNS: 	void.
    -- NOTES:
    -- Creates a message to send to the server to indicate that a resource was depleted.
    ----------------------------------------------------------------------------------------------------------------------*/
    void SendResourceDepletedMessage() {
		List<Pair<string, string>> msg = CreateResourcePositionMessage();
		SendMessageToServer(msg, (int)MapManager.EventType.RESOURCE_DEPLETED);
	}

    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	SendResourceRespawnMessage
    -- DATE: 		April 1, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:    Krystle Bulalakaw
    -- PROGRAMMER:  Krystle Bulalakaw
    -- INTERFACE: 	void SendResourceRespawnMessage()
    -- RETURNS: 	void.
    -- NOTES:
    -- Creates a message to send to the server to indicate that a resource was depleted.
    ----------------------------------------------------------------------------------------------------------------------*/
    void SendResourceRespawnMessage() {
		List<Pair<string, string>> msg = CreateResourcePositionMessage();
		SendMessageToServer(msg, (int)MapManager.EventType.RESOURCE_RESPAWN);
	}
}
