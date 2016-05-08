using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BuyPotion1 : MonoBehaviour {
	private Inventory _inventory;

	Dictionary<string, int> MyResources; 
	public void buyPotion1 () {
		print ("Button 1 pressed");
		MyResources=GameData.MyPlayer.Resources;
		if(MyResources[Constants.GOLD_RES]>=500){
			_inventory= GameObject.Find("Inventory").GetComponent<Inventory>();
			_inventory.AddItem(7,1);
			_inventory.UseResources(Constants.GOLD_RES,500);	
			print (MyResources[Constants.GOLD_RES]);
		}
	}

}
