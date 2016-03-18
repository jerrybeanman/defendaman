using UnityEngine;
using System.Collections;

public class LightFollowPlayer : MonoBehaviour {
	//Amount it slows down, also known as damp time.
	//public float friction = 0.15f;
	//private Vector3 velocity = Vector3.zero;
	public Transform target;
	
	// Update is called once per frame
	void FixedUpdate () {
        if (target != null) {
            //Vector3 destination = new Vector3(target.transform.position.x, target.transform.position.y,
			//                                  transform.position.z);
			//transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, friction);
			transform.position = new Vector3(target.transform.position.x, target.transform.position.y,
			                                 transform.position.z);
        }
	}
}
