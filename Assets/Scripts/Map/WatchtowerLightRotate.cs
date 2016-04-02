using UnityEngine;
using System.Collections;

public class WatchtowerLightRotate : MonoBehaviour {


	public float 	cycleTime;			// Adjust this to change the speed of rotation
	public float 	angle;				// Angle in degrees to rotate by
	public Sprite 	allyTop;			// Ally's sprite to render
	public Sprite 	enemyTop;			// Enemy's sprite to render

	private Building 	parent;				// Parent object, which contains a buliding component
	private float 		elapsedTime = 0f;	// Total time elapsed before reaching cycleTime
	private Quaternion 	startingRotation;	// Starting rotation of object
	private Quaternion 	targetRotation;		// Target rotation of object

	// Use this for initialization
	void Start () 
	{
		parent = transform.parent.gameObject.GetComponent<Building>();

		// Load sprite correspondingly 
		if(parent.team == GameData.MyPlayer.TeamID)
			gameObject.GetComponent<SpriteRenderer>().sprite = allyTop;
		else
			gameObject.GetComponent<SpriteRenderer>().sprite = enemyTop;

		// Only activate vision system if the building created is on the ally team, not placing it and when 
		//the building has finished construction
		if(parent.placing || parent.team != GameData.MyPlayer.TeamID || !parent.constructed)
		{
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
		}

		// Stores our starting rotation
		startingRotation = transform.localRotation;

		// Calculate our targeting rotation
		targetRotation = Quaternion.Euler(new Vector3(0f, 0f, transform.localRotation.z + angle));
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(!parent.placing && parent.team == GameData.MyPlayer.TeamID && parent.constructed)
		{
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(true);
		}
		if(parent.constructed)
		{
			SmoothRotation();
		}
	}

	/*----------------------------------------------------------------------------
    --	Smoothly rotate the object attached back and forth in a ping pong fasion
    --
    --	Interface:  private void SmoothRotation()
    --
    --	programmer: Jerry Jia
    --	@return: void
	------------------------------------------------------------------------------*/
	private void SmoothRotation()
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
