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

    public bool silenced = false;
    
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

    public float doDamage(float damage, bool trueDamage = false)
    {
        // hank: Added defensive calculation
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

    //Apr 5, added multihit prevention
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

    // hank
    public virtual float specialAttack(Vector2 dir, Vector2 playerLoc = default(Vector2))
    {
        float distance;

        if (GameData.PlayerPosition.ContainsKey(GameData.MyPlayer.PlayerID)){
            if (playerID == GameData.MyPlayer.PlayerID) {
                if (silenced) {
                    // play au_special_l
                    au_attack.volume = .8f;
                    System.Random rnd = new System.Random();
                    int playSpecial = rnd.Next(1, 50);
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

    public void StartAttackAnimation()
    {
        
        gameObject.GetComponent<Animator>().SetBool("attacking", true);
    }

    public void EndAttackAnimation()
    {
        gameObject.GetComponent<Animator>().SetBool("attacking", false);
    }

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

    IEnumerator Debuff(int damage, int armour, int speed, int duration)
    {
        yield return new WaitForSeconds(duration);
        if (damage != 0)
            ClassStat.AtkPower -= damage;
        if (armour != 0)
            ClassStat.Defense -= armour;
        ClassStat.MoveSpeed -= speed;
    }
}
