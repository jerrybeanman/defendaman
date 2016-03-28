using UnityEngine;
using System.Collections;

public class WatchtowerLightRotate : MonoBehaviour {

	// Adjust this to change the speed of rotation
	public int speed = 2;
	Building parent;
	// Use this for initialization
	void Start () 
	{
		parent = transform.parent.gameObject.GetComponent<Building>();
		if(parent.placing)
		{
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
		}
	}

	bool set = false;
	// Update is called once per frame
	void Update () 
	{
		if(!parent.placing && !set)
		{
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(true);
			set = !set;
		}
		transform.Rotate(new Vector3(0, 0, speed));
	}
}
