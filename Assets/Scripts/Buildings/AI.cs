/*------------------------------------------------------------------------------------------------------------------
-- SOURCE FILE: AI.cs - The AI for turrets, and some older code for movement before it was cut
-- PROGRAM: DefendAman
--
-- FUNCTIONS:
--		void Start()
--      void UpdateAI(JSONClass packet)
--      public void instantTurret(float reload, int speed, int teamToIgnore, int range, int damage1)
--      void Update() 
--      private bool checkBlocked(Vector2 attackSpot)
--      private void createBullet(Vector2 attackDir)
--      private void setFace(Vector2 face)
--      Vector2 getRoute()
--      Vector2 getIntersection(Vector2 cur, Vector2 last, int spd)
--      public float getDegree(float angle)
--
-- DATE:		19/01/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Colin Bose
-- PROGRAMMER:  Colin Bose
--
-- NOTES:
-- Attach this to a turret to have a turret that shoots at enemies.
----------------------------------------------------------------------------------------------------------------------*/

using UnityEngine;
using System.Collections;
using SimpleJSON;

public class AI : MonoBehaviour {
    bool route = false;
    float facing = 0;
    float accuracy = 3.5f;
    public float angleFacing;
    Pair<int, Vector2> lastLocation = new Pair<int, Vector2>(-5, new Vector2());
    int range = 35;
    Vector2 curMove;
    public float xCoord, yCoord;
    private Rigidbody2D rb2d;
    private int speed = 35;
    public double reload ;
	public double resetReload;
    public int damage = 10;
    public Rigidbody2D bullet;
    public double swap;
    public int team = -2;
    public int aiID = 0;
    int teamSwap;
    double reloadSwap;
	public Building parent;


    /*--------------s----------------------------------------------------------------------------------------------------
    -- FUNCTION:  start
    -- DATE:	   10/04/16
    -- REVISIONS:  (V1.0)
    -- DESIGNER:   Colin Bose
    -- PROGRAMMER: Colin Bose
    --
    -- 
    -- RETURNS:    void
    --
    -- NOTES: The constructor. Sets initial parameters
    --
    ----------------------------------------------------------------------------------------------------------------------*/
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
       	//bullet = (Rigidbody2D)Resources.Load("Prefabs/Bullet", typeof(Rigidbody2D));
		NetworkingManager.Subscribe(UpdateAI, DataType.AI, aiID);
        NetworkingManager.Subscribe(CreateProjectile, DataType.AIProjectile, aiID);
        rb2d = GetComponent<Rigidbody2D>();
        
        gameObject.layer = 2;
		parent = gameObject.GetComponent<Building>();
        Debug.Log("Constructed");
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  instantTurret
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    void
--
-- NOTES: sets the parameters of the turret - reload speed, bullet speed, who to attack, range and damage
--
----------------------------------------------------------------------------------------------------------------------*/

