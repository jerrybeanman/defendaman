using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Attack : MonoBehaviour {

    BaseClass player;

    bool attackReady = true, specialReady = true;
    
   //Start of scripts creation. Used to instantiate variables in our case.
    void Start() {
        player = gameObject.GetComponent<BaseClass>();
    }
    
    //Called every frame
    void Update() {
        if (Input.GetKey(KeyCode.Mouse0) && attackReady) {
            //left click attack
            attackReady = false;
            var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            float delay = player.basicAttack(dir);
            Invoke("enableAttack", delay);
            NetworkingManager.send_next_packet(DataType.Trigger, player.playerID, new List<Pair<string, string>> {
                new Pair<string, string>("Attack", "0"),
                new Pair<string, string>("DirectionX", dir.x.ToString()),
                new Pair<string, string>("DirectionY", dir.y.ToString())
            }, Protocol.UDP);
        } 

        if (Input.GetKey(KeyCode.Mouse1) && specialReady) {
            //right click attack
            specialReady = false;
            var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            float delay = player.specialAttack(dir);
            Invoke("enableSpecial", delay);
            NetworkingManager.send_next_packet(DataType.Trigger, player.playerID, new List<Pair<string, string>> {
                new Pair<string, string>("Attack", "1"),
                new Pair<string, string>("DirectionX", dir.x.ToString()),
                new Pair<string, string>("DirectionY", dir.y.ToString()),
            }, Protocol.UDP);
        }
    }
	

    private void enableAttack()
    {
        attackReady = true;
    }


    private void enableSpecial()
    {
        specialReady = true;
    }
}

