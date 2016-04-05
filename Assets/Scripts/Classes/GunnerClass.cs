using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public enum SpecialCase { GunnerSpecial = 1 }

public class GunnerClass : RangedClass
{
	int[] distance = new int[2] { 12, 12 };
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

	// values for 
	public 	float 		 chargeTime 		= 1f;
	public  float 		 targetConeRadius 	= 25f;
	public  float 		 targetConeAngle  	= 25f;
 	public	float 		 targetZoomOutRange = 14f;
	public  float		 zoomInTime 		= 0.5f;

	private Movement	 movement;				// Need to access Movement comopenent to change the player speed
	private DynamicLight FOVCone;				// Need to access vision cone to extend when in special attack mode
	private DynamicLight FOVConeHidden;

	// keep track of starting speed
	private float startingOrthographicSize;
	private float startingConeRadius;
	private float startingRangeAngle;

	new void Start()
	{
		cooldowns = new float[2] { 0.2f, 10f };

        healthBar = transform.GetChild(0).gameObject.GetComponent<HealthBar>();
        _classStat = new PlayerBaseStat(playerID, healthBar);
        _classStat.MaxHp = 1100;
		_classStat.MoveSpeed = 10;
		_classStat.AtkPower = 40;
		_classStat.Defense = 30;

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
            Inventory.instance.AddItem(1);
			Inventory.instance.AddItem(5, 5);
			Inventory.instance.AddItem(6);
			Inventory.instance.AddItem(7);

			// Initialize Vision Cone and movement components
			FOVCone 		= transform.GetChild(1).gameObject.GetComponent<DynamicLight>();
			FOVConeHidden 	= transform.GetChild(3).gameObject.GetComponent<DynamicLight>();
			movement 		= gameObject.GetComponent<Movement>();

			startingOrthographicSize = mainCamera.orthographicSize;
			startingConeRadius 		 = FOVCone.LightRadius;
			startingRangeAngle 		 = FOVCone.RangeAngle;
		}

        //add gunboi attack sound
        au_simple_attack = Resources.Load("Music/Weapons/gunboi_gun_primary") as AudioClip;
        au_special_attack = Resources.Load("Music/Weapons/gunboi_gun_secondary") as AudioClip;
    }


    //attacks return time it takes to execute
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
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower;
        attack.GetComponent<BasicRanged>().maxDistance = distance[0];

        return cooldowns[0];
    }

    public override float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {

    	if (!silenced) {
	        if (playerLoc == default(Vector2))
	            playerLoc = dir;
	        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
            playerLoc = default(Vector2);
	        base.specialAttack(dir, playerLoc);


	        this.dir = dir;
	        inSpecial = true;
	    }

        return cooldowns[1];
    }
	
    // hank april 4, added check for magic debuff, added autoshot at max range
	void Update()
	{
		if (silenced) {
			inSpecial = false;
			StartCoroutine(ReleaseAttack());
		}

		if (playerID == GameData.MyPlayer.PlayerID)
		{
			if (inSpecial && Input.GetMouseButton(1))
			{
				// ugly check..update sucks lol
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

			// Set pooling radius to allow more pooling objects
			MapManager.cameraDistance = -mainCamera.orthographicSize;
			yield return new WaitForEndOfFrame ();
		}

		started = false;

		// elapsedTime has eached chargeTime, release attack
		if(inSpecial)
		{
			yield return StartCoroutine(ReleaseAttack());
		}
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
			FOVConeHidden.RangeAngle = Mathf.Lerp(currentConeAngle, targetConeAngle, t);
			
			// Set pooling radius to allow more pooling objects
			MapManager.cameraDistance = -mainCamera.orthographicSize;
			yield return new WaitForEndOfFrame ();
		}
		yield return null;
		
	}

	void SendLaserPacket()
	{
		var member = new List<Pair<string, string>>();
		member.Add(new Pair<string, string>("playerID", playerID.ToString()));
		NetworkingManager.send_next_packet(DataType.SpecialCase, (int)SpecialCase.GunnerSpecial, member, Protocol.UDP);
	}
	void fire()
    {
		dir = (gameObject.transform.rotation * Vector3.right);
        var startPosition = new Vector3(transform.position.x + (dir.x * 1.25f), transform.position.y + (dir.y * 1.25f), -5);
                        playspecialSound(playerID);
        Rigidbody2D attack = (Rigidbody2D)Instantiate(laser, startPosition, transform.rotation);
        attack.AddForce(dir * speed[0]);
        var laserAttack = attack.GetComponent<Laser>();
        laserAttack.playerID = playerID;
        laserAttack.teamID = team;
        var zoomRatio = (mainCamera.orthographicSize / (zoomIn * .8f));
        // Hank changed this for balance issues - charging damage with the new numbers won't work out
        laserAttack.damage = ClassStat.AtkPower * 1.5f;
        laserAttack.maxDistance = (int)(distance[1] * zoomRatio);
        laserAttack.pierce = 10;

        var member = new List<Pair<string, string>>();
        member.Add(new Pair<string, string>("playerID", playerID.ToString()));
        EndAttackAnimation();
        CancelInvoke("EndAttackAnimation");
        inSpecial = false;

    }

    void fireFromServer(JSONClass packet)
    {
        if (packet["playerID"].AsInt == playerID && playerID != GameData.MyPlayer.PlayerID)
        {
            fire();
        }
    }


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
}