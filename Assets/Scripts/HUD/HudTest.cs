using UnityEngine;
using System.Collections;

public class HudTest : MonoBehaviour {
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown("1"))
			HUD_Manager.instance.PlayerTakeDmg(0.05f);
		if(Input.GetKeyDown("2"))
			HUD_Manager.instance.AllyKingTakeDmg(0.05f);
		if(Input.GetKeyDown("3"))
			HUD_Manager.instance.EnemyKingTakeDmg(0.05f);
		if(Input.GetKeyDown("4"))
			HUD_Manager.instance.UpdateCurrency(100);
	}
}
