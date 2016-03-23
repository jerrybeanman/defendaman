using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		var target = GameManager.instance.player;
        if (target != null) {
            transform.position = new Vector3(target.transform.position.x,
                                            target.transform.position.y,
                                            transform.position.z);
        }
	}
}
