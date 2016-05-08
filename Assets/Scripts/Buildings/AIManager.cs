using UnityEngine;
using System.Collections;
/*------------------------------------------------------------------------------------------------------------------
Was cut from the project when we made the decision to keep all AI local to the client. 
    
    
    -- SOURCE FILE: AIManager.cs - Manager for AI. Takes in data from the server and manipulates the UI client side
-- PROGRAM: DefendAman
--
-- FUNCTIONS:
--		
--
-- DATE:		19/01/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Colin Bose
-- PROGRAMMER:  Colin Bose
--
-- NOTES:
-- Attach this to a turret to have a turret that shoots at enemies.
----------------------------------------------------------------------------------------------------------------------*/
public class AIManager : MonoBehaviour {
    public Transform AI;
    int numAi = 10;
    private float accuracy = 3.0f;
    int reactionRadius = 15;
    private GameObject[] aiArray = new GameObject[10];
	// Use this for initialization
	void Start () {
        return;
        int x = 0;
        Vector2 check = new Vector2();
        while (x < 10)
       {
           System.Random rnd = new System.Random();
           int xCoord = rnd.Next(1,99);
           int yCoord = rnd.Next(1,99);
           check.x = xCoord;
           check.y = yCoord;
           RaycastHit2D hit = Physics2D.Raycast(check, Vector2.up, 0.0001f);
           if (hit.collider != null)
           {
               continue;
           }
         
           aiArray[x] =  ((Transform)Instantiate(AI, new Vector3(xCoord,yCoord, -10), Quaternion.identity)).gameObject;
            AI temp = aiArray[x].GetComponent<AI>();
            temp.aiID = x;
            temp.xCoord = xCoord;
            temp.yCoord = yCoord;
            Debug.Log("ADding AI:" + temp.aiID);
            x++;
        }
    }

    // Update is called once per frame
    void Update () {
        return;
        //{DataType : 10, ID : aiID, TeamID : -1, ...}
        float newAngle;
        Vector3 attack;
        string packet = "[";
        foreach(var AIObject in aiArray)
        {
            var AI = AIObject.GetComponent<AI>();
            newAngle = faceClosest(AI);
            if(newAngle == -1)
            {
                AI.reload -= Time.fixedDeltaTime;
                continue;
            }
            if(AI.reload < 0)
            {

                string projectilePacket = getAttack(newAngle, AI);
                packet += projectilePacket + ",";
                AI.reload = 2;
            }
            else
            {
                AI.reload -= Time.fixedDeltaTime;
            }
            string localPacket = "{DataType : 9, ID : " + AI.aiID + ", x : " + AI.xCoord + ", y : " + AI.yCoord + ", facing : " + newAngle + "},";
            packet += localPacket;
        }

        packet.Remove(packet.Length-1);
        packet += "]";

        Debug.Log("Sending packet: " + packet);
        NetworkingManager.instance.update_data(packet);
       /* if (!route)
        {
            curMove = getRoute();
            route = true;
        }
        */
    }
    private string getAttack(float newAngle, AI A)
    {
        string local = "{DataType : 10, ID : " + A.aiID + ", startX : " + A.xCoord + ", startY : " + A.yCoord + ", ";
        Vector2 attack;
        float radAngle = (float)getRad(newAngle);
        attack.x = Mathf.Cos(radAngle);
        attack.y = Mathf.Sin(radAngle);
        local += "vecX : " + attack.x + ", vecY : " + attack.y + " }";
        return local;
    }
    private float faceClosest(AI A)
    {
        Vector3 vec = new Vector3();
        Vector3 face = new Vector3();
        float closest = 999;
        float facing;
        float xC, yC;
        xC = A.xCoord;
        yC = A.yCoord;
        float dist = 999;
        foreach (var playerData in GameData.LobbyData)
        {
            if (playerData.Value.TeamID == -1)
            {
                continue;
            }
           
            vec = GameData.PlayerPosition[playerData.Key];
            dist = Mathf.Sqrt(Mathf.Pow((xC - vec.x), 2) + Mathf.Pow((yC - vec.y), 2));
          
            if (dist < closest)
            {
                closest = dist;
                face = vec;
            }
        }
        if(closest > reactionRadius)
        {
            return -1;
        }
        float x, y;
        x = face.x - xC;
        y = face.y - yC;
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
        //A.angleFacing = getDegree(facing);
        return getDegree(facing);
      /*
        if (reload < 0)
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
        */

    }
    public float getDegree(float angle)
    {
        return (float)(angle * 180 / System.Math.PI);
    }
    public double getRad(double angle)
    {
        return (System.Math.PI / 180) * angle;
    }
}
