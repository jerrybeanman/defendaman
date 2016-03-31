using UnityEngine;
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
--                CreateResourceDecreasedNetworkMessage(int x, int y, int amt)
--
-- DATE:		05/03/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Jaegar Sarauer, Krystle Bulalakaw
-- PROGRAMMER:  Krystle Bulalakaw
-----------------------------------------------------------------------------*/
class Resource : MonoBehaviour {
	public int x {get; set;}
	public int y {get; set;}
	public int amount {get; set;}
	public int instanceId {get; set;}
	public bool trigger_entered {get; set;}
	public bool depleted {get; set;}
	Animator animator {get; set;}
	
	public Resource(int x, int y) {
		this.x = x;
		this.y = y;
		this.amount = 10;
	}
	
	public Resource(int x, int y, int ra) {
		this.x = x;
		this.y = y;
		this.amount = ra;
	}
	
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
    -- INTERFACE: 	DecreaseAmount(int amount)
    --					int amount: the remaining quantity of the resource object on the map
    -- RETURNS: 	void.
    -- NOTES:
    -- Decreases the amount of a resource object by some number.
    -- On resource depletion, start a coroutine to play the explosion animation then destroy the object.
    ----------------------------------------------------------------------------------------------------------------------*/
	public void DecreaseAmount(int amount) {
		string amt1, amt2;
		amt1 = this.amount.ToString();
		this.amount -= amount;
		
		DropGold (amount);
		
		if (this.amount <= 0) {
			this.amount = 0;
			depleted = true;
			StartCoroutine(ExplodeAndDestroy());
			StartCoroutine (Respawn());
		}
		
		Debug.Log("Decreased resource amount from " + amt1 + " to " + this.amount + " at (" + this.x + ", " + this.y + ")" );
	}
	
	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	ExplodeAndDestroy
    -- DATE: 		March 30, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	ExplodeAndDestory()
    -- RETURNS: 	IEnumerator - generic enumerator used to run a coroutine alongside Update()
    -- NOTES:
    -- Plays the explosion animation on resource depletion, waits for animation to complete, then destroys the gameobject.
    ----------------------------------------------------------------------------------------------------------------------*/
	IEnumerator ExplodeAndDestroy() {
		animator.SetTrigger("Depleted");
		// TODO: get animation clip length
		yield return new WaitForSeconds(0.267f);
		//Destroy(gameObject);
		gameObject.SetActive(false);
	}
	
	// Not working
	IEnumerator Respawn() {
		yield return new WaitForSeconds(3f);
		this.amount = 5;
		animator.ResetTrigger("Depleted");
		gameObject.SetActive (true);
	}
	
	
	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnTriggerEnter2D
    -- DATE: 		March 30, 2016
    -- REVISIONS: 	March 31 - Use network updating logic
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	OnTriggerEnter2D(Collider2D other)
    --					Collider2D other - the object that triggered the collision box
    -- RETURNS: 	void.
    -- NOTES:
    -- Triggered when the resource object's collision box is triggered by a Collider 2D object (player attack)
    -- Decreasea the resource amount by some number.
    ----------------------------------------------------------------------------------------------------------------------*/
	void OnTriggerEnter2D(Collider2D other) {
		int amount = 1;
		//DecreaseAmount(amount);
		List<Pair<string, string>> msg = CreateResourceDecreasedNetworkMessage(x, y, amount);
		Debug.Log ("resource taken message: " + msg);
		Debug.Log ("networking manager instance: " + NetworkingManager.instance);
		
		var packet = NetworkingManager.send_next_packet(DataType.Environment, (int)MapManager.EventType.RESOURCE_TAKEN, msg, Protocol.TCP);
		// Wrap JSON child into array
		string temp = "[" + packet + "]";
		Debug.Log ("packet: " + temp);
		// Fakes network data updates for local testing. Comment this line when actually testing on network.
		NetworkingManager.instance.update_data(temp);
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
		float offsetX = Random.Range (-1.0f, 1.0f);
		float offsetY = Random.Range (-1.0f, 1.0f);
		WorldItemManager.Instance.CreateWorldItem(100, 2, amount, x + offsetX, y + offsetY);
	}
	
	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	CreateResourceDecreasedNetworkMessage
    -- DATE: 		March 30, 2016
    -- REVISIONS: 	N/A
    -- DESIGNER:  	Krystle Bulalakaw
    -- PROGRAMMER: 	Krystle Bulalakaw
    -- INTERFACE: 	List<Pair<string, string>> CreateResourceDecreasedNetworkMessage(int x, int y, int amt)
    --					int x      - resource X position
    --                  int y      - resource Y position
    --					int amount - amount of gold to drop
    -- RETURNS: 	List<Pair<string, string>>   - List of map event data 
    -- NOTES:
    -- Creates a gold world item with some amount.
    -- Its X and Y position is offset so that it doesn't drop in the same spot every time, and so it is easier to pick
    -- up (not right in the center of the tree).
    ----------------------------------------------------------------------------------------------------------------------*/
	public List<Pair<string, string>> CreateResourceDecreasedNetworkMessage(int x, int y, int amt)
	{
		List<Pair<string, string>> _message = new List<Pair<string, string>>();
		
		_message.Add(new Pair<string, string>(NetworkKeyString.XPos, x.ToString()));
		_message.Add(new Pair<string, string>(NetworkKeyString.YPos, y.ToString()));
		_message.Add(new Pair<string, string>("ResourceAmountTaken", amt.ToString()));
		
		return _message;
	}
}
