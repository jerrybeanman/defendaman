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
    public double reload = 9999999.0f;
    public double resetReload = 999;
    public int damage = 10;
    public Rigidbody2D bullet;
    public double swap;
    public int team = -2;
    public int aiID = 0;
    int teamSwap;
    double reloadSwap;
    // Use this for initialization
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
       	//bullet = (Rigidbody2D)Resources.Load("Prefabs/Bullet", typeof(Rigidbody2D));
		NetworkingManager.Subscribe(UpdateAI, DataType.AI, aiID);
        NetworkingManager.Subscribe(CreateProjectile, DataType.AIProjectile, aiID);
        rb2d = GetComponent<Rigidbody2D>();
        reload = 999999999f;
        resetReload = 999;
        gameObject.layer = 2;
        Debug.Log("Constructed");
    }

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

    void UpdateAI(JSONClass packet)
    {
    
        Debug.Log("Received packet: " + packet.ToString());
        xCoord = packet["x"].AsFloat;
        yCoord = packet["y"].AsFloat;
        angleFacing = packet["facing"].AsFloat;
        Vector2 newPos = new Vector2(xCoord, yCoord);

        transform.rotation = Quaternion.AngleAxis((float)angleFacing, Vector3.forward);
    }

    // Update is called once per frame
    void Update() {
        if (gameObject.GetComponent<Building>().placing)
            return;
        if (reload > 1000)
        {
            reload = reloadSwap;    
            resetReload = reloadSwap;            
        }
        
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
    private void createBullet(Vector2 attackDir)
    {
        reload = resetReload;
        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);
        attack.AddForce(attackDir * speed * 2.5f);
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = damage;
        attack.GetComponent<BasicRanged>().maxDistance = 30;
        
    }
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
    public float getDegree(float angle)
    {
        return (float)(angle * 180 / System.Math.PI);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        route = false;


    }
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
