/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    GunnerClass.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start(void)
--      override float basicAttack(Vector2 dir)
--      override float specialAttack(Vector2 dir)
-- 		void Update(void)
--      void SendLaserPacket()
--      void fire()
--      void fireFromServer(JSONClass packet)
--      void playspecialSound(int)
--      void playspecialCharge(int)
--
--  DATE:           March 9, 2016
--
--  REVISIONS:      April 1: 
--                      Eunwon : add sound function only for gunner
--                  April 4th: Hank Lo
--                      - Numbers balancing
--
--  DESIGNERS:      Hank Lo, Allen Tsang, Carson Roscoe
--
--  PROGRAMMER:     Hank Lo, Allen Tsang, Jerry Jia, Eunwon Moon, Carson Roscoe
--
--  NOTES:
--  This class contains the logic that relates to the Gunner Class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public enum SpecialCase { GunnerSpecial = 1 }

public class GunnerClass : RangedClass
{
	int[] distance = new int[2] { 13, 13 };
	int[] speed = new int[2] { 300, 400 };
	Rigidbody2D bullet;
	Rigidbody2D laser;
	Camera mainCamera;
	Camera visionCamera;
	Camera hiddenCamera;
	float zoomIn;
	bool inSpecial;
	bool fired;
	Vector2 dir;
	
	//---added by jerry---//

	// values for gunner special attack
	public  float 		 holdTime			= 2.5f;
	public 	float 		 chargeTime 		= 1.5f;
	public  float 		 targetConeRadius 	= 28f;
	public  float 		 targetConeAngle  	= 20f;
 	public	float 		 targetZoomOutRange = 14f;
	public  float		 zoomInTime 		= 0.5f;
	public  float 		 maxRatio			= 8f;

	// hank
	public	float 		 endingAtkRatio;
    public  float        startingAtkRatio   = 0.5f;


	private Movement	 movement;				// Need to access Movement comopenent to change the player speed
	public DynamicLight FOVCone;				// Need to access vision cone to extend when in special attack mode
	public DynamicLight FOVConeHidden;

	// keep track of starting speed
	private float startingOrthographicSize;
	private float startingConeRadius;
	private float startingRangeAngle;

