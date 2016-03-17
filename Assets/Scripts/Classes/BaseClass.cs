using UnityEngine;
using System.Collections;
using SimpleJSON;
using System.Collections.Generic;

public abstract class BaseClass : MonoBehaviour {
    //Cooldowns
    public float[] cooldowns { get; protected set; }

	/* Name of the class. Ex: "Archer, warrior.." */
	protected string _className;

    /* Short description of the class.*/
    protected string _classDescription;

    /* Base stats that all classes share*/
    protected PlayerBaseStat _classStat = new PlayerBaseStat();

    public int team;
    public int playerID;
    private int yourPlayerID;
    private int allyKingID;
    private int enemyKingID;

    protected void Start ()
    {
        var networkingManager = GameObject.Find("GameManager").GetComponent<NetworkingManager>();
        yourPlayerID = GameManager.instance.player.GetComponent<BaseClass>().playerID;
        allyKingID = GameData.AllyKingID;
        enemyKingID = GameData.EnemyKingID;

        NetworkingManager.Subscribe(receiveAttackFromServer, DataType.Trigger, playerID);

        if (playerID == yourPlayerID)
        {
            HUD_Manager.instance.subSkill.CoolDown = cooldowns[0];
            HUD_Manager.instance.mainSkill.CoolDown = cooldowns[1];
            HUD_Manager.instance.playerProfile.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
            if (playerID == allyKingID)
                HUD_Manager.instance.allyKing.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
            if (playerID == enemyKingID)
                HUD_Manager.instance.enemyKing.Health.fillAmount = ClassStat.CurrentHp / ClassStat.MaxHp;
        }
    }
	
	public string ClassName
	{
		get { return this._className; }
		set { this._className = value;}
	}

	public string ClassDescription
	{
		get { return this._classDescription; }
		set { this._classDescription = value;}
	}

	public PlayerBaseStat ClassStat
	{
		get
        {
            if (this._classStat == null)
            {
                Debug.Log("Classstat was not set");
                this._classStat = new PlayerBaseStat();
            }
            return this._classStat;
        }

		protected set
		{
			this._classStat.CurrentHp = value.CurrentHp;
			this._classStat.MaxHp = value.MaxHp;
			this._classStat.MoveSpeed = value.MoveSpeed;
			this._classStat.AtkPower = value.AtkPower;
            this._classStat.Defense = value.Defense;
		}
	}

    public float doDamage(float damage, bool trueDamage = false)
    {
        // hank: Added defensive calculation
        float finaldamage = damage;

        if (!trueDamage)
        {
            float reduction = (30 / (ClassStat.Defense + 30));
            finaldamage = damage * reduction;
        }
        
        ClassStat.CurrentHp -= finaldamage;
        if(ClassStat.CurrentHp > ClassStat.MaxHp)
        {
            ClassStat.CurrentHp = ClassStat.MaxHp;
        }

        //Debug.Log(ClassStat.CurrentHp + "/" + ClassStat.MaxHp + " HP");

        GameManager.instance.PlayerTookDamage(playerID, finaldamage, ClassStat);

        if (ClassStat.CurrentHp <= 0.0f)
        {
            //death
            NetworkingManager.Unsubscribe(DataType.Player, playerID);
            Destroy(gameObject);
        }

        return finaldamage;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (playerID != GameData.MyPlayer.PlayerID)
            return;

        Trigger attack;
        if ((attack = other.gameObject.GetComponent<Trigger>()) != null) {
            if (attack.teamID == team) {
                return;
            }

            var damageTaken = doDamage(attack.damage);

            if (attack.IsDestroyable)
                Destroy(other.gameObject);

            var memersToSend = new List<Pair<string, string>>();
            memersToSend.Add(new Pair<string, string>("EnemyID", attack.playerID.ToString()));
            memersToSend.Add(new Pair<string, string>("Damage", damageTaken.ToString()));
            NetworkingManager.send_next_packet(DataType.Hit, GameData.MyPlayer.PlayerID, memersToSend, Protocol.TCP);
            NetworkingManager.send_next_packet(DataType.TriggerKilled, attack.triggerID, new List<Pair<string, string>>(), Protocol.TCP);

            return;
        } else {
            Debug.Log("Attack was null");
        }

    }

    void receiveAttackFromServer(JSONClass playerData)
    {
        if (playerData["ID"].AsInt == GameData.MyPlayer.PlayerID)
            return;
        Vector2 directionOfAttack = new Vector2(playerData["DirectionX"].AsFloat, playerData["DirectionY"].AsFloat);
        switch (playerData["Attack"].AsInt)
        {
            case 0:
                HUD_Manager.instance.UseMainSkill(cooldowns[0]);
                basicAttack(directionOfAttack);
                //Regular attack
                break;
            case 1:
                HUD_Manager.instance.UseSubSkill(cooldowns[1]);
                specialAttack(directionOfAttack);
                //Regular special attack
                break;
            case 2:
                //Aman special attack
                break;
            default:
                break;
        }
    }

    public virtual float basicAttack(Vector2 dir)
    {
        HUD_Manager.instance.UseMainSkill(cooldowns[0]);
        return cooldowns[0];
    }

    public virtual float specialAttack(Vector2 dir)
    {
        HUD_Manager.instance.UseSubSkill(cooldowns[1]);
        return cooldowns[1];
    }

    [System.Serializable]
	public class PlayerBaseStat
	{
		public float CurrentHp;
		public float MaxHp;
		public float MoveSpeed;
		public float AtkPower;
        public float Defense;
        //TODO: defensive stats, etc.
	}
}
