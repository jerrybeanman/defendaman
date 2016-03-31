using UnityEngine;
using System;

public class Upgrader : MonoBehaviour {

	void OnMouseDown() {
		if (gameObject.tag == "Armory") {
			GameObject.Find ("HUD").transform.GetChild (12).gameObject.SetActive (true);
		} else if (gameObject.tag == "Alchemist") {
			GameObject.Find ("HUD").transform.GetChild (11).gameObject.SetActive (true);
		}
	}
}

