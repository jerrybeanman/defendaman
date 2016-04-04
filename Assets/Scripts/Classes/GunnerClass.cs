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
	float zoomOut = 11;
	float zoomIn;
	bool inSpecial;
	bool fired;
	Vector2 dir;
	
	// added by jerry
	public	float 		 slowPercentage = 1;	// Speed to slow by when in special attack mode. Stacks up.
	private Movement	 movement;				// Need to access Movement comopenent to change the player speed
	private DynamicLight FOVCone;				// Need to access vision cone to extend when in special attack mode
	private float		 BaseSpeed = 10;		// Stores the base move speed
	private DynamicLight FOVConeHidden;
	
	new void Start()
	{
		cooldowns = new float[2] { 0.2f, 10f };

        healthBar = transform.GetChild(0).gameObject.GetComponent<HealthBar>();
        _classStat = new PlayerBaseStat(playerID, healthBar);
        _classStat.MaxHp = 1100;
		_classStat.MoveSpeed = BaseSpeed;
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
        attack.AddForce(dir * speed[0]);//was newdir
        attack.GetComponent<BasicRanged>().playerID = playerID;
        attack.GetComponent<BasicRanged>().teamID = team;
        attack.GetComponent<BasicRanged>().damage = ClassStat.AtkPower;
        attack.GetComponent<BasicRanged>().maxDistance = distance[0];

        return cooldowns[0];
    }


    public override float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
    	if (gameObject.GetComponent<MagicDebuff>() == null) {
	        if (playerLoc == default(Vector2))
	            playerLoc = dir;
	        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
	        base.specialAttack(dir, playerLoc);

	        this.dir = dir;
	        inSpecial = true;
	    }

        return cooldowns[1];
    }
	
    // hank april 4, added check for magic debuff, added autoshot at max range
	void Update()
	{
		if (gameObject.GetComponent<MagicDebuff>() != null) {
			inSpecial = false;
			StartCoroutine(ZoomIn());
		}

		if (playerID == GameData.MyPlayer.PlayerID)
		{
			if (inSpecial && Input.GetMouseButton(1))
			{
				if (mainCamera.orthographicSize < zoomOut)
				{
					FOVCone.LightRadius++;
					FOVConeHidden.LightRadius++;
					FOVCone.RangeAngle -= 2.5f;
					FOVConeHidden.RangeAngle -= 2.5f;
					
					mainCamera.orthographicSize += .1f;
					visionCamera.orthographicSize += .1f;
					hiddenCamera.orthographicSize += .1f;

				} else {
					dir = (gameObject.transform.rotation * Vector3.right);
					inSpecial = false;
					fire();
					var member = new List<Pair<string, string>>();
					member.Add(new Pair<string, string>("playerID", playerID.ToString()));
					NetworkingManager.send_next_packet(DataType.SpecialCase, (int)SpecialCase.GunnerSpecial, member, Protocol.UDP);
					StartCoroutine(ZoomIn());
				}

				MapManager.cameraDistance = -mainCamera.orthographicSize;
			}
			
			if (inSpecial && !Input.GetMouseButton(1))
			{
				dir = (gameObject.transform.rotation * Vector3.right);
				inSpecial = false;
				fire();
				var member = new List<Pair<string, string>>();
				member.Add(new Pair<string, string>("playerID", playerID.ToString()));
				NetworkingManager.send_next_packet(DataType.SpecialCase, (int)SpecialCase.GunnerSpecial, member, Protocol.UDP);
				StartCoroutine(ZoomIn());
			}
		}
	}

	/*----------------------------------------------------------------------------
    --	Set speed back to normal, and zooms camera back in
    --
    --	Interface:  IEnumerator ZoomIn()
    --
    --	programmer: Jerry Jia, Carson Roscoe, Allen Tsang
    --	@return: number of seconds to wait before executing next instruction
	------------------------------------------------------------------------------*/
	IEnumerator ZoomIn()
	{

		// Wait a bit so we can see that 360 quickscope
		yield return new WaitForSeconds(1);

		// TODO:: Fix all these magic numbers after...
		while(mainCamera.orthographicSize > zoomIn)
		{
			// Zooms the vision cone back in and adjust angle back to original
			FOVCone.LightRadius -= 2;
			FOVConeHidden.LightRadius -= 2;
			FOVCone.RangeAngle += 5;
			FOVConeHidden.RangeAngle += 5;

			// Zooms camera back in
			mainCamera.orthographicSize -= .2f;
			visionCamera.orthographicSize -= .2f;
			hiddenCamera.orthographicSize -= .2f;
			MapManager.cameraDistance = -mainCamera.orthographicSize;
			yield return null;
		}
	}

	void fire()
    {
        var startPosition = new Vector3(transform.position.x + (dir.x * 1.25f), transform.position.y + (dir.y * 1.25f), -5);

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
}