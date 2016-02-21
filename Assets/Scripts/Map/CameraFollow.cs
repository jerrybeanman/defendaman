using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    private GameObject Player;

	// Use this for initialization
	void Start ()
    {
        if (Player != null)
        Player = GameObject.Find("Player");
    }
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(Player.transform.position.x, 10, Player.transform.position.z);
    }
}
