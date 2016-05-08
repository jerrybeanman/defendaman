using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BuyPotion2 : MonoBehaviour {
	private Inventory _inventory;
	
	Dictionary<string, int> MyResources;
	public void buyPotion1 () {
		MyResources=GameData.MyPlayer.Resources;
		if(MyResources[Constants.GOLD_RES]>=700){
			_inventory= GameObject.Find("Inventory").GetComponent<Inventory>();
			_inventory.AddItem(5,1);
			_inventory.UseResources(Constants.GOLD_RES,700);	
			print (MyResources[Constants.GOLD_RES]);
		}
		
	}
	
}
