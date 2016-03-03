using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
/*    float movespeed = 1f;
    PlayerClass player;

    int moving = 0;
    float midX, midY;
    double angleFacing;
    // Use this for initialization
    
    void Start()
    
    {
        player = new PlayerClass();
        midX = Screen.width / 2;
        midY = Screen.height / 2;
    }
    double getInfo()
    {
        Vector2 mousePos = Input.mousePosition;
        angleFacing = calcAngle(mousePos);
        angleFacing = getDegree(angleFacing);
        return angleFacing;
    }
    Vector2 getMovement()
    {
        double angleHorz = 0;
        bool hMoved = false;
        bool down = false;
        bool vMoved = false;
        Vector2 ret;
        if (Input.GetKey("w"))
        {
            angleFacing += 0;
            vMoved = true;
        }
        else if (Input.GetKey("s"))
        {
            vMoved = true;
            down = true;
            angleFacing += 180;
        }
        if (Input.GetKey("a"))
        {
            angleHorz += 90;
            hMoved = true;
        }
        if (Input.GetKey("d"))
        {
            angleHorz += -90;
            hMoved = true;
        }

        if (hMoved || vMoved)
        {
            moving = 0;

            if (hMoved && vMoved)
            {
                if (down)
                {
                    ret = updateCoordinates(angleFacing - angleHorz / 2);
                }
                else
                {
                     ret = updateCoordinates((angleHorz + angleVert + angleVert) / 2.0f);
                }
            }
            else
            {
                if (hMoved && angleHorz == 0 && !vMoved)
                {
                    return;
                }
                 ret =updateCoordinates(angleHorz + angleVert);
            }
        }


    }

    // Update is called once per frame
    void Update()
    {

        Vector2 mousePos = Input.mousePosition;
        double angleVert = calcAngle(mousePos);
        angleVert = getDegree(angleVert);

        transform.rotation = Quaternion.AngleAxis((float)angleVert, Vector3.forward);
        //angleVert = 90;
        double angleHorz = 0;
        bool hMoved = false;
        bool down = false;
        bool vMoved = false;
        if (Input.GetKey("w"))
        {
            angleVert += 0;
            vMoved = true;
        }
        else if (Input.GetKey("s"))
        {
            vMoved = true;
            down = true;
            angleVert += 180;
        }
        if (Input.GetKey("a"))
        {
            angleHorz += 90;
            hMoved = true;
        }
        if (Input.GetKey("d"))
        {
            angleHorz += -90;
            hMoved = true;
        }
        
        if (hMoved || vMoved)
        {
            moving = 0;
            
            if (hMoved && vMoved)
            {
                if (down)
                {
                    updateCoordinates(angleVert - angleHorz / 2);
                }
                else
                {
                    updateCoordinates((angleHorz + angleVert + angleVert) / 2.0f);
                }
            }
            else
            {
                if (hMoved && angleHorz == 0 && !vMoved)
                {
                    return;
                }
                updateCoordinates(angleHorz + angleVert);
            }
        }
       
       
    }
    void updateCoordinates(double angle)
    {
        angle = getRad(angle);
        double yMod, xMod;
        xMod = System.Math.Cos(angle) * movespeed;
        yMod = System.Math.Sin(angle) * movespeed;
        player.setX(xMod * movespeed);
        player.setY(yMod * movespeed);
        Vector2 position = new Vector2((float)player.getX(), (float)player.getY());
        transform.position = position;
    }


    public double getRad(double angle)
    {
        return (System.Math.PI / 180) * angle;
    }
    public double getDegree(double angle)
    {
        return angle * 180 / System.Math.PI;
    }
    private double calcAngle(Vector2 mousePos)
    {
        double x, y;
        
        x = mousePos.x - midX;
        y = mousePos.y - midY;
        double modAngle;
        if (x > 0)
        {
            modAngle = (Mathf.PI*2 + System.Math.Atan(y / x)) % (Mathf.PI*2);
                
        }
        else
        {
            modAngle = (Mathf.PI + System.Math.Atan(y / x));// % 360;
        }
        return modAngle;
    }
    */
}