using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position = new Vector3(transform.position.x, 0, transform.position.z + .1f);
        if (Input.GetKey(KeyCode.A))
            transform.position = new Vector3(transform.position.x -.1f, 0, transform.position.z);
        if (Input.GetKey(KeyCode.S))
            transform.position = new Vector3(transform.position.x, 0, transform.position.z - .1f);
        if (Input.GetKey(KeyCode.D))
            transform.position = new Vector3(transform.position.x + .1f, 0, transform.position.z);
    }
}
