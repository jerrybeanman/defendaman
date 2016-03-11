using UnityEngine;
using System.Collections;

public class RotateWithPlayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var target = GameObject.Find("GameManager").GetComponent<NetworkingManager>().player;
		if (target != null
		    && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)) {
			transform.rotation = target.transform.rotation;
        }
	}
}
