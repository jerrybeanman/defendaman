using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class WeaponUpgrade : MonoBehaviour {
	int monies;
	private Inventory _inventory;
	public void upgrade(){
		Dictionary<string, int> MyResources = GameData.MyPlayer.Resources;
		_inventory = GameObject.Find("Inventory").GetComponent<Inventory>();
		monies = MyResources[Constants.GOLD_RES];

		if(monies>50)
		switch(GameData.MyPlayer.ClassType)	
		{
			case ClassType.Gunner:
			print (monies);
				switch(_inventory.inventory_item_list[0].id){
					//If stick, upgrade to slingshot
					case 1:
						if(MyResources[Constants.GOLD_RES] >= 600)
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(12,1);
						_inventory.UseResources(Constants.GOLD_RES,600);
						HUD_Manager.instance.upgradeAmount.text = "$800";
						print (MyResources[Constants.GOLD_RES]);
						break;
					//If slingshot, upgrade to handgun
					case 12:
						if(MyResources[Constants.GOLD_RES]>=800){
							_inventory.DestroyInventoryItem(0,1);
							_inventory.AddItem(13,1);
							_inventory.UseResources(Constants.GOLD_RES,800);	
							HUD_Manager.instance.upgradeAmount.text = "$1000";
							print (MyResources[Constants.GOLD_RES]);
						}
							break;
					//If handgun, upgrade to two handguns
					case 13:
						if(MyResources[Constants.GOLD_RES]>=1000){
							_inventory.DestroyInventoryItem(0,1);
							_inventory.AddItem(14,1);		
							_inventory.UseResources(Constants.GOLD_RES,1000);	
							HUD_Manager.instance.upgradeAmount.text = "$1200";
					
							print (MyResources[Constants.GOLD_RES]);
						}
						break;
					//If two handguns... upgrade to 3?....
					case 14:
						if(MyResources[Constants.GOLD_RES]>=1200){
							_inventory.DestroyInventoryItem(0,1);
							_inventory.AddItem(15,1);	
							_inventory.UseResources(Constants.GOLD_RES,1200);
							HUD_Manager.instance.upgradeAmount.text = "$1400";
					
							print (MyResources[Constants.GOLD_RES]);
						}
						break;
					//If 3 handguns.. upgradae to machine gun
					case 15:
					if(MyResources[Constants.GOLD_RES]>=1400){
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(16,1);		
						_inventory.UseResources(Constants.GOLD_RES,1400);
						HUD_Manager.instance.upgradeAmount.text = "$1600";
						print (MyResources[Constants.GOLD_RES]);
					}
						break;

				}
			print ("I'm a gunner");
				break;

			case ClassType.Ninja:
				switch(_inventory.inventory_item_list[0].id){
					//If stick, upgrade to greater stick
				case 1:
				if(MyResources[Constants.GOLD_RES] >= 600)
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(8,1);
					_inventory.UseResources(Constants.GOLD_RES,600);
					HUD_Manager.instance.upgradeAmount.text = "$600";
				
					print (MyResources[Constants.GOLD_RES]);
					break;
					//If greater stick, upgrade to greatest stick
				case 8:
				if(MyResources[Constants.GOLD_RES]>=800){
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(9,1);
					_inventory.UseResources(Constants.GOLD_RES,800);
					HUD_Manager.instance.upgradeAmount.text = "$800";
					
					print (MyResources[Constants.GOLD_RES]);
				}
					break;
					//If greatest stick, upgrde to 4th dimensional rod
				case 9:
				if(MyResources[Constants.GOLD_RES]>=1000){
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(10,1);	
					_inventory.UseResources(Constants.GOLD_RES,1000);
					HUD_Manager.instance.upgradeAmount.text = "$1000";
					
					print (MyResources[Constants.GOLD_RES]);
				}
					break;
					//If 4th  dimensional rod, upgrade to Mace of pain.
				case 10:
					if(MyResources[Constants.GOLD_RES]>=1200){
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(11,1);	
						_inventory.UseResources(Constants.GOLD_RES,1200);
						HUD_Manager.instance.upgradeAmount.text = "$1200";
						print (MyResources[Constants.GOLD_RES]);
					}
					break;
					
				}
				print("im a ninja");
				break;
			case ClassType.Wizard:
				switch(_inventory.inventory_item_list[0].id){
				//If stick, upgrade to fancy playing cards
				case 1:
				if(MyResources[Constants.GOLD_RES] >= 600)
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(17,1);
					_inventory.UseResources(Constants.GOLD_RES,600);
					HUD_Manager.instance.upgradeAmount.text = "$600";
				
					print (MyResources[Constants.GOLD_RES]);
					break;
					//If fancy playing cards, upgrade to Magical wand
				case 17:
					if(MyResources[Constants.GOLD_RES]>=800){
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(18,1);				
						_inventory.UseResources(Constants.GOLD_RES,800);
						HUD_Manager.instance.upgradeAmount.text = "$800";
					
						print (MyResources[Constants.GOLD_RES]);
					}
					break;
					//If magical wand, upgrade to magiccal shovel
				case 18:
					if(MyResources[Constants.GOLD_RES]>=1000){
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(19,1);
						_inventory.UseResources(Constants.GOLD_RES,1000);
						HUD_Manager.instance.upgradeAmount.text = "$1000";
					
						print (MyResources[Constants.GOLD_RES]);
					}
					break;
					//If magical shovel, upgrade to ball of fire.
				case 19:
				if(MyResources[Constants.GOLD_RES]>=1200){
						_inventory.DestroyInventoryItem(0,1);
						_inventory.AddItem(20,1);
						_inventory.UseResources(Constants.GOLD_RES,1200);
						HUD_Manager.instance.upgradeAmount.text = "$1200";
					
						print (MyResources[Constants.GOLD_RES]);
					}
					break;
					//If ball of fire, upgrade to Fire Wave.
				case 20:
				if(MyResources[Constants.GOLD_RES]>=1400){
					_inventory.DestroyInventoryItem(0,1);
					_inventory.AddItem(21,1);	
					_inventory.UseResources(Constants.GOLD_RES,1400);
					HUD_Manager.instance.upgradeAmount.text = "$1400";
					
					print (MyResources[Constants.GOLD_RES]);
				}
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
