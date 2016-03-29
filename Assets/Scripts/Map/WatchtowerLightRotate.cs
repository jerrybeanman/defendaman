using UnityEngine;
using System.Collections;

public class WatchtowerLightRotate : MonoBehaviour {

	// Adjust this to change the speed of rotation
	public int speed = 2;
	public Sprite allyTop;
	public Sprite enemyTop;

	Building parent;
	
	// Use this for initialization
	void Start () 
	{
		parent = transform.parent.gameObject.GetComponent<Building>();
		if(parent.team == GameData.MyPlayer.TeamID)
			gameObject.GetComponent<SpriteRenderer>().sprite = allyTop;
		else
			gameObject.GetComponent<SpriteRenderer>().sprite = enemyTop;

		if(parent.placing)
		{
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
		}
		if(parent.team != GameData.MyPlayer.TeamID)
		{
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
		}
	}

	bool set = false;
	// Update is called once per frame
	void Update () 
	{
		if(!parent.placing && !set && parent.team == GameData.MyPlayer.TeamID)
		{
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(true);
			set = !set;
		}
		transform.Rotate(new Vector3(0, 0, speed));
	}
}
