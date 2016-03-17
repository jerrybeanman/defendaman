using UnityEngine;
using System.Collections;

public class GunnerClass : BaseClass
{
<<<<<<< Updated upstream
    int[] distance = new int[2]{ 10, 20};
    int[] speed = new int[2] { 100, 200 };
    Rigidbody2D bullet = (Rigidbody2D)Resources.Load("Prefabs/Bullet", typeof(Rigidbody2D));
    Rigidbody2D bullet2 = (Rigidbody2D)Resources.Load("Prefabs/Bullet2", typeof(Rigidbody2D));

    public GunnerClass()
	{
=======
    int[] distance = new int[2]{ 15, 60};
    int[] speed = new int[2] { 800, 1200 };
    Rigidbody2D bullet;
    Rigidbody2D bullet2;
    int inaccuracy = 40;
    Camera mainCamera;
    float zoomOut = 20;
    float zoomIn;
    bool inSpecial;
    bool fired;

    GunnerClass()
    {
>>>>>>> Stashed changes
        this._className = "Gunner";
        this._classDescription = "Test class Gunner";
        this._classStat.CurrentHp = 100;
        this._classStat.MaxHp = 150;

        //placeholder numbers
        this._classStat.MoveSpeed = 15;
        this._classStat.AtkPower = 20;
        this._classStat.Defense = 5;
        inSpecial = false;
        fired = false;

        cooldowns = new float[2] { 0.2f, 5f };
    }

    new void Start()
    {
        base.Start();
        bullet = (Rigidbody2D)Resources.Load("Prefabs/SmallBullet", typeof(Rigidbody2D));
        bullet2 = (Rigidbody2D)Resources.Load("Prefabs/SmallBullet", typeof(Rigidbody2D));
        

        var controller = Resources.Load("Controllers/gunboi") as RuntimeAnimatorController;
        gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        zoomIn = mainCamera.orthographicSize;
    }

    //attacks return time it takes to execute
    public override float basicAttack(Vector2 dir)
    {
        base.basicAttack(dir);

<<<<<<< Updated upstream
        //var dir = ((Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position)).normalized;
        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
        attack.AddForce(dir * speed[0]);
=======
        var innacX = Random.value * inaccuracy - (inaccuracy / 2);
        var innacY = Random.value * inaccuracy - (inaccuracy / 2);
        var newMouse = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + innacX, Input.mousePosition.y + innacY, 0));
        var newdir = (Vector2)((newMouse - transform.position).normalized);
        
        var startPosition = new Vector3(transform.position.x + (dir.x * 2.5f), transform.position.y + (dir.y * 2.5f), -5);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, startPosition, transform.rotation);
        attack.AddForce(newdir * speed[0]);
        attack.GetComponent<BasicRanged>().playerID = playerID;
>>>>>>> Stashed changes
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower/5;
        attack.GetComponent<BasicRanged>().maxDistance = distance[0];

        return cooldowns[0];
    }

    public override float specialAttack(Vector2 dir)
    {
        base.specialAttack(dir);

<<<<<<< Updated upstream
        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet2, transform.position, transform.rotation);
        attack.AddForce(dir * speed[1]);
=======
        this.dir = dir;
        inSpecial = true;

        return cooldowns[1];
    }

    Vector2 dir;
    void Update() {
        if (inSpecial && Input.GetMouseButton(1))
        {
            var startPosition = new Vector3(transform.position.x + (dir.x * 2.5f), transform.position.y + (dir.y * 2.5f), -5);

            /*Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet2, startPosition, transform.rotation);
            attack.AddForce(dir * speed[1]);
            attack.GetComponent<BasicRanged>().playerID = playerID;
            attack.GetComponent<BasicRanged>().teamID = team;
            attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower * 3;
            attack.GetComponent<BasicRanged>().maxDistance = distance[1];*/
            if (mainCamera.orthographicSize < zoomOut)
                mainCamera.orthographicSize += .1f;
            MapManager.cameraDistance = -mainCamera.orthographicSize;
        }

        if (inSpecial && !Input.GetMouseButton(1))
        {
            inSpecial = false;
            fire();
        }
        if (mainCamera.orthographicSize > zoomIn && !Input.GetMouseButton(1)) {
            mainCamera.orthographicSize -= .2f;
            MapManager.cameraDistance = -mainCamera.orthographicSize;
        }
    }

    void fire()
    {
        //Replace with lazer beammmm
        var innacX = Random.value * inaccuracy - (inaccuracy / 2);
        var innacY = Random.value * inaccuracy - (inaccuracy / 2);
        var newMouse = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + innacX, Input.mousePosition.y + innacY, 0));
        var newdir = (Vector2)((newMouse - transform.position).normalized);

        var startPosition = new Vector3(transform.position.x + (newdir.x * 2.5f), transform.position.y + (newdir.y * 2.5f), -5);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, startPosition, transform.rotation);
        attack.AddForce(newdir * speed[0]);
        attack.GetComponent<BasicRanged>().playerID = playerID;
>>>>>>> Stashed changes
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower / 5;
        attack.GetComponent<BasicRanged>().maxDistance = distance[1];
    }
} 
