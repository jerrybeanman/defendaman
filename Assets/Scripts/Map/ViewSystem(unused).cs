using UnityEngine;
using System.Collections;


/*--------------------------------------------------------------------------------------------  
--  SOURCE:          ViewSystem
--  
--  PROGRAM:         ViewSystem (Unused in final product)
--  
--  FUNCTIONS:       void Start ();
--                   
--                   void FixedUpdate();
--                   
--                   float realAngle();
--                   
--                   void buildPointsAndMesh();
--                   
--                   Mesh buildMesh(params Vector2[] points);
--                   
--                   Vector2 LineCastColl(Vector2 thisPos, Vector2 endPos);
--                   
--                   void drawView(params Vector2[] points);
--  
--  DATE:            ViewSystem (Unused in final product)
--  
--  DESIGNERS:       Jaegar Sarauer
--  
--  REVISIONS:       Jaegar Sarauer = Initial creation for 360 degree vision system.
--                   Micah Willems = Changes to work with character of the game.
--  
--  PROGRAMMERS:     Jaegar Sarauer
--  
--  NOTES:           This file creates a 360 degree vision system around the character intended to
--                   act as a vision system similar to the one in place in the current game.
--                   The vision works by ray casting out from the center to find collision points,
--                   then building a mesh based on all the collisions, and applying a filter to the mesh.
--                   This vision system was removed when Jerry bought a library which allowed differnent
--                   shapes of vision to be applied, of which Micah fine tuned it and applied it to the
--                   characters and teams of the game.
------------------------------------------------------------------------------------------*/
public class ViewSystem : MonoBehaviour {

	Vector2[] shadowPoints;
	private int casts = 45;
	private int viewDistance = 5;
	public Mesh mesh;
	public GameObject player;
	private int degreeOfCasts;
	private float playerX, playerY, rot;
	
	// Use this for initialization
	void Start() {
		mesh = new Mesh();
		gameObject.AddComponent<MeshFilter>().mesh = mesh;
		degreeOfCasts = 360 / casts;
		shadowPoints = new Vector2[casts];
		MeshCollider collider = gameObject.AddComponent(typeof(MeshCollider)) as MeshCollider;
		collider.convex = true;
		collider.isTrigger = true;
		buildPointsAndMesh();
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		Vector3[] vertices = mesh.vertices;
		Vector3[] normals = mesh.normals;
		int t = 0;
		//player = GameObject.Find("GameManager").GetComponent<NetworkingManager>().player;
		while (t < vertices.Length) {
			vertices[t] += normals[t] * Mathf.Sin(Time.time);
			t++;
		}
		// GetComponent<MeshFilter>().transform.position = new Vector2(transform.localPosition.x, transform.localPosition.y);
		mesh.vertices = vertices;
		GetComponent<MeshCollider>().sharedMesh = mesh;
		buildPointsAndMesh();
	}
	
	float realAngle() {
		if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0
		    || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)
		    || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
			rot = -player.transform.rotation.eulerAngles.z + 90;
		}
		return rot;
	}
	
	void buildPointsAndMesh() {
		player = GameManager.instance.player;
		if (player == null) {
			return;
		}
		for (int i = 0; i < casts; i++) {
			Vector2 checker = new Vector2(Mathf.Cos(Mathf.Deg2Rad * i * degreeOfCasts) + Mathf.Sin(Mathf.Deg2Rad * realAngle()) * .9f, 
			                              Mathf.Sin(Mathf.Deg2Rad * i * degreeOfCasts) + Mathf.Cos(Mathf.Deg2Rad * -realAngle()) * .9f);
			shadowPoints[i] = LineCastColl(transform.position, checker * 10);
		}
		buildMesh(shadowPoints);
	}
	
	Mesh buildMesh(params Vector2[] points) {
		// Use the triangulator to get indices for creating triangles
		Triangulator tr = new Triangulator(points);
		int[] indices = tr.Triangulate();
		
		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[points.Length];
		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] = new Vector3(points[i].x, points[i].y, 0);
		}
		
		// Create the mesh
		Mesh msh = new Mesh();
		msh.vertices = vertices;
		msh.triangles = indices;
		msh.RecalculateNormals();
		msh.RecalculateBounds();
		
		
		MeshFilter mf = GetComponent<MeshFilter>();
		mf.mesh = msh;
		return msh;
	}
	
	Vector2 LineCastColl(Vector2 thisPos, Vector2 endPos) {
		RaycastHit2D hit;
		
		Vector2 endCheck = new Vector2(endPos.x + thisPos.x, endPos.y + thisPos.y);
		
		hit = Physics2D.Linecast(thisPos, endCheck, 1 << LayerMask.NameToLayer("Water"));
		//Debug.DrawLine(thisPos, endCheck, Color.green);
		if (hit.collider != null)
			return hit.point - thisPos;
		else
			return endPos;
	}
	
	void drawView(params Vector2[] points) {
		for (int i = 0; i < points.Length; i++) {
			if (i == points.Length - 1) {
				if (points[i].x != 0 && points[0].x != 0) {
					Debug.DrawLine(points[i], points[0]);
				}
			} else
				if (points[i].x != 0 && points[i + 1].x != 0)
					Debug.DrawLine(points[i], points[i + 1]);
		}
		
	}
}
