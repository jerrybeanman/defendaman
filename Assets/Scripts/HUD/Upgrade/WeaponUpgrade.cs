using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class WeaponUpgrade : MonoBehaviour {

	private Inventory _inventory;
	public void upgrade(){
		Dictionary<string, int> MyResources = GameData.MyPlayer.Resources;
		_inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
		switch(GameData.MyPlayer.ClassType)
		{
			case ClassType.Gunner:
			print (MyResources);
				switch(_inventory.inventory_item_list[0].id){
					//If stick, upgrade to slingshot
					case 1:
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(12,1);
						break;
					//If slingshot, upgrade to handgun
					case 12:
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(13,1);					
						break;
					//If handgun, upgrade to two handguns
					case 13:
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(14,1);					
						break;
					//If two handguns... upgrade to 3?....
					case 14:
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(15,1);					
						break;
					//If 3 handguns.. upgradae to machine gun
					case 15:
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(16,1);					
						break;

				}
			print ("I'm a gunner");
				break;

			case ClassType.Ninja:
				switch(_inventory.inventory_item_list[0].id){
					//If stick, upgrade to greater stick
				case 1:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(8,1);
					break;
					//If greater stick, upgrade to greatest stick
				case 8:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(9,1);					
					break;
					//If greatest stick, upgrde to 4th dimensional rod
				case 9:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(10,1);					
					break;
					//If 4th  dimensional rod, upgrade to Mace of pain.
				case 10:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(11,1);					
					break;
					
				}
				print("im a ninja");
				break;
			case ClassType.Wizard:
				switch(_inventory.inventory_item_list[0].id){
					//If stick, upgrade to fancy playing cards
				case 1:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(17,1);
					break;
					//If fancy playing cards, upgrade to Magical wand
				case 17:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(18,1);					
					break;
					//If magical wand, upgrade to magiccal shovel
				case 18:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(19,1);					
					break;
					//If magical shovel, upgrade to ball of fire.
				case 19:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(20,1);					
					break;
					//If ball of fire, upgrade to Fire Wave.
				case 20:
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(21,1);					
					break;
				}
			print("im a Wizrd");
			break;
			default:
				print ("wtf");
			break;
		}	

		//print ("Upgrade call test" + i1.id);
	}
}
