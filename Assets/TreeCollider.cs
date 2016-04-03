using UnityEngine;
using System.Collections;

public class TreeCollider : MonoBehaviour {

	Resource tree;

	// Use this for initialization
	void Start () {
		tree = gameObject.GetComponentInParent<Resource>();
	}
	
	// Update is called once per frame
	void Update () {
	
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
    -- Decreases the resource amount by damage dealt to it.
    ----------------------------------------------------------------------------------------------------------------------*/
	void OnTriggerEnter2D(Collider2D other) {
		// Prevents health bar trigger
		if (other.GetComponent<Trigger>() != null) {
			float damage = other.GetComponent<Trigger>().damage;
			tree.SendResourceTakenMessage((int)damage);
		} 
	}
}
