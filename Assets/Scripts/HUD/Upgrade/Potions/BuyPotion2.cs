using UnityEngine;
using System.Collections;

public class BuyPotion2 : MonoBehaviour {
	private Inventory _inventory;
	
	
	public void buyPotion1 () {
		_inventory= GameObject.Find("Inventory").GetComponent<Inventory>();
		_inventory.AddItem(5,1);
		
	}
	
}
