/*---------------------------------------------------------------------------------------
--  SOURCE FILE:    BaseClass.cs
--
--  PROGRAM:        Linux Game
--
--  FUNCTIONS:
--      void Start()
--      float doDamage(float damage, bool trueDamage = false)
--      void OnTriggerEnter2D(Collider2D other)
--      void receiveAttackFromServer(JSONClass playerData)
--      float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
--      float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
--      public class PlayerBaseStat
--          public void update_stats()
--      void StartAttackAnimation()
--      void EndAttackAnimation()
--      void UsePotion(int damage, int armour, int health, int speed, int duration)
--      IEnumerator Debuff(int damage, int armour, int speed, int duration)
--
--  DATE:           March 9, 2016
--
--  REVISIONS:      March 31, 2016
--                      Eunwon: Added attack sound effect 
--                  April 4, 2016
--                      Hank: Added Albert easter egg
--
--  DESIGNERS:      Hank Lo, Allen Tsang, Carson Roscoe
--
--  PROGRAMMER:     Hank Lo, Allen Tsang, Carson Roscoe, Eunwon Moon
--
--  NOTES:
--  This class is the grandparent class for all classes. Every class is a base class.
---------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;
using System;

public abstract class BaseClass : MonoBehaviour {
    //Cooldowns
    public float[] cooldowns { get; protected set; }

    /* Base stats that all classes share*/
    protected PlayerBaseStat _classStat;

    public int team;
    public int playerID;
    private int allyKingID;
    private int enemyKingID;
    
	protected HealthBar healthBar;

    public AudioSource au_attack;
    public AudioClip au_simple_attack;
    public AudioClip au_special_attack;
    public AudioClip au_special_l;
    public AudioClip au_failed_special;
    public AudioClip au_gunner_charge;

    public bool silenced = false;
    
    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: Start
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: March 9th, 2016: Created the start [Allen]
    --            March 15th, 2016: Hooked up with Networking Manager [Allen/Carson]
    --            March 25th, 2016: Attached with HUD [Carson]
    --            April 2nd, 2016: Added sound [Eunwon]
    --            April 2nd, 2016: Added Aman buff [Allen]
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe, Eunwon Moon
    --
    -- INTERFACE: void Start(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Start of scripts creation. Used to instantiate variables in our case.
    ---------------------------------------------------------------------------------------------------------------------*/
    protected void Start ()
    {
        allyKingID = GameData.AllyKingID;
        enemyKingID = GameData.EnemyKingID;

        NetworkingManager.Subscribe(receiveAttackFromServer, DataType.Trigger, playerID);

        if (playerID == GameData.MyPlayer.PlayerID)
        {
            HUD_Manager.instance.subSkill.CoolDown = cooldowns[0];
            HUD_Manager.instance.mainSkill.CoolDown = cooldowns[1];
            HUD_Manager.instance.playerProfile.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
            print("Our health:" + HUD_Manager.instance.playerProfile.Health.fillAmount);
        }

        if (playerID == allyKingID)
        {
            HUD_Manager.instance.allyKing.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
            print("Ally king:" + HUD_Manager.instance.allyKing.Health.fillAmount);
        }

        if (playerID == enemyKingID)
        {
            HUD_Manager.instance.enemyKing.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
            print("Enemy king:" + HUD_Manager.instance.enemyKing.Health.fillAmount);
        }

        if (playerID == allyKingID || playerID == enemyKingID)
        {
            gameObject.AddComponent<AmanSelfBuff>();
        }

        //add audio component
        au_attack = gameObject.AddComponent<AudioSource>();
        au_special_l = Resources.Load("Music/Weapons/laugh_cut") as AudioClip;
        au_failed_special = Resources.Load("Music/Weapons/failed_special_use") as AudioClip;
    }

    //TODO: Comment this
    public PlayerBaseStat ClassStat
	{
		get {
            if (_classStat == null) {
                _classStat = new PlayerBaseStat(playerID, healthBar);
            }
            return _classStat;
        }

		protected set {
            _classStat = value;
		}
	}

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: doDamage
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: March 21, 2016
    --              - Hank: Added defensive calculations
    --            April 4, 2016
    --              - Hank: Changed defensive calculation formula for balancing issues
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void doDamage(float damage, bool trueDamage = false)
    --          float damage: the damage to take
    --          bool trueDamage: whether or not the damage will be affected by defense - if true, defense does not
    --                              affect the damage taken.
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Used to cause damage to the player. Also does the defense calculations
    ---------------------------------------------------------------------------------------------------------------------*/
    public float doDamage(float damage, bool trueDamage = false)
    {
        float finaldamage = damage;

        if (!trueDamage)
        {
            float reduction = (100 / (ClassStat.Defense + 100));
            finaldamage = damage * reduction;
        }

        print("Final damage:" + finaldamage);
        GameManager.instance.PlayerTookDamage(playerID, ClassStat.CurrentHp - finaldamage, ClassStat);
        ClassStat.CurrentHp -= finaldamage;
        if(ClassStat.CurrentHp > ClassStat.MaxHp)
        {
            print("doDamage over max");
            ClassStat.CurrentHp -= Math.Abs(ClassStat.MaxHp-ClassStat.CurrentHp);
        }
        

        if (ClassStat.CurrentHp <= 0.0f && playerID == GameData.MyPlayer.PlayerID)
        {
            GameManager.instance.PlayerDied();
            Destroy(gameObject);
        }

        return finaldamage;
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: OnTriggerEnter2D
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: April 5, 2016
    --              - Allen: Added multihit prevention
    --
    -- DESIGNER: Hank Lo, Allen Tsang
    --
    -- PROGRAMMER: Hank Lo, Allen Tsang
    --
    -- INTERFACE: void OnTriggerEnter2D(Collider2D other)
    --              Collider2D other: The object we are colliding with
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Used to deal with collisions with players
    ---------------------------------------------------------------------------------------------------------------------*/
    void OnTriggerEnter2D(Collider2D other) {
        Trigger attack;
        if ((attack = other.gameObject.GetComponent<Trigger>()) != null)
        {
            if (attack.teamID == team || GameData.MyPlayer == null)
                return;

            //check for melee multihit, ignore if already in set
            if (attack is Melee && !((Melee)attack).targets.Add(gameObject))
                return;

            var damageTaken = 0f;
            if (playerID == GameData.MyPlayer.PlayerID)
                damageTaken = doDamage(attack.damage);

            if (GameData.MyPlayer == null || playerID != GameData.MyPlayer.PlayerID)
                return;

            var memersToSend = new List<Pair<string, string>>();
            memersToSend.Add(new Pair<string, string>("EnemyID", attack.playerID.ToString()));
            memersToSend.Add(new Pair<string, string>("NewHP", ClassStat.CurrentHp.ToString()));
            print(NetworkingManager.send_next_packet(DataType.Hit, GameData.MyPlayer.PlayerID, memersToSend, Protocol.UDP));
            
            return;
        }
	}

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: receiveAttackFromServer
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe
    --
    -- INTERFACE: void receiveAttackFromServer(JSONClass playerData)
    --              JSONClass playerData
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Used to parse the JSON info from the server and initiate attacks for that player
    ---------------------------------------------------------------------------------------------------------------------*/
    void receiveAttackFromServer(JSONClass playerData)
    {
        if (playerData["ID"].AsInt == GameData.MyPlayer.PlayerID)
            return;
        Vector2 directionOfAttack = new Vector2(playerData["DirectionX"].AsFloat, playerData["DirectionY"].AsFloat);
        Vector2 playerLoc = (Vector2)GameData.PlayerPosition[playerData["ID"].AsInt];
      
        switch (playerData["Attack"].AsInt)
        {
            case 0:
                basicAttack(directionOfAttack, playerLoc);
                //Regular attack
                break;
            case 1:
                specialAttack(directionOfAttack, playerLoc);
                //Regular special attack
                break;
            default:
                break;
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: basicAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: March 31, 2016
    --              Eunwon: add sound effect 
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe, Eunwon Moon
    --
    -- INTERFACE: float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    --              Vector2 dir: A Vector2 representing the direction of the attack
    --              Vector2 playerLoc: The player location represented by a Vector2
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Used to run the code that all classes will execute when doing a basic attack
    -- attack sound is playing depending on the distance between my player and that character. 
    ---------------------------------------------------------------------------------------------------------------------*/
    public virtual float basicAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        if (!GameData.PlayerPosition.ContainsKey(GameData.MyPlayer.PlayerID))
            return cooldowns[0];

        float distance = Vector2.Distance(playerLoc, GameData.PlayerPosition[GameData.MyPlayer.PlayerID]);

        if (playerLoc!= default(Vector2) && distance < 13)
        {
            au_attack.volume = (15 - distance) / 40;
            au_attack.PlayOneShot(au_simple_attack);
        }
        return cooldowns[0];
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- FUNCTION: specialAttack
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS:   March 16th, 2016
    --                  Carson/Allen: Rewrote for a new special ability
    --              March 31, 2016
    --                  Eunwon: Add attack sound effect
    --              April 4, 2016
    --                  Hank: Added Albert Easter egg
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe, Hank Lo
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe, Hank Lo, Eunwon Moon
    --
    -- INTERFACE: float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    --              Vector2 dir: A Vector2 representing the direction of the attack
    --              Vector2 playerLoc: The player location represented by a Vector2
    --
    -- RETURNS: a float representing the cooldown of the attack
    --
    -- NOTES:
    -- Used to run the code that all classes will execute when doing a special attack
    -- Attack sound effect is playing except gunner.
    ---------------------------------------------------------------------------------------------------------------------*/
    public virtual float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        float distance;

        if (GameData.PlayerPosition.ContainsKey(GameData.MyPlayer.PlayerID)){
            if (playerID == GameData.MyPlayer.PlayerID) {
                if (silenced) {
                    // play au_special_l
                    au_attack.volume = 1.0f;
                    System.Random rnd = new System.Random();
                    int playSpecial = rnd.Next(40, 45);
                    if (playSpecial == 42) {
                        au_attack.PlayOneShot(au_special_l);
                    } else {
                        au_attack.PlayOneShot(au_failed_special);
                    }
                }
            }
            return cooldowns[1];
        }
        if (playerLoc != default(Vector2) && 
            (distance = Vector2.Distance(playerLoc, GameData.PlayerPosition[GameData.MyPlayer.PlayerID])) < 13)
        {
            au_attack.volume = (15 - distance) / 40;
            au_attack.PlayOneShot(au_special_attack);
        }
        return cooldowns[1];
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- CLASS: PlayerBaseStat
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: 
    --
    -- FUNCTIONS:
    --      public void update_stats()
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe
    --
    -- NOTES:
    -- Inner class that holds all the players' base stats.  Includes getter and setter methods.  Setters also call methods
    -- to handle notification of the HUD and networking manager.
    ---------------------------------------------------------------------------------------------------------------------*/
    [System.Serializable]
	public class PlayerBaseStat
	{
        public PlayerBaseStat(int id, HealthBar healthBar)
        {
            _playerID = id;
			_healthBar = healthBar;
        }

		private HealthBar _healthBar;
        private int _playerID;
        private float _currentHp;
		public float CurrentHp {
            get
            {
                return _currentHp;
            }
            set {
                float damage;
                if ((damage = _currentHp - value) != 0)
                {
                    if (_playerID == GameData.AllyKingID)
                    {
                        HUD_Manager.instance.UpdateAllyKingHealth(-(damage / MaxHp));
                    }
                    else if (_playerID == GameData.EnemyKingID)
                    {
                        HUD_Manager.instance.UpdateEnemyKingHealth(-(damage / MaxHp));
                    }
                    _currentHp = (value > MaxHp) ? MaxHp : value;
                    _healthBar.UpdateHealth(MaxHp, CurrentHp);
                }
            }
        }
        private float _maxHP;
		public float MaxHp
        {
            get {
                return _maxHP;
            }
            set {
                _maxHP = value;
                _currentHp = value;
            }
        }
		public float MoveSpeed;
        private float _atkPower;
		public float AtkPower
        {
            get { return _atkPower; }
            set
            {
                _atkPower = value;
                update_stats();
            }
        }

        private float _defense;
        public float Defense
        {
            get { return _defense; }
            set
            {
                _defense = value;
                update_stats();
            }
        }

        /*---------------------------------------------------------------------------------------------------------------------
        -- METHOD: update_stats
        --
        -- DATE: March 16, 2016
        --
        -- REVISIONS:
        --
        -- DESIGNER: Allen Tsang, Carson Roscoe
        --
        -- PROGRAMMER: Allen Tsang, Carson Roscoe
        --
        -- INTERFACE: void update_stats()
        --
        -- RETURNS: void
        --
        -- NOTES:
        -- Sends stat updates to the networking manager to notify other players that your base stats have changed.
        ---------------------------------------------------------------------------------------------------------------------*/
        public void update_stats()
        {
            if (_playerID != GameData.MyPlayer.PlayerID)
                return;
            List<Pair<string, string>> memers = new List<Pair<string, string>>();
            memers.Add(new Pair<string, string>("AtkPower", AtkPower.ToString()));
            memers.Add(new Pair<string, string>("Defense", Defense.ToString()));
            //NetworkingManager.send_next_packet(DataType.StatUpdate, _playerID, memers, Protocol.TCP));
            Debug.Log(NetworkingManager.send_next_packet(DataType.StatUpdate, _playerID, memers, Protocol.TCP));
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: StartAttackAnimation
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Allen Tsang
    --
    -- PROGRAMMER: Allen Tsang
    --
    -- INTERFACE: void StartAttackAnimation(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Starts the attack animation
    ---------------------------------------------------------------------------------------------------------------------*/
    public void StartAttackAnimation() {
        gameObject.GetComponent<Animator>().SetBool("attacking", true);
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: EndAttackAnimation
    --
    -- DATE: March 9, 2016
    --
    -- REVISIONS: 
    --
    -- DESIGNER: Allen Tsang
    --
    -- PROGRAMMER: Allen Tsang
    --
    -- INTERFACE: void EndAttackAnimation(void)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Ends the attack animation
    ---------------------------------------------------------------------------------------------------------------------*/
    public void EndAttackAnimation() {
        gameObject.GetComponent<Animator>().SetBool("attacking", false);
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: UsePotion
    --
    -- DATE: March 16, 2016
    --
    -- REVISIONS:
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe
    --
    -- INTERFACE: void UsePotion(int damage, int armour, int health, int speed, int duration)
    --              int damage: damage value of the potion
    --              int armour: armor value of the potion
    --              int health: health value of the potion
    --              int speed: the speed value of the potion
    --              int duration: the duration of the potion in seconds
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- Used to consume a potion and apply buffs/hp recovery to the player who used a potion
    ---------------------------------------------------------------------------------------------------------------------*/
    public void UsePotion(int damage, int armour, int health, int speed, int duration)
    {
        if (duration == 0)
        {
            ClassStat.AtkPower += damage;
            ClassStat.Defense += armour;
            if (health != 0)
                doDamage(-health);
            ClassStat.CurrentHp += health;
            ClassStat.MoveSpeed += speed;
        } else
        {
            if (damage != 0)
                ClassStat.AtkPower += damage;
            if (armour != 0)
                ClassStat.Defense += armour;
            if (health != 0)
                doDamage(-health);
            ClassStat.CurrentHp += health;
            ClassStat.MoveSpeed += speed;
            Debug.Log(ClassStat.MoveSpeed);
            StartCoroutine(Debuff(damage, armour, speed, duration));
        }
    }

    /*---------------------------------------------------------------------------------------------------------------------
    -- METHOD: Debuff
    --
    -- DATE: March 16, 2016
    --
    -- REVISIONS:
    --
    -- DESIGNER: Allen Tsang, Carson Roscoe
    --
    -- PROGRAMMER: Allen Tsang, Carson Roscoe
    --
    -- INTERFACE: IEnumerator Debuf(int damage stat to undo
    --                              int armour stat to undo
    --                              int speed stat to dundo
    --                              int duration to wait before undoing the stat increase)
    --
    -- RETURNS: void
    --
    -- NOTES:
    -- After consuming a potion that has a duration, this will be invoked. Essentially it waits for the amount of time
    -- stated as the duration parameter and then undoes all the stats from the first three parameters.
    ---------------------------------------------------------------------------------------------------------------------*/
    IEnumerator Debuff(int damage, int armour, int speed, int duration) {
        yield return new WaitForSeconds(duration);
        if (damage != 0)
            ClassStat.AtkPower -= damage;
        if (armour != 0)
            ClassStat.Defense -= armour;
        ClassStat.MoveSpeed -= speed;
    }
}
