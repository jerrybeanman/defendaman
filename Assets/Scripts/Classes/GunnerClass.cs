using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public enum SpecialCase { GunnerSpecial = 1 }

public class GunnerClass : RangedClass
{
    int[] distance = new int[2] { 12, 12 };
    int[] speed = new int[2] { 200, 300 };
    Rigidbody2D bullet;
    Rigidbody2D laser;
    Camera mainCamera;
    Camera visionCamera;
    Camera hiddenCamera;
    float zoomOut = 14;
    float zoomIn;
    bool inSpecial;
    bool fired;

    new void Start()
    {
        cooldowns = new float[2] { 0.2f, 5f };
        base.Start();

        _classStat.MaxHp = 150;
        _classStat.CurrentHp = _classStat.MaxHp;
        _classStat.MoveSpeed = 10;
        _classStat.AtkPower = 20;
        _classStat.Defense = 5;
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

        //Starting item kit
        if (playerID == GameData.MyPlayer.PlayerID)
        {
            Inventory.instance.AddItem(1);
            Inventory.instance.AddItem(5, 5);
            Inventory.instance.AddItem(6);
            Inventory.instance.AddItem(7);
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
        if (playerLoc == default(Vector2))
            playerLoc = dir;
        dir = ((Vector2)((Vector3)dir - transform.position)).normalized;
        base.specialAttack(dir, playerLoc);

        this.dir = dir;
        inSpecial = true;

        return cooldowns[1];
    }

    Vector2 dir;
    void Update()
    {
        if (playerID == GameData.MyPlayer.PlayerID)
        {
            if (inSpecial && Input.GetMouseButton(1))
            {
                if (mainCamera.orthographicSize < zoomOut)
                {
                    mainCamera.orthographicSize += .1f;
                    visionCamera.orthographicSize += .1f;
                    hiddenCamera.orthographicSize += .1f;
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
            }
            if (mainCamera.orthographicSize > zoomIn && !Input.GetMouseButton(1))
            {
                mainCamera.orthographicSize -= .2f;
                visionCamera.orthographicSize -= .2f;
                hiddenCamera.orthographicSize -= .2f;
                MapManager.cameraDistance = -mainCamera.orthographicSize;
            }
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
        laserAttack.damage = ClassStat.AtkPower * zoomRatio;
        laserAttack.maxDistance = (int)(distance[1] * zoomRatio);
        laserAttack.pierce = 10;

        var member = new List<Pair<string, string>>();
        member.Add(new Pair<string, string>("playerID", playerID.ToString()));
        EndAttackAnimation();
        CancelInvoke("EndAttackAnimation");
    }
    void fireFromServer(JSONClass packet)
    {
        if (packet["playerID"].AsInt == playerID && playerID != GameData.MyPlayer.PlayerID)
        {
            fire();
        }
    }
}