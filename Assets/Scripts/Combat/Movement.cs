/*------------------------------------------------------------------------------------------------------------------
-- 
-- SOURCE FILE: Movement.cs - Character Movement
-- PROGRAM: DefendAman
--
-- FUNCTIONS:
--		void Start()
--      bool checkEnd(Vector2 vec, double distance)
--      public bool doBlink(float distance)
--      void Update() 
--      void sendToServer(double x, double y)
--      void localPosWrong(double x, double y)
--      void absMove()
        double getInfo()
        void relMove()
--      double getInfo()    
--      private double calcAngle(Vector2 mousePos)
--      private double getDistance(Vector2 mousePos)
--      public float getDegree(float angle)
--
-- DATE:		19/02/2016
-- REVISIONS:	(V1.0)
-- DESIGNER:	Colin Bose
-- PROGRAMMER:  Colin Bose
--
-- NOTES:
-- Attach this to a player to allow them movement. Provides two different movement systems: absolute and relative
----------------------------------------------------------------------------------------------------------------------*/using UnityEngine;
using System.Collections;


public class Movement : MonoBehaviour
{
    enum movestyle { absolute, relative };
    Vector2 looking;
    private Rigidbody2D rb2d;
    string up, down, left, right;
    public float speed;
    movestyle movestyles;
    float midX, midY;
    public BaseClass baseClass;
	Animator anim;
	 
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        midX = Screen.width / 2;
        midY = Screen.height / 2;
        up = "w";
        down = "s";
        left = "a";
        right = "d";
        movestyles = movestyle.absolute;
		anim = gameObject.GetComponent<Animator>();
		baseClass = gameObject.GetComponent<BaseClass>();
        GameData.PlayerPosition.Add(GameData.MyPlayer.PlayerID, transform.position);
    }
    //Checks if the end teleport point is valid, or if it is in a wall
    bool checkEnd(Vector2 vec, double distance)
    {

        RaycastHit2D hit = Physics2D.Raycast(rb2d.position + vec * (float)distance, -Vector2.up, 0.0001f);
        if (hit.collider != null)
        {
            return false;
        }
        return true;
    }
/*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  doBlink
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--  public bool doBlink(float distance)
                        float distance - max distance to blink
-- 
-- RETURNS:    bool - true = success blink, false = no
--
-- NOTES: Call this on the ninja class to blink
--
----------------------------------------------------------------------------------------------------------------------*/
    public bool doBlink(float distance) {
        Vector2 mousePos = Input.mousePosition;
        double angle = getInfo();
        double mouseDistance = getDistance(mousePos);
      
        if (mouseDistance < distance)
        {
            distance = (float)mouseDistance;
          
        }
        Debug.Log("Mouse distnace: " + mouseDistance + " Real Distance: " + distance);
        Vector2 vec = updateCoordinates(angle);
        var layerMask = (1 << 10);
        layerMask = ~layerMask;
        var secondMask = (1 << 8);
        secondMask |= (1 << 10);
        secondMask = ~secondMask;
        RaycastHit2D hit = Physics2D.Raycast(rb2d.position, vec, distance, layerMask);

        if (hit.collider != null && hit.collider.gameObject.tag == "Building" && hit.collider.gameObject.GetComponent<Building>().team != GameData.MyPlayer.TeamID)
        {
            rb2d.position = rb2d.position + vec * (hit.distance - 0.1f);
        }
        //rb2d.MovePosition(rb2d.position + vec * (hit.distance - 0.1f));
        else {
            if (checkEnd(vec, distance))
            {
                //rb2d.MovePosition(rb2d.position + vec * distance);
                rb2d.position = rb2d.position + vec * distance;
                //  (rb2d.position + vec * distance);

            }
            //Uncomment return false to not have half blinks -- blinks that take you up to a wall. 
            else
            {

                //return false;
                // var layerMask = (1 << 8);
                Debug.Log("Doing second cast");
                for (int i = 100; i >= 0; i -= 5)
                {
                    float x = distance * i / 100.0f;
                    Debug.Log(x);
                    if (checkEnd(vec, x))
                    {
                        rb2d.position = rb2d.position + vec * x;
                        Debug.Log("Doing half blink");
                        return true;

                    }
                }

            }
        }
        return true;
         
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
-- NOTES: The update function, called 60 times per sec
--
----------------------------------------------------------------------------------------------------------------------*/

    void Update()
    {
        speed = baseClass.ClassStat.MoveSpeed;
		if(speed == 0)
		{
			if(baseClass is GunnerClass)
				baseClass.ClassStat.MoveSpeed = 6;
			else			
			if(baseClass is NinjaClass)
				baseClass.ClassStat.MoveSpeed = 7;
			else
			if(baseClass is WizardClass)
				baseClass.ClassStat.MoveSpeed = 5;
		}
		if (Input.GetKeyDown(KeyCode.Equals) && !GameData.InputBlocked)
            movestyles = (movestyles == movestyle.absolute ? movestyle.relative : movestyle.absolute);

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
        GameData.PlayerPosition[GameData.MyPlayer.PlayerID] = transform.position;

        // animation trigger test
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            anim.SetBool("moving", true);
        }
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.D))
        {
            anim.SetBool("moving", false);
        }
    }
    void sendToServer(double x, double y)
    {
        //JSON OK
    }
    //called if local position does not match where the server thinks player is
