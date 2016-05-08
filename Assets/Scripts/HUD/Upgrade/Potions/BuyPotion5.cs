using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuyPotion5 : MonoBehaviour {
	private Inventory _inventory;
	
	Dictionary<string, int> MyResources;	

	public void buyPotion1 () {
		MyResources=GameData.MyPlayer.Resources;
		if(MyResources[Constants.GOLD_RES]>250){
			_inventory= GameObject.Find("Inventory").GetComponent<Inventory>();
			_inventory.AddItem(3,1);
			_inventory.UseResources(Constants.GOLD_RES,250);	
			print (MyResources[Constants.GOLD_RES]);
		}
	}
	
}
