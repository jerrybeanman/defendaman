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

            Vector2 dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
            Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
            attack.AddForce(dir * 100);
            attack.GetComponent<BasicRanged>().teamID = gameObject.GetComponent<BaseClass>().team;
            attack.GetComponent<BasicRanged>().damage = gameObject.GetComponent<BaseClass>().ClassStat.AtkPower;
            Invoke("enableAttack", 1);
            //Invoke("enableAttack", delay);
        } else if (Input.GetKey(KeyCode.Mouse1) && !attacking) {
			//right click attack
			//attacking = true;
			//delay = special_attack();
			//WaitForSeconds(delay);
			//attacking = false;
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

