using UnityEngine;
using System.Collections;

/*
Carson:
Basic WASD absolute/static movement.
*/

public class Movement : MonoBehaviour {
    //Speed to move at
    public float Speed = 0;

    //Horizontal movement. 1 is right, -1 is left, 0 is no movement.
    private float movex;

    //Vertical movement. 1 is up, -1 is down, 0 is no movement.
    private float movey;
    
    //Start of scripts creation. Used to instantiate variables in our case.
    void Start() {
        movex = 0f;
        movey = 0f;
    }
    
    //Called every frame
    void FixedUpdate() {
        //Get the x and y movement
        movex = Input.GetAxis("Horizontal");
        movey = Input.GetAxis("Vertical");
        //Add velocity to the object based on this velocity.
        GetComponent<Rigidbody2D>().velocity = new Vector2(movex * Speed, movey * Speed);
    }

    /*##Exact Movement##, left/right/up/down, no diagnol
    void Update() {

        if (Input.GetKey(KeyCode.A))
            movex = -1;
        else if (Input.GetKey(KeyCode.D))
            movex = 1;
        else
            movex = 0;
        if (Input.GetKey(KeyCode.W))
            movey = 1;
        else
            movey = 0;
    }

    void FixedUpdate() {

        rigidbody2D.velocity = new Vector2(movex * Speed, movey * Speed);
    }
    */
}

