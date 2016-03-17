using UnityEngine;
using System.Collections;

public class RotateWithPlayer : MonoBehaviour {
	//how fast the FOV will follow the player's rotation (smaller = more lag)
	private float speed = 1.0f;
	public Transform target;

	// Update is called once per frame
	void FixedUpdate () {
		if (target != null
		    && (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)) {
			//transform.rotation = target.transform.rotation;
			Quaternion targetRotation = target.transform.rotation * Quaternion.Euler(0,0,-90);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
			                                        Time.time * speed);
        }
	}
}
