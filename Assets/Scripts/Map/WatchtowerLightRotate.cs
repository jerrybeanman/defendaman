using UnityEngine;
using System.Collections;

public class WatchtowerLightRotate : MonoBehaviour {

	// Adjust this to change the speed of rotation
	private int speed = 2;
	public Transform TowerVision;
	public Transform TowerVisionHidden;

	// Use this for initialization
	void Start () {
		TowerVision = gameObject.transform.GetChild (0);
		TowerVisionHidden = gameObject.transform.GetChild (1);
	}
	
	// Update is called once per frame
	void Update () {
		TowerVision.Rotate (new Vector3 (0, 0, speed));
		TowerVisionHidden.Rotate (new Vector3 (0, 0, speed));
	}
}
