using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

    BaseClass player;

    bool attackReady = true, specialReady = true;
    
   //Start of scripts creation. Used to instantiate variables in our case.
    void Start() {
        player = gameObject.GetComponent<BaseClass>();
    }
    
    //Called every frame
    void Update() {
		if(Input.GetKey(KeyCode.Mouse0) && attackReady) {
            //left click attack
            attackReady = false;
            float delay = player.basicAttack();
            Invoke("enableAttack", delay);
        } else if (Input.GetKey(KeyCode.Mouse1) && attackReady && specialReady) {
            //right click attack
            attackReady = false;
            specialReady = false;
            float[] delay = player.specialAttack();
            Invoke("enableAttack", delay[0]);
            Invoke("enableSpecial", delay[1]);
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

