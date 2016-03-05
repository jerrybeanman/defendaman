using UnityEngine;
using System.Collections;

public class HudTest : MonoBehaviour {
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetKeyDown("1"))
			HUD_Manager.instance.UpdatePlayerHealth(-0.05f);
		if(Input.GetKeyDown("2"))
			HUD_Manager.instance.UpdateAllyKingHealth(-0.05f);
		if(Input.GetKeyDown("3"))
			HUD_Manager.instance.UpdateEnemyKingHealth(-0.05f);
		if(Input.GetKeyDown("4"))
			HUD_Manager.instance.UpdateCurrency(100);
		if(Input.GetKeyDown("q"))
			HUD_Manager.instance.UseMainSkill(1.0f);
		if(Input.GetKeyDown("e"))
			HUD_Manager.instance.UseSubSkill(5.0f);
	}
}
