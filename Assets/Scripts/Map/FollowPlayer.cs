using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var target = GameObject.Find("GameManager").GetComponent<NetworkingManager>().player;
        if (target != null) {
            transform.position = new Vector3(target.transform.position.x,
                                            target.transform.position.y,
                                            transform.position.z);
        }
	}
}
