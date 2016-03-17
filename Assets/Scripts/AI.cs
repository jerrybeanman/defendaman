using UnityEngine;
using System.Collections;
using SimpleJSON;

public class AI : MonoBehaviour {
    bool route = false;
    float facing;
    float accuracy = 3.5f;
    public float angleFacing;
    
    Vector2 curMove;
    public float xCoord, yCoord;
    private Rigidbody2D rb2d;
    private int speed = 5;
    public double reload = 2.0f;
    Rigidbody2D bullet = (Rigidbody2D)Resources.Load("Prefabs/Bullet", typeof(Rigidbody2D));
    public int team = -2;
    public int aiID = 0;
    // Use this for initialization
    void Start()
    {
        NetworkingManager.Subscribe(UpdateAI, DataType.AI, aiID);
        NetworkingManager.Subscribe(CreateProjectile, DataType.AIProjectile, aiID);
        rb2d = GetComponent<Rigidbody2D>();

        

    }

    void CreateProjectile(JSONClass packet)
    {
        //I created a projectile
        Vector2 attack;
        attack.x = packet["vecX"].AsFloat;
        attack.y = packet["vecY"].AsFloat;
       
        
       

        Rigidbody2D attack2 = (Rigidbody2D)Instantiate(bullet, transform.position, transform.rotation);

        attack2.AddForce(attack * 100);
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
    void Update () {
       Vector3 vec = new Vector3();
        Vector3 face = new Vector3();
        float closest = 999;
        float facing;
        if (!route)
        {
            curMove = getRoute();
            route = true;
        }
        foreach (var playerData in GameData.LobbyData)
        {
            float dist;
            vec = GameData.PlayerPosition[playerData.Key];
            dist = Mathf.Sqrt(Mathf.Pow((rb2d.position.x - vec.x), 2) + Mathf.Pow((rb2d.position.y - vec.y), 2));
            //Debug.Log("Player Position: " + vec.x + " " + vec.y + "Distance: " + dist + "Current Closest: " + closest );


            if (dist < closest)
            {
                closest = dist;
                face = vec;
            }
        }
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
                facing =  3 * Mathf.PI / 2;
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
        //facing = Mathf.Tan(xVal / yVal);
      //  Debug.Log("Your position:" + rb2d.position.x + " " + rb2d.position.y);
        //Debug.Log("Closest player position:" + face.x + " " + face.y);
        //Debug.Log(facing);
        if(reload < 0)
        {
            Vector2 attack = new Vector2();
            attack.x = x;
            attack.y = y;
            //attack.Normalize();
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
            attack.Normalize();
            Rigidbody2D attack2 = (Rigidbody2D)Instantiate(bullet, transform.position + (Vector3)attack * 0.3f, transform.rotation);
            attack2.AddForce(attack * 25);
            Debug.Log(attack);
            attack2.GetComponent<BasicRanged>().teamID = team;
            attack2.GetComponent<BasicRanged>().damage = 10;
            attack2.GetComponent<BasicRanged>().maxDistance = 10;
            reload = 1;
        }
        else
        {
            reload -= Time.fixedDeltaTime;
        }
        //rb2d.MovePosition(rb2d.position + curMove * speed  * Time.fixedDeltaTime);
        transform.rotation = Quaternion.AngleAxis((float)facing, Vector3.forward);
        
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
}
