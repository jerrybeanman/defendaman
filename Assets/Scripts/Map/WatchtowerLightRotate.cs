using UnityEngine;
using System.Collections;

public class WatchtowerLightRotate : MonoBehaviour {

	// Adjust this to change the speed of rotation
	public float cycleTime;
	public float angle;
	public Sprite allyTop;
	public Sprite enemyTop;

	Building parent;

	
	bool set = false;
	float elapsedTime = 0f;
	Quaternion startingRotation;
	Quaternion targetRotation;

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
		startingRotation = transform.localRotation;
		targetRotation = Quaternion.Euler(new Vector3(0f, 0f, transform.localRotation.z + angle));
		print ("Targer: " + targetRotation.z);
		print ("starting: " + startingRotation.z);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!parent.placing && !set && parent.team == GameData.MyPlayer.TeamID)
		{
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(true);
			set = !set;
		}
		if(set)
		{
			if(elapsedTime < cycleTime)
			{
				elapsedTime += Time.deltaTime; // <- move elapsedTime increment here
				// Rotations
				transform.localRotation = Quaternion.Slerp(startingRotation, targetRotation,  (elapsedTime / cycleTime));
			}else
			{
				elapsedTime = 0;
				angle *= -1f;
				targetRotation =  Quaternion.Euler (new Vector3(0f, 0f, transform.localRotation.z + angle));
				startingRotation = transform.localRotation; // have a startingRotation as well
			}
		}
	}
}