	/*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Start
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: None
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void Start(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Function that's called when the script is first executed - it initializes all required values
    ---------------------------------------------------------------------------------------------------------------------*/
	new void Start()
	{
		cooldowns = new float[2] { 0.25f, 8f };

        healthBar = transform.GetChild(0).gameObject.GetComponent<HealthBar>();
        _classStat = new PlayerBaseStat(playerID, healthBar);
        _classStat.MaxHp = 1100;
		_classStat.MoveSpeed = 6;
		_classStat.AtkPower = 100;
		_classStat.Defense = 25;

        base.Start();

        inSpecial = false;
		fired = false;
		
		bullet = (Rigidbody2D)Resources.Load("Prefabs/SmallBullet", typeof(Rigidbody2D));
		laser = (Rigidbody2D)Resources.Load("Prefabs/Laser", typeof(Rigidbody2D));
		
		var controller = Resources.Load("Controllers/gunboi") as RuntimeAnimatorController;
		gameObject.GetComponent<Animator>().runtimeAnimatorController = controller;
		mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		zoomIn = mainCamera.orthographicSize;
		visionCamera = GameObject.Find("Camera FOV").GetComponent<Camera>();
		hiddenCamera = GameObject.Find("Camera Enemies").GetComponent<Camera>();
		NetworkingManager.Subscribe(fireFromServer, DataType.SpecialCase, (int)SpecialCase.GunnerSpecial);

        //Player specific initialization
        if (playerID == GameData.MyPlayer.PlayerID)
		{
            //Starting item kit
            Inventory.instance.AddItem(12);

			// No longer needed - reference is given to this class when DynamicLight objects are created
			// Initialize Vision Cone and movement components
			//FOVCone 		= transform.GetChild(1).gameObject.GetComponent<DynamicLight>();
			//FOVConeHidden 	= transform.GetChild(3).gameObject.GetComponent<DynamicLight>();
			movement 		= gameObject.GetComponent<Movement>();

			startingOrthographicSize = mainCamera.orthographicSize;
			startingConeRadius 		 = FOVCone.LightRadius;
			startingRangeAngle 		 = FOVCone.RangeAngle;
		}

        //add gunboi attack sound
        au_simple_attack = Resources.Load("Music/Weapons/gunboi_gun_primary") as AudioClip;
        au_special_attack = Resources.Load("Music/Weapons/gunboi_gun_secondary") as AudioClip;
        au_gunner_charge = Resources.Load("Music/Weapons/gunboi_gun_charge") as AudioClip;
    }


    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: basicAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: 
    -- 			April 6, 2016 Hank: Added .2 AD Ratio
    -- 			April 7, 2016 Hank: Changed ratio to .25
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: float basicAttack(Vector2 dir)
    --              dir: a vector2 object which shows the direction of the attack
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Function that's called when the gunner uses the left click attack
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2)) 
    {
        if (playerLoc == default(Vector2))
            playerLoc = dir;
        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
        if (inSpecial)
            return 0f;
        base.basicAttack(dir, playerLoc);
        var startPosition = new Vector3(transform.position.x + (dir.x * 1.25f), transform.position.y + (dir.y * 1.25f), -5);

        Rigidbody2D attack = (Rigidbody2D)Instantiate(bullet, startPosition, transform.rotation);
        attack.AddForce(dir * speed[0]); //was newdir
        attack.GetComponent<BasicRanged>().playerID = playerID;
        attack.GetComponent<BasicRanged>().teamID = team;
        // hank april 6th added .2 ad ratio
        // april 7h, changed ratio to .25
        attack.GetComponent<BasicRanged>().damage = (float) (ClassStat.AtkPower * .25);
        attack.GetComponent<BasicRanged>().maxDistance = distance[0];

        return cooldowns[0];
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: specialAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS:
    --      - April 4, 2016: Added check for silence for magic circle - Hank
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang, Jerry Jia
    --
    -- INTERFACE: float specialAttack(Vector2 dir)
    --              dir: a vector2 object which shows the direction of the attack
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Function that's called when the Gunner uses the right click special attack (Charging laser)
    ---------------------------------------------------------------------------------------------------------------------*/
    public override float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
	    if (playerLoc == default(Vector2))
	        playerLoc = dir;
	    dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
        playerLoc = default(Vector2);
	    base.specialAttack(dir, playerLoc);
        playspecialCharge(playerID);

        if (!silenced) {
	        this.dir = dir;
	        inSpecial = true;
	    }
        return cooldowns[1];
    }
	
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Update
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS:
    --      - April 4, 2016: Added check for silence for magic circle - Hank
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang, Jerry Jia
    --
    -- INTERFACE: float specialAttack(Vector2 dir)
    --              dir: a vector2 object which shows the direction of the attack
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Function that's called every frame. We check in here if the user is doing the special attack, or if he is silenced
    ---------------------------------------------------------------------------------------------------------------------*/
	void Update()
	{
		if (silenced && inSpecial) {
			inSpecial = false;
			StartCoroutine(ReleaseAttack());
		}

		if (playerID == GameData.MyPlayer.PlayerID)
		{
			if (inSpecial && Input.GetMouseButton(1))
			{
				if(!started)
					StartCoroutine(ChargeAttack());
			}
			if (inSpecial && !Input.GetMouseButton(1))
			{
				StartCoroutine(ReleaseAttack());
			}
		}
	}

	private bool started = false;

	/*----------------------------------------------------------------------------
    --	Build up laser attack, zooms out camera and adjust vision cone values
    --  by linear interpolation. 
    --
    --	Interface:  IEnumerator ZoomIn()
    --
    --	programmer: Jerry Jia
    --	@return: number of seconds to wait before executing next instruction
	------------------------------------------------------------------------------*/
	IEnumerator ChargeAttack()
	{
		started = true;
		float elapsedTime = 0;

		// Show charge bar
		HUD_Manager.instance.chargeBar.Holder.SetActive(true);

		startingAtkRatio = 0.1f;
		while(inSpecial && elapsedTime < chargeTime)
		{
			elapsedTime += Time.deltaTime;

			// linear interpolation value
			float t = elapsedTime / chargeTime;

			// interpolate camera size, which zooms out
			mainCamera.orthographicSize = Mathf.Lerp(startingOrthographicSize, targetZoomOutRange, t);
			visionCamera.orthographicSize = Mathf.Lerp(startingOrthographicSize, targetZoomOutRange, t);
			hiddenCamera.orthographicSize = Mathf.Lerp(startingOrthographicSize, targetZoomOutRange, t);

			// Interpolate vision cone radius, which expands range
			FOVCone.LightRadius = Mathf.Lerp(startingConeRadius, targetConeRadius, t);
			FOVConeHidden.LightRadius = Mathf.Lerp(startingConeRadius, targetConeRadius, t);

			// Interpolate vision cone angle, which narrows angle
			FOVCone.RangeAngle  = Mathf.Lerp(startingRangeAngle, targetConeAngle, t);
			FOVConeHidden.RangeAngle = Mathf.Lerp(startingRangeAngle, targetConeAngle, t);

			HUD_Manager.instance.chargeBar.Bar.fillAmount = Mathf.Lerp(0, 1, t);

			// Set pooling radius to allow more pooling objects
			MapManager.cameraDistance = -mainCamera.orthographicSize;
			
			endingAtkRatio = Mathf.Lerp(startingAtkRatio, maxRatio, t);
			yield return new WaitForEndOfFrame ();
		}
        
		if(inSpecial)
		{
			HUD_Manager.instance.chargeBar.Holder.GetComponent<Animator>().SetTrigger("play");
		}
		// Reset value to be used again
		elapsedTime = 0;
		while(inSpecial && elapsedTime < holdTime)
		{
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}

		// elapsedTime has eached chargeTime, release attack
		if(inSpecial)
		{
			yield return StartCoroutine(ReleaseAttack());
		}
		started = false;
		yield return null;
	}

	/*----------------------------------------------------------------------------
    --	Releases laser attack, sets camera and vision cones value back to normal
    --
    --	Interface:  IEnumerator ZoomIn()
    --
    --	programmer: Jerry Jia
    --	@return: number of seconds to wait before executing next instruction
	------------------------------------------------------------------------------*/
	IEnumerator ReleaseAttack()
	{
		fire();

		HUD_Manager.instance.chargeBar.Holder.GetComponent<Animator>().Stop();

		inSpecial = false;

		// Send packet to indicate fire
		SendLaserPacket();

		// Wait a bit so we can see the beautiful quickscope
		yield return new WaitForSeconds(1);

		float elapsedTime = 0;

		// Retrieve current values in special attack mode 
		float currentZoomOutRange 	= mainCamera.orthographicSize;
		float currentConeRadius 	= FOVCone.LightRadius;
		float currentConeAngle		= FOVCone.RangeAngle;
		float currentFillAmount 	= HUD_Manager.instance.chargeBar.Bar.fillAmount;
		while(elapsedTime < zoomInTime)
		{
			elapsedTime += Time.deltaTime;
			
			// linear interpolation value
			float t = elapsedTime / zoomInTime;
			
			// interpolate camera size, which zooms in
			mainCamera.orthographicSize = Mathf.Lerp(currentZoomOutRange, startingOrthographicSize, t);
			visionCamera.orthographicSize = Mathf.Lerp(currentZoomOutRange, startingOrthographicSize, t);
			hiddenCamera.orthographicSize = Mathf.Lerp(currentZoomOutRange, startingOrthographicSize, t);
			
			// Interpolate vision cone radius, which shrinks range
			FOVCone.LightRadius = Mathf.Lerp(currentConeRadius, startingConeRadius, t);
			FOVConeHidden.LightRadius = Mathf.Lerp(currentConeRadius, startingConeRadius, t);
			
			// Interpolate vision cone angle, which narrows angle
			FOVCone.RangeAngle  = Mathf.Lerp(currentConeAngle, startingRangeAngle, t);
			FOVConeHidden.RangeAngle = Mathf.Lerp(currentConeAngle, startingRangeAngle, t);

			HUD_Manager.instance.chargeBar.Bar.fillAmount = Mathf.Lerp(currentFillAmount, 0, t);
			// Set pooling radius to allow more pooling objects
			MapManager.cameraDistance = -mainCamera.orthographicSize;
			yield return new WaitForEndOfFrame ();
		}

		HUD_Manager.instance.chargeBar.Holder.SetActive(false);
		yield return null;
		
	}

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    SendLaserPacket
    --
    -- DATE:        March 16, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER:    Carson Roscoe
    --
    -- PROGRAMMER:  Carson Roscoe
    --
    -- INTERFACE:   void SendLaserPacket()
    --
    -- RETURNS:     void
    --
    -- NOTES:
    --  Sends a packet to the networking manager to tell it that you have fired a gunner special shot.
    ---------------------------------------------------------------------------------------------------------------------*/
    void SendLaserPacket()
	{
		var member = new List<Pair<string, string>>();
		member.Add(new Pair<string, string>("playerID", playerID.ToString()));
		NetworkingManager.send_next_packet(DataType.SpecialCase, (int)SpecialCase.GunnerSpecial, member, Protocol.UDP);
	}

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    fire
    --
    -- DATE:        March 16, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER:    Carson Roscoe
    --
    -- PROGRAMMER:  Carson Roscoe
    --
    -- INTERFACE:   void fire()
    --
    -- RETURNS:     void
    --
    -- NOTES:
    --  Called when a player releases the gunner special attack.
    ---------------------------------------------------------------------------------------------------------------------*/
    void fire()
    {
		dir = (gameObject.transform.rotation * Vector3.right);
        var startPosition = new Vector3(transform.position.x + (dir.x * 1.25f), transform.position.y + (dir.y * 1.25f), -5);
            au_attack.Stop();
            playspecialSound(playerID);
            
        Rigidbody2D attack = (Rigidbody2D)Instantiate(laser, startPosition, transform.rotation);
        attack.AddForce(dir * speed[0]);
        var laserAttack = attack.GetComponent<Laser>();
        laserAttack.playerID = playerID;
        laserAttack.teamID = team;
        var zoomRatio = (mainCamera.orthographicSize / (zoomIn * .8f));
        laserAttack.damage = ClassStat.AtkPower * endingAtkRatio;
        laserAttack.maxDistance = (int)(distance[1] * zoomRatio);
        laserAttack.pierce = 10;

        var member = new List<Pair<string, string>>();
        member.Add(new Pair<string, string>("playerID", playerID.ToString()));
        EndAttackAnimation();
        CancelInvoke("EndAttackAnimation");
        inSpecial = false;

    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:    fireFromServer
    --
    -- DATE:        March 16, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER:    Carson Roscoe
    --
    -- PROGRAMMER:  Carson Roscoe
    --
    -- INTERFACE:   void fireFromServer(JSONClass packet)
    --              JSONClass packet: packet received from the networking manager containing the source player's info
    --
    -- RETURNS: void
    --
    -- NOTES:
    --  Called when receiving a packet that another player on the server has fired a special shot.
    ---------------------------------------------------------------------------------------------------------------------*/
    void fireFromServer(JSONClass packet)
    {
        if (packet["playerID"].AsInt == playerID && playerID != GameData.MyPlayer.PlayerID)
        {
            fire();
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:  playspecialSound
    --
    -- DATE: April 1, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER:    Eunwon Moon
    --
    -- PROGRAMMER:  Eunwon Moon
    --
    -- INTERFACE:   void playspecialSound(int PlayerID)
    --              PlayerID : who shoot the special skill
    --
    -- RETURNS: void
    --
    -- NOTES:
    --  Play the shooting sound when laser is shooting.
    --  volume will be changing based on the distance between player who shoot and my character.
    ---------------------------------------------------------------------------------------------------------------------*/
    void playspecialSound(int PlayerID)
    {
        Vector2 playerLoc = (Vector2)GameData.PlayerPosition[PlayerID];
        float distance = Vector2.Distance(playerLoc, GameData.PlayerPosition[GameData.MyPlayer.PlayerID]);
        if (distance < 13)
        {
            au_attack.volume = (15 - distance) / 40;
            au_attack.PlayOneShot(au_special_attack);
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION:  playspecialCharge
    --
    -- DATE:        April 1, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER:    Eunwon Moon
    --
    -- PROGRAMMER:  Eunwon Moon
    --
    -- INTERFACE: void playspecialCharge(int PlayerID)
    --              PlayerID : who shoot the special skill
    --
    -- RETURNS: void
    --
    -- NOTES:
    --  Play the shooting sound while charge the laser gun before shooting (release mouse button)
    --  volume will be changing based on the distance between player who shoot and my character.
    ---------------------------------------------------------------------------------------------------------------------*/
    void playspecialCharge(int PlayerID)
    {
        Vector2 playerLoc = (Vector2)GameData.PlayerPosition[PlayerID];
        float distance = Vector2.Distance(playerLoc, GameData.PlayerPosition[GameData.MyPlayer.PlayerID]);
        if (distance < 13)
        {
            au_attack.volume = (15 - distance) / 40;
            au_attack.PlayOneShot(au_gunner_charge);
        }
    }
}