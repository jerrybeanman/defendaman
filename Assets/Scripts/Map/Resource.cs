using UnityEngine;
using System.Collections;

/*-----------------------------------------------------------------------------
-- WorldItemManager.cs - Script attached to GameManager game object
--                       responsible for managing world items.
--
-- FUNCTIONS:
--		void Start()
--		void Update()
--		void DecreaseAmount(int amount)
--      void OnCollisionEnter2D(Collision2D other)
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
	void DecreaseAmount(int amount) {
		string amt1, amt2;
		amt1 = this.amount.ToString();
		this.amount -= amount;

		DropGold (amount);

		if (this.amount <= 0) {
			this.amount = 0;
			depleted = true;
			StartCoroutine(ExplodeAndDestroy());
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
		yield return new WaitForSeconds(0.65f);
		Destroy(gameObject);
	}

	/*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: 	OnTriggerEnter2D
    -- DATE: 		March 30, 2016
    -- REVISIONS: 	N/A
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
		DecreaseAmount(1);
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
		// Create Gold world item with playerId = 100 at this resource's X and Y
		WorldItemManager.Instance.CreateWorldItem(100, 2, amount, x + offsetX, y + offsetY);
	}
}
