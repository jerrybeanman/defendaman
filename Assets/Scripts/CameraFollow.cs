using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{

    public float cameraDistOffset = 10;
    private Camera mainCamera;
    private GameObject player;

   
    void Start()
    {
        mainCamera = GetComponent<Camera>();
        //Who to follow
        player = GameObject.Find("New Sprite");
    }

    
    void Update()
    {
        Vector3 playerInfo = player.transform.transform.position;
        mainCamera.transform.position = new Vector3(playerInfo.x, playerInfo.y, playerInfo.z - cameraDistOffset);
    }
}