    public void instantTurret(float reload, int speed, int teamToIgnore, int range, int damage1)
    {
        Debug.Log("init called");
        this.resetReload = reload;
        this.reload = reload;
        reloadSwap = reload;
        teamSwap = teamToIgnore;
        this.speed = speed;
        this.team = teamToIgnore;
        this.range = range;
        this.damage = damage1;
        swap = reload;
        Debug.Log("Stuff:" + reload + " " + speed + " " + teamToIgnore + " " + range + " " + damage);
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  createProjectile
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    VOID
--
-- NOTES: Creates a projectile based on a packet received from the server.
--
----------------------------------------------------------------------------------------------------------------------*/

    void CreateProjectile(JSONClass packet)
    {
        //I created a projectile
        Vector2 attack;
        attack.x = packet["vecX"].AsFloat;
        attack.y = packet["vecY"].AsFloat;




        Rigidbody2D attack2 = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);

        attack2.AddForce(attack * speed);
        Debug.Log(attack);
        attack2.GetComponent<BasicRanged>().teamID = team;
        attack2.GetComponent<BasicRanged>().damage = 10;
        attack2.GetComponent<BasicRanged>().maxDistance = 10;
        reload = 1;
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  updateAI
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    VOID
--
-- NOTES: updates the ai based on data from the server
--
----------------------------------------------------------------------------------------------------------------------*/

    void UpdateAI(JSONClass packet)
    {
    
        Debug.Log("Received packet: " + packet.ToString());
        xCoord = packet["x"].AsFloat;
        yCoord = packet["y"].AsFloat;
        angleFacing = packet["facing"].AsFloat;
        Vector2 newPos = new Vector2(xCoord, yCoord);

        transform.rotation = Quaternion.AngleAxis((float)angleFacing, Vector3.forward);
    }

    /*------------------------------------------------------------------------------------------------------------------
 -- FUNCTION:  update
 -- DATE:	   10/04/16
 -- REVISIONS:  (V1.0)
 -- DESIGNER:   Colin Bose
 -- PROGRAMMER: Colin Bose
 --
 -- 
 -- RETURNS:    VOID
 --
 -- NOTES: Called 60 times per second, does all the logic for the AI each frame
 --
 ----------------------------------------------------------------------------------------------------------------------*/

    void Update() {
        if (parent.placing && !parent.constructed)
            return;
        /*if (reload > 1000)
        {
            reload = reloadSwap;    
            resetReload = reloadSwap;            
        }
        */
        // Debug.Log("Reload: " + reload);
        if (GameData.GameState == GameState.Won || GameData.GameState == GameState.Lost)
            return;

        Vector3 vec = new Vector3();
        Vector3 face = new Vector3();
        float closest = 999;
        int id = 0, realId;
        if (!route)
        {
            curMove = getRoute();
            route = true;
        }
        foreach (var playerData in GameData.LobbyData)
        {
           // Debug.Log("Player TEam: " + playerData.Value.TeamID + "Ignoring: " + team);
            if (playerData.Value.TeamID == team)
            {
                continue;
            }
            if (!GameData.PlayerPosition.ContainsKey(playerData.Value.PlayerID))
            {
                continue;
            }
            float dist;
            vec = GameData.PlayerPosition[playerData.Key];
            realId = playerData.Value.PlayerID;
            dist = Mathf.Sqrt(Mathf.Pow((rb2d.position.x - vec.x), 2) + Mathf.Pow((rb2d.position.y - vec.y), 2));
            if (dist < closest)
            {
                id = realId;
                closest = dist;
                face = vec;
                lastLocation.first = realId;
            }
        }
        if (closest > range)
        {
            id = -1;
            lastLocation.first = -1;
            reload -= Time.deltaTime;
            return;
        }
        setFace(face);

        if (reload < 0)
        {
            Vector2 attackSpot = new Vector2();
            Vector2 attack = new Vector2();

            if (face.x != lastLocation.second.x || face.y != lastLocation.second.y)
            {
                if (id == lastLocation.first && face != (Vector3)lastLocation.second)
                {

                    attackSpot = getIntersection(face, lastLocation.second, 25);
                    attack = attackSpot;
                }
                else
                {
                    attack.x = face.x - rb2d.position.x;
                    attack.y = face.y - rb2d.position.y;
                }
            }
            else
            {


                attack.x = face.x - rb2d.position.x;
                attack.y = face.y - rb2d.position.y;

            }


            //attack.Normalize();
            /*
            System.Random rnd = new System.Random();
            float offset;
            
            offset = (float)(rnd.NextDouble() * accuracy - accuracy / 2);
            //offsetY = (float)(rnd.NextDouble() * 3.0 - 1.5);
            if (attack.x > attack.y)
            {
                attack.x += attack.x * offset;

            }
            else
            {
                attack.y += attack.y * offset;
            }
            */

            if (checkBlocked(attack))
            {
                attack.Normalize();
                createBullet(attack);
                
            }
            else
            {
                reload -= Time.fixedDeltaTime;
            }
            transform.rotation = Quaternion.AngleAxis(facing, Vector3.forward);
            lastLocation.first = id;
            lastLocation.second = face;

          /*  attack.Normalize();
            Rigidbody2D attack2 = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
            attack2.AddForce(attack * speed * 2.5f);
            attack2.GetComponent<BasicRanged>().teamID = team;
            attack2.GetComponent<BasicRanged>().damage = 10;
            attack2.GetComponent<BasicRanged>().maxDistance = 30;
            reload = 1;*/

        }
        else
        {
            reload -= Time.fixedDeltaTime;
        }
        //rb2d.MovePosition(rb2d.position + curMove * speed  * Time.fixedDeltaTime);
        transform.rotation = Quaternion.AngleAxis(facing, Vector3.forward);
        lastLocation.first = id;
        lastLocation.second = face;

    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  checkBlocked
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    bool - is there a path to the target
--
-- NOTES: Checks if there is a valid path to shoot at the target. If so, return true;
--
----------------------------------------------------------------------------------------------------------------------*/

    private bool checkBlocked(Vector2 attackSpot)
    {
        Vector2 move = attackSpot;
        Debug.Log(attackSpot.magnitude);
        float dist = attackSpot.magnitude;
        move.Normalize();
        var layerMask = (1 << 2);
        layerMask = ~layerMask;
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, attackSpot, dist * 1.1f, layerMask);
       // RaycastHit2D hit = Physics2D.Raycast(rb2d.position, attackSpot, dist * 0.8f);
        if (hit.collider != null)
        {

            if (hit.collider.tag.Equals("Player"))
            {
                return true;
            }
            // if(hit.collider.gameObject.name)
            //Debug.Log("Collision on shot" + hit.point + "Vector: " + attackSpot);

            return false;
        }
        return true;
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  createBullet
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    VOID
--
-- NOTES: Cretes a projectile based on the attack direction
--
----------------------------------------------------------------------------------------------------------------------*/

    private void createBullet(Vector2 attackDir)
    {
        reload = resetReload;
		Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
        attack.AddForce(attackDir * speed * 2.5f);
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = damage;
        attack.GetComponent<BasicRanged>().maxDistance = 30;
        
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  setFace
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    VOID
--
-- NOTES: Sets the facing angle of the turret 
--
----------------------------------------------------------------------------------------------------------------------*/

    private void setFace(Vector2 face)
    {
        float x, y;

        x = face.x - rb2d.position.x;
        y = face.y - rb2d.position.y;
        if (x == 0)
        {
            if (y > 0)
            {
                facing = Mathf.PI / 2;
            }
            else
            {
                facing = 3 * Mathf.PI / 2;
            }
        }
        else
        {
            if (x > 0)
            {
                facing = (float)(Mathf.PI * 2 + System.Math.Atan(y / x)) % (Mathf.PI * 2);

            }
            else
            {
                facing = (float)(Mathf.PI + System.Math.Atan(y / x));// % 360;
            }
        }
        facing = getDegree(facing);
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  getRoute
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    Vector2 - direction to move
--
-- NOTES: Sets the next direction for the AI to move in
--
----------------------------------------------------------------------------------------------------------------------*/

    Vector2 getRoute()
    {
        System.Random rand = new System.Random();
        float angle = (float)rand.NextDouble() * (2 * Mathf.PI);
        facing = angle;
        double yMod, xMod;
        xMod = System.Math.Cos(angle);
        yMod = System.Math.Sin(angle);
        Vector2 position = new Vector2((float)xMod, (float)yMod);
        return position;
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  getDegree
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    double - angle in degrees
-- 
-- NOTES: rad to degree conversion
--
----------------------------------------------------------------------------------------------------------------------*/

    public float getDegree(float angle)
    {
        return (float)(angle * 180 / System.Math.PI);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        route = false;


    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  getIntersection
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    Vector2 - the direction to attack in
--
-- NOTES: Calculates where to shoot for predictive shooting. If a target is running, this is called to calculate
-- where the turret needs to shoot for the bullet to intercept them
--
----------------------------------------------------------------------------------------------------------------------*/

    Vector2 getIntersection(Vector2 cur, Vector2 last, int spd)
    {
        Vector2 shoot;
        Vector2 target = new Vector2();
        Vector2 path = cur - last ;
        path.Normalize();
        path = path * spd;
        target.x = cur.x - rb2d.position.x;
        target.y = cur.y - rb2d.position.y;
        float a = Vector2.Dot(path, path) - (this.speed * this.speed);
        float b = 2 * Vector2.Dot(path, target);
        float c = Vector2.Dot(target, target);
        float quad1 = -b / (2 * a);
        float quad2 = (float)Mathf.Sqrt((b * b) - 4 * a * c) / (2 * a);
        float t1 = quad1 - quad2;
        float t2 = quad1 + quad2;
        float t;
        if(t1 > t2 && t2 > 0)
        {
            t = t2;
        }
        else
        {
            t = t1;
        }
        Vector2 aimSpot = cur + path * t;
        shoot.x = aimSpot.x - rb2d.position.x;
        shoot.y = aimSpot.y - rb2d.position.y;
        //shoot.Normalize();
        return shoot;

    }
}
