using UnityEngine;
using System.Collections;

public class WatchtowerLightRotate : MonoBehaviour {

	// Adjust this to change the speed of rotation
	public float cycleTime;
	public float angle;
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
		StartCoroutine(LoopRotation());
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
		//transform.Rotate(new Vector3(0, 0, speed));
	}

	IEnumerator LoopRotation()
	{
		while(true)
		{
			float elapsedTime = 0f;
			Quaternion startingRotation = transform.rotation; // have a startingRotation as well
			Quaternion targetRotation =  Quaternion.Euler (0f, 0f, transform.rotation.z + angle);
			while (elapsedTime < cycleTime) 
			{
				elapsedTime += Time.deltaTime; // <- move elapsedTime increment here
				// Rotations
				transform.rotation = Quaternion.Slerp(startingRotation, targetRotation,  (elapsedTime / cycleTime));
				yield return new WaitForEndOfFrame ();
			}
			angle *= -1f;
		}
	}
}
