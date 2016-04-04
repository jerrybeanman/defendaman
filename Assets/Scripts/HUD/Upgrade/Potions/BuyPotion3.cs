using UnityEngine;
using System.Collections;

public class BuyPotion3 : MonoBehaviour {
	private Inventory _inventory;
	
	
	public void buyPotion1 () {
		_inventory= GameObject.Find("Inventory").GetComponent<Inventory>();
		_inventory.AddItem(6,1);
		
	}
	
}
