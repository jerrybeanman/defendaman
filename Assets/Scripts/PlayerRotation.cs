using UnityEngine;
using System.Collections;

/*
Carson:
Quick PlayerRotation to look at mouse script.
*/

public class PlayerRotation : MonoBehaviour {
    public Quaternion curRotation;

    //Updates the characters rotation every frame
    void FixedUpdate() {
        //Get the mouses position in the world
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var rotation = mousePosition - transform.position;
        //Rotate towards that mouse
        transform.rotation = Quaternion.LookRotation(Vector3.forward, rotation);
        curRotation = transform.rotation;
    }
}