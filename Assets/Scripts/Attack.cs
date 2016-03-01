using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {
	bool attacking = false;
	float delay;
    Rigidbody2D bullet = (Rigidbody2D)Resources.Load("Bullet", typeof(Rigidbody2D));
   //Start of scripts creation. Used to instantiate variables in our case.
    void Start() {
        
    }
    
    //Called every frame
    void Update() {
		if(Input.GetKey(KeyCode.Mouse0) && !attacking) {
			//left click attack
			attacking = true;
            //delay = basic_attack();

            //TODO: attack stats as parameters
            int team = gameObject.GetComponent<BaseClass>().team;
            float speed = 100;
            float atkpower = gameObject.GetComponent<BaseClass>().ClassStat.AtkPower;
            float delay = 0.5f;


            var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
            attack.AddForce(dir * speed);
            attack.GetComponent<BasicRanged>().teamID = team;
            attack.GetComponent<BasicRanged>().damage = atkpower;
            Invoke("enableAttack", delay);
        } else if (Input.GetKey(KeyCode.Mouse1) && !attacking) {
            //right click attack
            attacking = true;
            //delay = special_attack();


            //TODO: attack stats as parameters
            int team = gameObject.GetComponent<BaseClass>().team;
            float speed = 100;
            float atkpower = gameObject.GetComponent<BaseClass>().ClassStat.AtkPower;
            float delay = 0.5f;

            Debug.Log("right clicked");

            Invoke("enableAttack", delay);
        }
       
    }
	
	//attacks return time it takes to execute
	public float basic_attack()
    {

        return 0;
    }
	//public abstract float special_attack();

    private void enableAttack()
    {
        attacking = false;
    }
}

