using UnityEngine;
using System.Collections;


public class Movement : MonoBehaviour
{
    enum movestyle { absolute, relative };
    public float moveSpeed = 5;
    Vector2 looking;
    private Rigidbody2D rb2d;
    string up, down, left, right;
    public float speed = 5;
    movestyle movestyles;
    float midX, midY;


    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        midX = Screen.width / 2;
        midY = Screen.height / 2;
        up = "w";
        down = "s";
        left = "a";
        right = "d";
        movestyles = movestyle.relative;


    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Equals))
            movestyles = movestyles == movestyle.absolute ? movestyle.relative : movestyle.absolute;

        if (movestyles == movestyle.absolute)
        {
            absMove();
        }
        else
        {
            relMove();
        }
        //Will need to send some info to server every update
        sendToServer(rb2d.position.x, rb2d.position.y);

    }
    void sendToServer(double x, double y)
    {
        //JSON OK
    }
    //called if local position does not match where the server thinks player is
    void localPosWrong(double x, double y)
    {
        Vector2 position = new Vector2((float)x, (float)y);
        //transform.position = position;
        rb2d.transform.position = position;
    }

    //absolute movement, just rotate and move
    void absMove()
    {
        rb2d.MovePosition(rb2d.position + new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * speed * Time.fixedDeltaTime);
        double looking = getInfo();
        transform.rotation = Quaternion.AngleAxis((float)looking, Vector3.forward);
    }
    //relative movemntment, move towards cursor
    void relMove()
    {
        double looking = getInfo();
        Vector2 moving = getMovement(looking);
        rb2d.MovePosition(rb2d.position + moving * speed * Time.fixedDeltaTime);
        transform.rotation = Quaternion.AngleAxis((float)looking, Vector3.forward);

    }
    //returns  the angle the player is facing IN DEGREES
    double getInfo()
    {
        double angleFacing;
        Vector2 mousePos = Input.mousePosition;
        angleFacing = calcAngle(mousePos);
        angleFacing = getDegree(angleFacing);
        return angleFacing;
    }
    //calculates the angle player is facing, IN RADIANS
    private double calcAngle(Vector2 mousePos)
    {
        double x, y;
        x = mousePos.x - midX;
        y = mousePos.y - midY;
        double modAngle;
        if (x > 0)
        {
            modAngle = (Mathf.PI * 2 + System.Math.Atan(y / x)) % (Mathf.PI * 2);

        }
        else
        {
            modAngle = (Mathf.PI + System.Math.Atan(y / x));// % 360;
        }
        return modAngle;
    }
    //convert rad to degree
    public double getDegree(double angle)
    {
        return angle * 180 / System.Math.PI;
    }
    //What to do on collision?
    void OnCollisionEnter2D(Collision2D collision)
    {

        // rb2d.velocity = new Vector2(0, 0);


    }
    //Get the vector based on the facing angle. This is for relative movement. Returns vector of movement direction
    Vector2 getMovement(double angleFacing)
    {
        double angleHorz = 0;
        bool hMoved = false;
        bool moveDown = false;
        bool vMoved = false;
        Vector2 ret = new Vector2();
        if (Input.GetKey(up))
        {
            angleFacing += 0;
            vMoved = true;
        }
        else if (Input.GetKey(down))
        {
            vMoved = true;
            moveDown = true;
            angleFacing += 180;
        }
        if (Input.GetKey(left))
        {
            angleHorz += 90;
            hMoved = true;
        }
        if (Input.GetKey(right))
        {
            angleHorz += -90;
            hMoved = true;
        }

        if (hMoved || vMoved)
        {
            if (hMoved && vMoved)
            {
                if (moveDown)
                {
                    ret = updateCoordinates(angleFacing - angleHorz / 2);
                }
                else
                {
                    ret = updateCoordinates((angleHorz + angleFacing + angleFacing) / 2.0f);
                }
            }
            else
            {
                if (hMoved && angleHorz == 0 && !vMoved)
                {
                    return Vector2.zero;
                }
                ret = updateCoordinates(angleHorz + angleFacing);
            }
        }
        return ret;
    }
    //degree to rad conversion
    public double getRad(double angle)
    {
        return (System.Math.PI / 180) * angle;
    }
    //Returns the vector based on facing angle + key input
    Vector2 updateCoordinates(double angle)
    {
        angle = getRad(angle);
        double yMod, xMod;
        xMod = System.Math.Cos(angle);
        yMod = System.Math.Sin(angle);
        Vector2 position = new Vector2((float)xMod, (float)yMod);

        return position;
    }

    void OnCollisonExit2D(Collision2D collision)
    {

    }

}