/*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  localPosWrong
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    VOID
--
-- NOTES: Call to update the position of a character based off data sent from server
--
----------------------------------------------------------------------------------------------------------------------*/

    void localPosWrong(double x, double y)
    {
        Vector2 position = new Vector2((float)x, (float)y);
        //transform.position = position;
        rb2d.transform.position = position;
    }

    
 /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  absMove
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    void
--
-- NOTES: Move absolute - W = up, S down, etc. Not dependant on mouse position
--
----------------------------------------------------------------------------------------------------------------------*/

    void absMove()
    {
        Vector2 moving = getMovement(90);
        rb2d.MovePosition(rb2d.position + moving * speed * Time.deltaTime);

        //rb2d.MovePosition(rb2d.position + new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * speed * Time.fixedDeltaTime);
        double looking = getInfo();
        transform.rotation = Quaternion.AngleAxis((float)looking, Vector3.forward);
    }
    //relative movemntment, move towards cursor
/*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  relMove
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    VOID
--
-- NOTES: Call to do relative move - W = go towards mouse
--
----------------------------------------------------------------------------------------------------------------------*/

    void relMove()
    {
        double looking = getInfo();
        Vector2 moving = getMovement(looking);
        rb2d.MovePosition(rb2d.position + moving * speed * Time.deltaTime);
        transform.rotation = Quaternion.AngleAxis((float)looking, Vector3.forward);

    }
    /*------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:  getInfo
    -- DATE:	   10/04/16
    -- REVISIONS:  (V1.0)
    -- DESIGNER:   Colin Bose
    -- PROGRAMMER: Colin Bose
    --
    -- 
    -- RETURNS:    angle based off the character looking at the mouse
    --
    -- NOTES: Returns the angle to rotate the character
    --
    ----------------------------------------------------------------------------------------------------------------------*/

    double getInfo()
    {
        double angleFacing;
        Vector2 mousePos = Input.mousePosition;
        angleFacing = calcAngle(mousePos);
        angleFacing = getDegree(angleFacing);
        return angleFacing;
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  calcAngle
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    double - the angle the character is facing
--
-- NOTES: Calculates the angle to the mouse pointer
--
----------------------------------------------------------------------------------------------------------------------*/

    private double calcAngle(Vector2 mousePos)
    {
        double x, y;
        x = mousePos.x - midX;
        y = mousePos.y - midY;
        double modAngle;
        if(x == 0)
        {
            if(y > 0)
            {
                return Mathf.PI / 2;
            }
            else
            {
                return 3 * Mathf.PI / 2;
            }
        }
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
    private double getDistance(Vector2 mousePos)
    {
        float x, y;
        x = (Input.mousePosition.x);
        y = (Input.mousePosition.y);
        Vector3 mouseposition = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 0));
        print(mouseposition);
        x = mouseposition.x - rb2d.position.x;
        y = mouseposition.y - rb2d.position.y;
        return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
    }
    //convert rad to degree
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  getDegree
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    double - the angle in degrees
--
-- NOTES: convert rad to degree
--
----------------------------------------------------------------------------------------------------------------------*/

    public double getDegree(double angle)
    {
        return angle * 180 / System.Math.PI;
    }
    //What to do on collision
    void OnCollisionEnter2D(Collision2D collision)
    {

        // rb2d.velocity = new Vector2(0, 0);


    }
 /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  getMovement
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS: Vector2 - the vector to move in
--
-- NOTES: Call, passing the facing angle, to get the vector that the character will move in
--
----------------------------------------------------------------------------------------------------------------------*/

    Vector2 getMovement(double angleFacing)
    {
        double angleHorz = 0;
        bool hMoved = false;
        bool moveDown = false;
        bool vMoved = false;
        Vector2 ret = new Vector2();
        if (Input.GetKey(up) && !GameData.InputBlocked)
        {
            angleFacing += 0;
            vMoved = true;
        }
        else if (Input.GetKey(down) && !GameData.InputBlocked)
        {
            vMoved = true;
            moveDown = true;
            angleFacing += 180;
        }
		if (Input.GetKey(left) && !GameData.InputBlocked)
        {
            angleHorz += 90;
            hMoved = true;
        }
		if (Input.GetKey(right) && !GameData.InputBlocked)
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
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  getRad
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    double - angle in radians
--
-- NOTES: convert degrees to rad
--
----------------------------------------------------------------------------------------------------------------------*/

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
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  setAbs
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    void
--
-- NOTES: Sets movement styel to absolute
--
----------------------------------------------------------------------------------------------------------------------*/

    public void setAbs()
    {
        movestyles = movestyle.absolute;
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  setRel
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS: VOID    
--
-- NOTES: sets movement to relative
--
----------------------------------------------------------------------------------------------------------------------*/

    public void setRel()
    {
        movestyles = movestyle.relative;
    }
    /*------------------------------------------------------------------------------------------------------------------
-- FUNCTION:  getInputType
-- DATE:	   10/04/16
-- REVISIONS:  (V1.0)
-- DESIGNER:   Colin Bose
-- PROGRAMMER: Colin Bose
--
-- 
-- RETURNS:    int - the movement style
--
-- NOTES: Call to look up movement style
--
----------------------------------------------------------------------------------------------------------------------*/

    public int getInputType()
    {
        if (movestyles == movestyle.absolute)
            return 1;
        else
            return 0;
    }

}